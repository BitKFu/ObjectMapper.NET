using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Oracle
{
    /// <summary>
    /// Oracle Schema Writer
    /// </summary>
    public class OracleSchemaWriter : BaseSchemaWriter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeMapper"></param>
        /// <param name="databaseSchema"></param>
        public OracleSchemaWriter(ITypeMapper typeMapper, string databaseSchema) 
            : base(typeMapper, databaseSchema)
        {
        }

        private string dataTablespace = "USERS";

        /// <summary>
        /// Accessor for the data tablespace
        /// </summary>
        public string DataTablespace
        {
            get { return dataTablespace; }
            set { dataTablespace = value; }
        }

        private string indexTablespace = "USERS";

        /// <summary>
        /// Accessor for the index tablespace
        /// </summary>
        public string IndexTablespace
        {
            get { return indexTablespace; }
            set { indexTablespace = value; }
        }

        /// <summary>
        /// Export a database schema file.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        public override void WriteSchema(TextWriter outputStream, IEnumerable<Type> persistentTypes)
        {
            var sql = new StringBuilder();
            var tables = new ArrayList();

            /*
             * Write Drop Table
             */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- DROP SEQUENCE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type)enumerator.Current;
                var projection = ReflectionHelper.GetProjection(type, null);

                try
                {
                    FieldDescription primaryKey = projection.GetPrimaryKeyDescription();

                    if (primaryKey.IsAutoIncrement)
                    {
                        string tablename = Table.GetTableInstance(type).DefaultName;
                        tables.Add(tablename);

                        sql.Append(string.Concat("DROP SEQUENCE ", tablename, "_SEQ;\n"));
                    }
                }
                catch(NoPrimaryKeyFoundException)
                {
                    // Do nothing. We can't drop a sequence, if no primary key could be found
                }
            }
            sql.Append("\n");

            /*
             * Write To Stream
             */
            outputStream.Write(sql.ToString());
            outputStream.Flush();

            base.WriteSchema(outputStream, persistentTypes);
        }

        /// <summary>
        /// Writes the Drop Table Statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tables"></param>
        /// <param name="persistentTypes"></param>
        protected override void PrivateWriteDropTable(StringBuilder sql, List<string> tables, IEnumerable<Type> persistentTypes)
        {
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- DROP TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type)enumerator.Current;

                string tablename = Table.GetTableInstance(type).DefaultName;
                tables.Add(tablename);

                var projection = ReflectionHelper.GetProjection(type, null);
                var fields = projection.GetFieldTemplates(false);
                var fieldEnumerator = fields.GetEnumerator();
                while (fieldEnumerator.MoveNext())
                {
                    var field = fieldEnumerator.Current.Value;

                    if ((field != null) && (field.FieldType.Equals(typeof(ListLink))))
                        sql.Append(string.Concat("DROP TABLE ", TypeMapper.Quote(CreateChildTableName(tablename, field.Name)), " CASCADE CONSTRAINTS;\n"));
                }
                sql.Append(string.Concat("DROP TABLE ", TypeMapper.Quote(tablename), " CASCADE CONSTRAINTS;\n"));
            }
            sql.Append("\n");
        }

        /// <summary>
        /// Gets the unique uniqueConstraint SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="constraintNumber">The uniqueConstraint number.</param>
        /// <param name="uniqueConstraint">The uniqueConstraint.</param>
        /// <returns></returns>
        protected override string GetUniqueConstraintSqlStmt(string tableName, int constraintNumber, string uniqueConstraint)
        {
            string uniqueSql = string.Concat("ALTER TABLE ", TypeMapper.Quote(tableName)
                                      , " ADD CONSTRAINT ", tableName, "_UK", constraintNumber.ToString("00")
                                      , " UNIQUE (", TypeMapper.Quote(uniqueConstraint), ")"
                                      , " USING INDEX TABLESPACE ", IndexTablespace, " PCTFREE 10;\n");
            return uniqueSql;
        }

        /// <summary>
        /// Gets the index SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="indexNumber">The index number.</param>
        /// <returns></returns>
        protected override string GetIndexSqlStmt(string tableName, string fieldName, int indexNumber)
        {
            return string.Concat("CREATE INDEX ", tableName, "_FKI", indexNumber.ToString("00"), " ON ", tableName, " (", fieldName, ")", " TABLESPACE ", IndexTablespace, ";\n\n");
        }

        /// <summary>
        /// Returns the sql statements to create a given table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns></returns>
        protected override String GetTableSql(String tableName, Type parentType, IDictionary fieldTemplates)
        {
            string tableSql = base.GetTableSql(tableName, parentType, fieldTemplates);
            tableSql = tableSql.Substring(0, tableSql.Length - 3);

            return tableSql;
        }

        /// <summary>
        /// Adds the primary key definition to create table SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="primaries">The primaries.</param>
        /// <param name="resultSql">The result SQL.</param>
        protected override void AddPrimaryKeyDefinitionToCreateTableSql(string tableName, List<FieldDescription> primaries, StringBuilder resultSql)
        {
            // Primarys hinzufügen
            if (primaries.Count > 0)
            {
                IEnumerator primaryEnumerator = primaries.GetEnumerator();
                resultSql.Append(string.Concat(", ", "CONSTRAINT " + tableName.ToUpper() + "_PK PRIMARY KEY", "("));
                bool first = true;
                while (primaryEnumerator.MoveNext())
                {
                    if (!first) resultSql.Append(", ");
                    resultSql.Append(TypeMapper.Quote(((FieldDescription)primaryEnumerator.Current).Name));
                    first = false;
                }
                resultSql.Append(") USING INDEX PCTFREE 5 TABLESPACE " + IndexTablespace);
            }

            resultSql.Append(") PCTFREE 10 TABLESPACE " + DataTablespace + ";\n");

            /*
             * If it's a single primary key and the type of that key is autoincrement
             * we've to create a sequence and trigger logic;
             */
            if (primaries.Count == 1)
            {
                FieldDescription primaryKey = primaries[0];
                if (primaryKey.IsAutoIncrement)
                {
                    resultSql.Append(string.Concat("CREATE SEQUENCE ", tableName, "_SEQ START WITH 1 INCREMENT BY 1 NOMAXVALUE;\n"));
                    resultSql.Append(
                        string.Concat("CREATE TRIGGER ", tableName, "_ID BEFORE INSERT ON ", tableName, " ",
                                      "FOR EACH ROW BEGIN SELECT ", tableName, "_SEQ.NEXTVAL INTO :NEW.", primaryKey.Name, " FROM DUAL; END;\n"));
                }
            }
        }

        /// <summary>
        /// Writes the integrity info required field.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected override void WriteIntegrityInfoRequiredField(FieldIntegrity fieldIntegrity, IntegrityInfo info, StringBuilder sql)
        {
            Debug.Assert(fieldIntegrity.RequiredFailure, "Only valid when RequiredFailure = true");

            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);
            sql.Append(" MODIFY ");
            sql.Append(Condition.QUOTE_OPEN);
            sql.Append(fieldIntegrity.Name);
            sql.Append(Condition.QUOTE_CLOSE);

            bool primaryKey = fieldIntegrity.Field.IsPrimary;
            bool typeRequires = TypeMapper.GetStringForDDL(fieldIntegrity.Field).IndexOf("NOT NULL") >= 0;
            bool required = fieldIntegrity.Field.CustomProperty.MetaInfo.IsRequiered || primaryKey || typeRequires;

            if (required)
                sql.Append(" NOT");
            sql.Append(" NULL");
            sql.Append(";\n");
        }

        /// <summary>
        /// Writes the integrity info field is longer.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected override void WriteIntegrityInfoTypeFailure(FieldIntegrity fieldIntegrity, IntegrityInfo info, StringBuilder sql)
        {
            string ddl = GetFieldDescriptionForDDL(fieldIntegrity.Field);
            ddl = ddl.Replace("NOT NULL", "");		// remove NOT NULL

            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);
            sql.Append(" MODIFY ");
            sql.Append(ddl);
            sql.Append(";\n");
        }

        /// <summary>
        /// Writes the integrity info field is longer.
        /// </summary>
        /// <param name="integrity">The integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected override void WriteIntegrityInfoFieldIsLonger(FieldIntegrity integrity, IntegrityInfo info, StringBuilder sql)
        {
            WriteIntegrityInfoTypeFailure(integrity, info, sql);
        }

        /// <summary>
        /// Writes the integrity info field is shorter.
        /// </summary>
        /// <param name="integrity">The integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected override void WriteIntegrityInfoFieldIsShorter(FieldIntegrity integrity, IntegrityInfo info, StringBuilder sql)
        {
            WriteIntegrityInfoTypeFailure(integrity, info, sql);
        }

    }
}
