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

namespace AdFactum.Data.Postgres
{
    public class PostgresSchemaWriter : BaseSchemaWriter
    {
        HashSet<Type> enumTypes = new HashSet<Type>();

        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresSchemaWriter(ITypeMapper typeMapper, string databaseSchema) 
            : base(typeMapper, databaseSchema)
        {
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
                        string tablename = Table.GetTableInstance(type).Name;
                        tables.Add(tablename);

                        sql.Append(string.Concat("DROP SEQUENCE IF EXISTS ", TypeMapper.Quote(tablename+ "_seq") + ";\n"));
                    }
                }
                catch(NoPrimaryKeyFoundException)
                {
                    // Do nothing in that case
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
        /// Gets the unique uniqueConstraint SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="constraintNumber">The uniqueConstraint number.</param>
        /// <param name="uniqueConstraint">The uniqueConstraint.</param>
        /// <returns></returns>
        protected override string GetUniqueConstraintSqlStmt(string tableName, int constraintNumber, string uniqueConstraint)
        {
            string uniqueSql = string.Concat("ALTER TABLE ", TypeMapper.Quote(tableName)
                                      , " ADD CONSTRAINT ", TypeMapper.Quote(tableName+ "_UK"+ constraintNumber.ToString("00"))
                                      , " UNIQUE (", uniqueConstraint, ");\n");
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
            return string.Concat("CREATE INDEX ", TypeMapper.Quote(tableName+ "_FKI"+ indexNumber.ToString("00")), " ON ", TypeMapper.Quote(tableName), " (", TypeMapper.Quote(fieldName), ");\n\n");
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
            /*
             * Create special boolean types
             */
            string createEnum = string.Empty;
            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;
                if (field == null || !field.FieldType.Equals(typeof(Field))) continue;

                if (field.ContentType.IsEnum && !enumTypes.Contains(field.ContentType))
                {
                    Type enumType = field.ContentType;
                    createEnum += "CREATE TYPE " + TypeMapper.Quote(enumType.Name) + " AS ENUM (";
                    bool first = true;
                    foreach (var value in Enum.GetNames(enumType))
                    {
                        if (!first) createEnum += ", ";
                        createEnum += "'" + value + "'";
                        first = false;
                    }
                    createEnum += ");\n";

                    enumTypes.Add(enumType);
                }
            }

            /*
             * Evaluate the table script
             */
            string tableSql = base.GetTableSql(tableName, parentType, fieldTemplates);
            tableSql = tableSql.Substring(0, tableSql.Length - 3);

            return createEnum+tableSql;
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
                resultSql.Append(string.Concat(", ", "CONSTRAINT " + TypeMapper.Quote(tableName + "_pk") +" PRIMARY KEY", "("));
                bool first = true;
                while (primaryEnumerator.MoveNext())
                {
                    if (!first) resultSql.Append(", ");
                    resultSql.Append(TypeMapper.Quote(((FieldDescription)primaryEnumerator.Current).Name));
                    first = false;
                }
                resultSql.Append(")");
            }

            resultSql.Append(");\n");

            /*
             * If it's a single primary key and the type of that key is autoincrement
             * we've to create a sequence and trigger logic;
             */
            if (primaries.Count == 1)
            {
                FieldDescription primaryKey = primaries[0];
                if (primaryKey.IsAutoIncrement)
                {
                    resultSql.Append(string.Concat("CREATE SEQUENCE ", TypeMapper.Quote(tableName+ "_seq")+ " INCREMENT BY 1 NO MAXVALUE START WITH 1;\n"));
                    resultSql.Append(
                        string.Concat("ALTER TABLE ", TypeMapper.Quote(tableName), " ALTER COLUMN ", TypeMapper.Quote(primaryKey.Name), " SET DEFAULT NEXTVAL('",TypeMapper.Quote(tableName+"_seq")+"');\n"));
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
            sql.Append(TypeMapper.Quote(info.TableName));
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
            sql.Append(TypeMapper.Quote(info.TableName));
            sql.Append(" MODIFY ");
            sql.Append(ddl);
            sql.Append(";\n");
        }

        /// <summary>
        /// Write the drop table statement
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

                string tablename = Table.GetTableInstance(type).Name;
                tables.Add(tablename);

                var projection = ReflectionHelper.GetProjection(type, null);
                var fields = projection.GetFieldTemplates(false);
                var fieldEnumerator = fields.GetEnumerator();
                while (fieldEnumerator.MoveNext())
                {
                    var field = fieldEnumerator.Current.Value;

                    if ((field != null) && (field.FieldType.Equals(typeof(ListLink))))
                        sql.Append(string.Concat("DROP TABLE IF EXISTS ", TypeMapper.Quote(CreateChildTableName(tablename, field.Name)), ";\n"));
                }
                sql.Append(string.Concat("DROP TABLE IF EXISTS ", TypeMapper.Quote(tablename), ";\n"));
            }
            sql.Append("\n");
        }
    }
}
