using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Oracle
{
    /// <summary>
    /// Oracle Repository Based Schema Writer
    /// </summary>
    public class OracleRepositorySchemaWriter : OracleSchemaWriter
    {
        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>The mapper.</value>
        public ObjectMapper Mapper { get; set;}

        /// <summary>
        /// Creates the Oracle Repository Schema Writer
        /// </summary>
        /// <param name="typeMapper"></param>
        /// <param name="databaseSchema"></param>
        public OracleRepositorySchemaWriter(ITypeMapper typeMapper, string databaseSchema) 
            : base(typeMapper, databaseSchema)
        {
        }

        /// <summary>
        /// Export a database schema file.
        /// </summary>
        /// <param name="schemaFile">File name for the schema export</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        public override void WriteSchema(string schemaFile, IEnumerable<Type> persistentTypes)
        {
            var tables = new Hashtable();
            var sql = new StringBuilder();
            string tablename;
            string tableSql;
            var uniqueConstraintCount = new Hashtable();
            var uniqueConstraints = new Hashtable();

            /*
			 * Write Drop Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- DROP TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type) enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);
                tablename = TypeMapper.DoCasing(Table.GetTableInstance(type).DefaultName);

                Dictionary<string, FieldDescription> fields = projection.GetFieldTemplates(false);
                Dictionary<string, FieldDescription>.Enumerator fieldEnumerator = fields.GetEnumerator();
                while (fieldEnumerator.MoveNext())
                {
                    FieldDescription field = fieldEnumerator.Current.Value;

                    if ((field != null) && (field.FieldType.Equals(typeof (ListLink))))
                        sql.Append("DROP TABLE " + tablename + "_" + field.Name + ";\n");
                }
                sql.Append("DROP TABLE " + tablename + ";\n");
            }
            sql.Append("\n");

            /*
			 * Write Create Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type) enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);
                tablename = TypeMapper.DoCasing(Table.GetTableInstance(type).DefaultName);


                if (!tables.ContainsKey(tablename))
                {
                    SetTablespaceForType(tablename);
                    tableSql = GetTableSql(
                        tablename,
                        type,
                        projection.GetFieldTemplates(false));
                    sql.Append(tableSql);

                    tables.Add(tablename, tablename);
                }
            }
            sql.Append("\n");

            /*
			 * Write Unique Constraints
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE UNIQUE CONSTRAINTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type) enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);
                tablename = TypeMapper.DoCasing(Table.GetTableInstance(type).DefaultName);

                SetTablespaceForType(Table.GetTableInstance(type).DefaultName);
                tableSql = GetUniqueConstraintSql(
                    tablename,
                    projection.GetFieldTemplates(false), uniqueConstraintCount,
                    uniqueConstraints);
                sql.Append(tableSql);
            }

            /*
			 * Write Unique Constraints for Virtual Fields
			 */
            sql.Append("\n");
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE UNIQUE KEY CONSTRAINTS FOR VIRTUAL LINKS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type) enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);

                tableSql = GetUniqueIndexForVirtualLinksSql(
                    type,
                    projection.GetFieldTemplates(false), uniqueConstraintCount,
                    uniqueConstraints);

                sql.Append(tableSql);
            }

            StreamWriter writer = File.CreateText(schemaFile);
            writer.Write(sql.ToString());
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// This method extracts the unique inndex constraint columns of a table that links through a virtual link to other tables and create the sql for it
        /// </summary>
        /// <param name="objectType">Type of the object from which the virtual fields shall obtained.</param>
        /// <param name="fieldTemplates">field templates</param>
        /// <param name="uniqueConstraintCount">count the foreign key constraints</param>
        /// <param name="uniqueConstraints">The unique constraints.</param>
        /// <returns></returns>
        protected String GetUniqueIndexForVirtualLinksSql(Type objectType,
                                                          IDictionary fieldTemplates, Hashtable uniqueConstraintCount,
                                                          Hashtable uniqueConstraints)
        {
            var resultSql = new StringBuilder();
            foreach (PropertyInfo info in objectType.GetProperties())
            {
                /*
				 * Is there a virtual field?
				 */
                VirtualLinkAttribute virtualLink = ReflectionHelper.GetVirtualLinkInstance(info);
                if ((virtualLink != null) && (virtualLink.JoinFieldForGlobalParameter != null))
                {
                    string tableName = TypeMapper.DoCasing(Table.GetTableInstance(virtualLink.LinkedClass).DefaultName);
                    int constraintNumber = (uniqueConstraintCount[tableName] == null)
                                               ? 1
                                               : (int) uniqueConstraintCount[tableName];

                    string fieldForGlobalParameter =
                        Property.GetPropertyInstance(
                            virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldForGlobalParameter)).MetaInfo.
                            ColumnName;
                    string fieldForKey =
                        Property.GetPropertyInstance(
                            virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldInLinkedClass)).MetaInfo.
                            ColumnName;
                    string constraint = string.Concat(fieldForKey, ", ", fieldForGlobalParameter);
                    if (ContainedInUniqueIdentifierList(uniqueConstraints, tableName, constraint))
                        continue;

                    SetTablespaceForType(tableName);
                    string sql = string.Concat("ALTER TABLE ", tableName, " ADD CONSTRAINT ", tableName, "_UK",
                                               constraintNumber.ToString("00"), " UNIQUE (", constraint, ")",
                                               " USING INDEX TABLESPACE ", IndexTablespace, " PCTFREE 10;\n");
                    resultSql.Append(sql);
                    uniqueConstraintCount[tableName] = ++constraintNumber;
                }
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Privates the export constraints.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="integrity">The integrity.</param>
        protected override void PrivateWriteCreateTable(StringBuilder sql, IEnumerable<IntegrityInfo> integrity)
        {
            /*
			 * Write Create Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator enumerator = integrity.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var info = (IntegrityInfo) enumerator.Current;
                if (info.TableExists)
                    continue;

                SetTablespaceForType(info.TableName);

                string tableSql = GetTableSql(info.TableName, info.ObjectType, info.Fields);
                sql.Append(tableSql);
            }
            sql.Append("\n");
        }

        /// <summary>
        /// Privates the write create table.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="persistentTables">The persistent tables.</param>
        /// <param name="persistentTypes">The persistent types.</param>
        protected override void PrivateWriteCreateTable(StringBuilder sql, List<string> persistentTables, IEnumerable<Type> persistentTypes)
        {
            /*
			 * Write Create Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            var enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);

                string tablename = TypeMapper.DoCasing(Table.GetTableInstance(type).DefaultName);
                if (!persistentTables.Contains(tablename))
                    continue;

                SetTablespaceForType(tablename);
                string tableSql = GetTableSql(tablename, type, projection.GetFieldTemplates(false));
                sql.Append(tableSql);
            }
            sql.Append("\n");
        }

        /// <summary>
        /// This method returns
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected virtual void SetTablespaceForType(string tableName)
        {
            tableName = TypeMapper.DoCasing(tableName);

            ICondition condition = new ConditionList();

            condition.Add(new AndCondition(typeof (TableStorage), "TableName", QueryOperator.Like_NoCaseSensitive,tableName));
            condition.Add(new AndCondition(typeof (TableStorage), "Application", QueryOperator.Like_NoCaseSensitive,Mapper.ApplicationName));

            IDictionary globalApplicationParameter = new HybridDictionary();
            globalApplicationParameter["!!APP!!"] = Mapper.ApplicationName;

            var storage = (TableStorage) Mapper.Load(typeof (TableStorage), condition, globalApplicationParameter);

            DataTablespace = storage.DataTablespace;
            IndexTablespace = storage.IndexTablespace;
        }
    }
}
