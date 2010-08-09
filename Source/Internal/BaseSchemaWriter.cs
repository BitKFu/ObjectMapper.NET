using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Base Class used to export the database schema
    /// </summary>
    public abstract class BaseSchemaWriter : ISchemaWriter
    {
        /// <summary> Used Type Mapper </summary>
        public ITypeMapper TypeMapper { get; private set; }

        /// <summary>Database Schema </summary>
        public string DatabaseSchema { get; private set; }

        /// <summary> Gets or sets a value indicating whether [export foreign key constraints]. </summary>
        public bool ExportForeignKeyConstraints { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected BaseSchemaWriter(ITypeMapper typeMapper, string databaseSchema)
        {
            TypeMapper = typeMapper;
            DatabaseSchema = databaseSchema;
            ExportForeignKeyConstraints = true;
        }

        /// <summary>
        /// Gets the concated schema.
        /// </summary>
        /// <value>The concated schema.</value>
        protected string ConcatedSchema
        {
            get
            {
                return !string.IsNullOrEmpty(DatabaseSchema) ? string.Concat(DatabaseSchema, ".") : "";
            }
        }


        /// <summary>
        /// Writes the Database Schema to file
        /// </summary>
        public virtual void WriteSchema(string schemaFile, IEnumerable<Type> persistentTypes)
        {
            StreamWriter writer = File.CreateText(schemaFile);
            WriteSchema(writer, persistentTypes);
            writer.Close();
        }

        /// <summary>
        /// Writes the Database Schema to an output stream
        /// </summary>
        public virtual void WriteSchema(TextWriter outputStream, IEnumerable<Type> persistentTypes)
        {
            var tables = new List<string>();
            var sql = new StringBuilder();

            /*
             * Write Drop Table
             */
            PrivateWriteDropTable(sql, tables, persistentTypes);

            /*
             * Write the create table statements
             */
            PrivateWriteCreateTable(sql, tables, persistentTypes);

            /*
             * Write the create table constraints
             */
            PrivateWriteConstraints(sql, tables, persistentTypes);

            /*
             * Write To Stream
             */
            outputStream.Write(sql.ToString());
            outputStream.Flush();
        }

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        public virtual void WriteSchemaDif(string schemaFile, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrityInfos)
        {
            StreamWriter writer = File.CreateText(schemaFile);
            WriteSchemaDif(writer, persistentTypes, integrityInfos);
            writer.Close();
        }

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        public virtual void WriteSchemaDif(TextWriter outputStream, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrityInfos)
        {
            var sql = new StringBuilder();
            var tables = new ArrayList();

            /*
			 * Write the create table statements
			 */
            PrivateWriteCreateTable(sql, integrityInfos);

            /* 
			 * Write the alter table statements
			 */
            PrivateWriteAlterTable(sql, integrityInfos);

            /*
			 * Find tables to write the constraints
			 */
            IEnumerator enumerator = integrityInfos.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var info = (IntegrityInfo)enumerator.Current;
                if (!info.TableExists)
                    tables.Add(info.TableName);
            }
            PrivateWriteConstraints(sql, tables, persistentTypes);

            /*
			 * Write Schema File
			 */
            outputStream.Write(sql.ToString());
            outputStream.Flush();
        }

        /// <summary>
        /// Privates the export constraints.
        /// </summary>
        protected virtual void PrivateWriteCreateTable(StringBuilder sql, IEnumerable<IntegrityInfo> integrity)
        {
            /*
			 * Write Create Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CREATE TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            var enumerator = integrity.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var info = enumerator.Current;
                if (info.TableExists)
                    continue;

                string tableSql = GetTableSql(info.TableName, info.ObjectType, info.Fields);
                sql.Append(tableSql);
            }
            sql.Append("\n");
        }

        /// <summary>
        /// Privates the write alter table.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="integrity">The integrity.</param>
        protected virtual void PrivateWriteAlterTable(StringBuilder sql, IEnumerable<IntegrityInfo> integrity)
        {
            /*
			 * Write Alter Table
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- ALTER TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            var enumerator = integrity.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var info = enumerator.Current;
                foreach (FieldIntegrity fieldIntegrity in info.MismatchedFields)
                {
                    if (fieldIntegrity.MissingField)
                        WriteIntegrityInfoMissingField(fieldIntegrity, info, sql);

                    if (fieldIntegrity.UnmatchedField)
                        WriteIntegrityInfoUnmatchedField(fieldIntegrity, info, sql);

                    if (fieldIntegrity.RequiredFailure)
                        WriteIntegrityInfoRequiredField(fieldIntegrity, info, sql);

                    if (fieldIntegrity.FieldIsShorter)
                        WriteIntegrityInfoFieldIsShorter(fieldIntegrity, info, sql);

                    if (fieldIntegrity.FieldIsLonger)
                        WriteIntegrityInfoFieldIsLonger(fieldIntegrity, info, sql);

                    if (fieldIntegrity.TypeFailure)
                        WriteIntegrityInfoTypeFailure(fieldIntegrity, info, sql);

                    if (fieldIntegrity.UniqueFailure)
                        WriteIntegrityInfoUniqueFailure(fieldIntegrity, info, sql);
                }
            }
        }

        /// <summary>
        /// Writes the integrity info unique failure.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoUniqueFailure(FieldIntegrity fieldIntegrity, IntegrityInfo info,
                                                               StringBuilder sql)
        {
            Debug.Assert(fieldIntegrity.UniqueFailure, "Only valid when UniqueFailure = true");

            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);

            if (fieldIntegrity.Field.CustomProperty.MetaInfo.IsUnique)
                sql.Append(" ADD ");
            else
                sql.Append(" DROP ");

            sql.Append(" { Unique Key Constraint for column ");
            sql.Append(fieldIntegrity.Field.Name);
            sql.Append(" }\n");
        }

        /// <summary>
        /// Writes the integrity info type failure.
        /// </summary>
        /// <param name="integrity">The integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoTypeFailure(FieldIntegrity integrity, IntegrityInfo info,
                                                             StringBuilder sql)
        {
            WriteIntegrityInfoRequiredField(integrity, info, sql);
        }

        /// <summary>
        /// Writes the integrity info field is longer.
        /// </summary>
        /// <param name="integrity">The integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoFieldIsLonger(FieldIntegrity integrity, IntegrityInfo info,
                                                               StringBuilder sql)
        {
            WriteIntegrityInfoRequiredField(integrity, info, sql);
        }

        /// <summary>
        /// Writes the integrity info field is shorter.
        /// </summary>
        /// <param name="integrity">The integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoFieldIsShorter(FieldIntegrity integrity, IntegrityInfo info,
                                                                StringBuilder sql)
        {
            WriteIntegrityInfoRequiredField(integrity, info, sql);
        }

        /// <summary>
        /// Writes the integrity info required field.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoRequiredField(FieldIntegrity fieldIntegrity, IntegrityInfo info,
                                                               StringBuilder sql)
        {
            Debug.Assert(
                fieldIntegrity.RequiredFailure || fieldIntegrity.FieldIsShorter || fieldIntegrity.FieldIsLonger ||
                fieldIntegrity.TypeFailure,
                "Only valid when RequiredFailure || FieldIsShorter || FieldIsLonger || TypeFailure = true");

            bool primaryKey = fieldIntegrity.Field.IsPrimary;
            bool typeRequires = TypeMapper.GetStringForDDL(fieldIntegrity.Field).IndexOf("NOT NULL") >= 0;
            bool required = fieldIntegrity.Field.CustomProperty.MetaInfo.IsRequiered || primaryKey || typeRequires;

            string ddl = GetFieldDescriptionForDDL(fieldIntegrity.Field);
            ddl = ddl.Replace("NOT NULL", ""); // remove NOT NULL

            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);
            sql.Append(" ALTER COLUMN ");
            sql.Append(ddl);
            if (required)
                sql.Append(" NOT");
            sql.Append(" NULL");
            sql.Append(";\n");
        }

        /// <summary>
        /// Writes the integrity info unmatched field.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoUnmatchedField(FieldIntegrity fieldIntegrity, IntegrityInfo info,
                                                                StringBuilder sql)
        {
            Debug.Assert(fieldIntegrity.UnmatchedField, "Only valid when UnmatchedField = true");
            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);
            sql.Append(" DROP COLUMN ");
            sql.Append(Condition.QUOTE_OPEN);
            sql.Append(fieldIntegrity.Name);
            sql.Append(Condition.QUOTE_CLOSE);
            sql.Append(";\n");
        }

        /// <summary>
        /// Writes the integrity info missing field.
        /// </summary>
        /// <param name="fieldIntegrity">The field integrity.</param>
        /// <param name="info">The info.</param>
        /// <param name="sql">The SQL.</param>
        protected virtual void WriteIntegrityInfoMissingField(FieldIntegrity fieldIntegrity, IntegrityInfo info,
                                                              StringBuilder sql)
        {
            Debug.Assert(fieldIntegrity.MissingField, "Only valid when MissingField = true");
            sql.Append("ALTER TABLE ");
            sql.Append(info.TableName);
            sql.Append(" ADD ");
            sql.Append(GetFieldDescriptionForDDL(fieldIntegrity.Field));
            sql.Append(";\n");
        }

        /// <summary>
        /// Writes the Drop Table Statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tables"></param>
        /// <param name="persistentTypes"></param>
        protected virtual void PrivateWriteDropTable(StringBuilder sql, List<string> tables, IEnumerable<Type> persistentTypes)
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
                        sql.Append(string.Concat("DROP TABLE ",  TypeMapper.Quote(CreateChildTableName(tablename, field.Name)), ";\n"));
                }
                sql.Append(string.Concat("DROP TABLE ",  TypeMapper.Quote(tablename), ";\n"));
            }
            sql.Append("\n");
        }

        /// <summary>
        /// Privates the write create table.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="persistentTables">The persistent tables.</param>
        /// <param name="persistentTypes">The persistent types.</param>
        protected virtual void PrivateWriteCreateTable(StringBuilder sql, List<string> persistentTables, IEnumerable<Type> persistentTypes)
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
                string tablename = Table.GetTableInstance(type).Name;
                if (!persistentTables.Contains(tablename))
                    continue;

                var projection = ReflectionHelper.GetProjection(type, null);
                string tableSql = GetTableSql(tablename, type, projection.GetFieldTemplates(false));
                sql.Append(tableSql);
            }
            sql.Append("\n");
        }

        /// <summary>
        /// Privates the write constraints.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="persistentTables">The persistent tables.</param>
        /// <param name="persistentTypes">The persistent types.</param>
        protected virtual void PrivateWriteConstraints(StringBuilder sql, IList persistentTables, IEnumerable<Type> persistentTypes)
        {
            var uniqueConstraintCount = new Hashtable();
            var foreignKeyConstraintCount = new Hashtable();
            var uniqueConstraints = new Hashtable();

            /*
			 * Write Unique Constraints
			 */
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- CONSTRAINTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator<Type> enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Type type = enumerator.Current;
                Table tableInstance = Table.GetTableInstance(type);
                string tablename = tableInstance.Name;
                if (!persistentTables.Contains(tablename))
                    continue;

                var projection = ReflectionHelper.GetProjection(type, null);

                string tableSql = GetUniqueConstraintSql(tablename,
                                                         projection.GetFieldTemplates(false),
                                                         uniqueConstraintCount, uniqueConstraints);
                sql.Append(tableSql);

                tableSql = GetUniqueIndexForVirtualLinksSql(type,
                                                            projection.GetFieldTemplates(false),
                                                            uniqueConstraintCount, uniqueConstraints);
                sql.Append(tableSql);

                if (ExportForeignKeyConstraints)
                {
                    tableSql = GetForeignKeyForLinksSql(tablename,
                                                        projection.GetFieldTemplates(false),
                                                        foreignKeyConstraintCount);
                    sql.Append(tableSql);

                    tableSql = GetForeignKeyForCollectionsSql(tablename,
                                                              projection.GetFieldTemplates(false),
                                                              foreignKeyConstraintCount);
                    sql.Append(tableSql);

                    tableSql = GetForeignKeyForUserDefinedSql(tablename,
                                                              projection.GetFieldTemplates(false),
                                                              foreignKeyConstraintCount);
                    sql.Append(tableSql);
                }
            }
        }

        /// <summary>
        /// Gets the foreign key for user defines.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="freignKeyConstraints">The freign key constraints.</param>
        /// <returns></returns>
        protected string GetForeignKeyForUserDefinedSql(string tableName, IDictionary fieldTemplates,
                                                        Hashtable freignKeyConstraints)
        {
            List<EntityRelation> foreignKeys =
                GetForeignKeyForUserDefinedEntityRelations(null, tableName, fieldTemplates);

            var resultSql = new StringBuilder();
            int constraintNumber = (freignKeyConstraints[tableName] == null) ? 1 : (int)freignKeyConstraints[tableName];
            foreach (EntityRelation relation in foreignKeys)
            {
                string foreignKeySql =
                    GetForeignKeySqlStmt(relation.ChildTable, relation.ChildColumn,
                                         relation.ParentTable, relation.ParentColumn, constraintNumber, false);

                resultSql.Append(foreignKeySql);
                freignKeyConstraints[tableName] = ++constraintNumber;
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Gets the foreign key for user defines.
        /// </summary>
        /// <param name="versionInfo">The version info.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <returns></returns>
        protected virtual List<EntityRelation> GetForeignKeyForUserDefinedEntityRelations(VersionInfo versionInfo,
                                                                                          String tableName,
                                                                                          IDictionary fieldTemplates)
        {
            var relations = new List<EntityRelation>();
            var keyGroupConstraints = new Hashtable();

            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
                 * Is there a unique field?
                 */
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsForeignKey))
                {
                    string constraint = field.Name;

                    /*
                     * If the property contains a single unique key
                     */
                    ForeignKeyGroup defaultGroup = field.CustomProperty.GetForeignKeyDefaultGroup;
                    if (defaultGroup != null)
                    {
                        var relation = new EntityRelation();
                        relation.Initialize(versionInfo,
                                            defaultGroup.ForeignTable,
                                            defaultGroup.ForeignColumn,
                                            Table.GetTableInstance(field.ParentType).Name,
                                            constraint, EntityRelation.OrmType.Association
                            );
                        relations.Add(relation);
                    }

                    /*
                     * Step through all other key groups, and gather the fields
                     */
                    IEnumerator enumerator = field.CustomProperty.MetaInfo.ForeignKeyGroups.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var group = (ForeignKeyGroup)enumerator.Current;
                        if (group.Number > 0)
                        {
                            if (keyGroupConstraints[group.Number] == null)
                                keyGroupConstraints[group.Number] = new SortedList();

                            int position = ((SortedList)keyGroupConstraints[group.Number]).Count;
                            if (group.Ordering > 0) position = group.Ordering;
                            group.Column = constraint;
                            ((SortedList)keyGroupConstraints[group.Number]).Add(position, group);
                        }
                    }
                }
            }

            /*
             * Now add the combined unique keys
             */
            foreach (SortedList sortedConstraint in keyGroupConstraints.Values)
            {
                var constraint = new StringBuilder();
                var foreignColumns = new StringBuilder();
                string foreignTable = string.Empty;
                bool first = true;
                foreach (DictionaryEntry entry in sortedConstraint)
                {
                    if (!first) constraint.Append(", ");
                    if (!first) foreignColumns.Append(", ");
                    var group = (ForeignKeyGroup)entry.Value;

                    constraint.Append(group.Column);
                    foreignColumns.Append(group.ForeignColumn);
                    foreignTable = group.ForeignTable;
                    first = false;
                }

                var relation = new EntityRelation();
                relation.Initialize(versionInfo, 
                                    foreignTable,
                                    foreignColumns.ToString(),
                                    tableName,
                                    constraint.ToString(), EntityRelation.OrmType.Association
                    );
                relations.Add(relation);
            }

            return relations;
        }

        /// <summary>
        /// This method extracts the foreign key constraint columns of a table that links directly to other tables and create the sql for it
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldTemplates">field templates</param>
        /// <param name="freignKeyConstraints">count the foreign key constraints</param>
        /// <returns></returns>
        protected String GetForeignKeyForCollectionsSql(String tableName, IDictionary fieldTemplates,
                                                        Hashtable freignKeyConstraints)
        {
            var resultSql = new StringBuilder();
            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
				 * Is there a specialized link?
				 */
                if (field.FieldType.Equals(typeof(ListLink)))
                {
                    int foreignKeyNumber = (freignKeyConstraints[tableName] == null)
                                               ? 1
                                               : (int)freignKeyConstraints[tableName];

                    /*
					 * Set Constraint from Parent To Link Table
					 */
                    string sql = GetForeignKeySqlStmt(tableName + "_" + field.Name, DBConst.ParentObjectField, tableName,
                                                      null, foreignKeyNumber, true);
                    resultSql.Append(sql);

                    sql = GetIndexSqlStmt(tableName + "_" + field.Name, DBConst.ParentObjectField, foreignKeyNumber);
                    resultSql.Append(sql);

                    freignKeyConstraints[tableName] = ++foreignKeyNumber;

                    /*
					 * Set Constraint from Link Table to Child
					 */
                    Type childType = field.CustomProperty.MetaInfo.LinkTarget;
                    if (childType != null)
                    {
                        Table childTable = Table.GetTableInstance(childType);
                        sql = GetForeignKeySqlStmt(tableName + "_" + field.Name, DBConst.PropertyField, childTable.Name,
                                                   null,
                                                   foreignKeyNumber, field.ParentType != childType);
                        resultSql.Append(sql);

                        sql = GetIndexSqlStmt(tableName + "_" + field.Name, DBConst.PropertyField, foreignKeyNumber);
                        resultSql.Append(sql);

                        freignKeyConstraints[tableName] = ++foreignKeyNumber;
                    }
                }
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// This method extracts the foreign key constraint columns of a table that links directly to other tables and create the sql for it
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldTemplates">field templates</param>
        /// <param name="freignKeyConstraints">count the foreign key constraints</param>
        /// <returns></returns>
        protected String GetForeignKeyForLinksSql(String tableName, IDictionary fieldTemplates,
                                                  Hashtable freignKeyConstraints)
        {
            var resultSql = new StringBuilder();
            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
				 * Is there a unique field?
				 */
                if (field.FieldType.Equals(typeof(SpecializedLink)))
                {
                    int foreignKeyNumber = (freignKeyConstraints[tableName] == null)
                                               ? 1
                                               : (int)freignKeyConstraints[tableName];

                    string sql = GetForeignKeySqlStmt(tableName, field.Name,
                                                      Table.GetTableInstance(field.ContentType).Name, null,
                                                      foreignKeyNumber, false);
                    resultSql.Append(sql);

                    sql = GetIndexSqlStmt(tableName, field.Name, foreignKeyNumber);
                    resultSql.Append(sql);

                    freignKeyConstraints[tableName] = ++foreignKeyNumber;
                }
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Gets the foreign key SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="referencedTableName">Name of the referenced table.</param>
        /// <param name="referencedColumn">The referenced column.</param>
        /// <param name="foreignKeyNumber">The foreign key number.</param>
        /// <param name="onDeleteCascade">if set to <c>true</c> [on delete cascade].</param>
        /// <returns></returns>
        protected virtual string GetForeignKeySqlStmt(string tableName, string fieldName, string referencedTableName,
                                                      string referencedColumn, int foreignKeyNumber,
                                                      bool onDeleteCascade)
        {
            string sql = string.Concat("ALTER TABLE ", ConcatedSchema, TypeMapper.Quote(tableName)
                                       , " ADD CONSTRAINT ", TypeMapper.Quote(tableName + "_FK" + foreignKeyNumber.ToString("00"))
                                       , " FOREIGN KEY (", TypeMapper.Quote(fieldName), ")"
                                       , " REFERENCES ", ConcatedSchema, TypeMapper.Quote(referencedTableName));

            if (referencedColumn != null)
                sql = string.Concat(sql, " (", TypeMapper.Quote(referencedColumn), ")");

            if (onDeleteCascade)
                sql = string.Concat(sql, " ON DELETE CASCADE");

            sql = string.Concat(sql, ";\n");
            return sql;
        }


        /// <summary>
        /// Gets the index SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="indexNumber">The index number.</param>
        /// <returns></returns>
        protected virtual string GetIndexSqlStmt(string tableName, string fieldName, int indexNumber)
        {
            return string.Concat("CREATE INDEX ", tableName, "_FKI", indexNumber.ToString("00"), " ON ", tableName, " (",
                                 fieldName, ");\n");
        }

        /// <summary>
        /// This method extracts the unique inndex constraint columns of a table that links through a virtual link to other tables and create the sql for it
        /// </summary>
        /// <param name="objectType">Type of the object from which the virtual fields shall obtained.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="uniqueConstraintCount">count the foreign key constraints</param>
        /// <param name="uniqueConstraints">The unique constraints.</param>
        /// <returns></returns>
        protected String GetUniqueIndexForVirtualLinksSql(Type objectType,
                                                          Dictionary<string, FieldDescription> fieldTemplates,
                                                          Hashtable uniqueConstraintCount, Hashtable uniqueConstraints)
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
                    string tableName = Table.GetTableInstance(virtualLink.LinkedClass).Name;
                    string fieldForGlobalParameter =
                        Property.GetPropertyInstance(
                            virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldForGlobalParameter)).MetaInfo.ColumnName;
                    string fieldForKey =
                        Property.GetPropertyInstance(virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldInLinkedClass)).
                            MetaInfo.ColumnName;

                    int constraintNumber = (uniqueConstraintCount[tableName] == null)
                                               ? 1
                                               : (int)uniqueConstraintCount[tableName];

                    string constraint = string.Concat(TypeMapper.Quote(fieldForKey), ", ", TypeMapper.Quote(fieldForGlobalParameter));
                    if (ContainedInUniqueIdentifierList(uniqueConstraints, tableName, constraint))
                        continue;

                    string sql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint);

                    resultSql.Append(sql);
                    uniqueConstraintCount[tableName] = ++constraintNumber;
                }
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Returns true, if the constraint is unique for the specified table
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected static bool ContainedInUniqueIdentifierList(Hashtable source, string tableName, string constraint)
        {
            IList identifierList = GetUniqueIdentifierList(source, tableName);
            if (identifierList.Contains(constraint))
                return true;

            identifierList.Add(constraint);
            return false;
        }

        private static IList GetUniqueIdentifierList(IDictionary source, string tableName)
        {
            if (source[tableName] == null)
                source[tableName] = new ArrayList();

            return source[tableName] as IList;
        }

        /// <summary>
        /// This method extracts the unique constraint columns of a table and creates alter table sql methods
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldTemplates">field templates</param>
        /// <param name="uniqueConstraintCount">count the unique constraints</param>
        /// <param name="uniqueConstraints">The unique constraints.</param>
        /// <returns></returns>
        protected string GetUniqueConstraintSql(String tableName, IDictionary fieldTemplates,
                                                Hashtable uniqueConstraintCount, Hashtable uniqueConstraints)
        {
            var resultSql = new StringBuilder();
            string uniqueSql;
            var keyGroupConstraints = new Hashtable();
            int constraintNumber = (uniqueConstraintCount[tableName] == null)
                                       ? 1
                                       : (int)uniqueConstraintCount[tableName];

            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
				 * Is there a unique field?
				 */
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsUnique))
                {
                    string constraint = TypeMapper.Quote(field.Name);
                    if (ContainedInUniqueIdentifierList(uniqueConstraints, tableName, constraint))
                        continue;

                    /*
                     * If the property contains a single unique key
                     */
                    if (field.CustomProperty.ContainsUniqueDefaultGroup)
                    {
                        uniqueSql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint);

                        resultSql.Append(uniqueSql);
                        uniqueConstraintCount[tableName] = ++constraintNumber;
                    }

                    /*
                     * Step through all other key groups, and gather the fields
                     */
                    IEnumerator enumerator = field.CustomProperty.MetaInfo.UniqueKeyGroups.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var group = (KeyGroup)enumerator.Current;
                        if (group.Number > 0)
                        {
                            if (keyGroupConstraints[group.Number] == null)
                                keyGroupConstraints[group.Number] = new SortedList();

                            int position = ((SortedList)keyGroupConstraints[group.Number]).Count;
                            if (group.Ordering > 0) position = group.Ordering;
                            ((SortedList)keyGroupConstraints[group.Number]).Add(position, constraint);
                        }
                    }
                }
            }

            /*
             * Now add the combined unique keys
             */
            foreach (SortedList sortedConstraint in keyGroupConstraints.Values)
            {
                var constraint = new StringBuilder();
                bool first = true;
                foreach (DictionaryEntry entry in sortedConstraint)
                {
                    if (!first) constraint.Append(", ");
                    constraint.Append(entry.Value);
                    first = false;
                }

                uniqueSql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint.ToString());

                resultSql.Append(uniqueSql);
                uniqueConstraintCount[tableName] = ++constraintNumber;
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Gets the unique uniqueConstraint SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="constraintNumber">The uniqueConstraint number.</param>
        /// <param name="uniqueConstraint">The uniqueConstraint.</param>
        /// <returns></returns>
        protected virtual string GetUniqueConstraintSqlStmt(string tableName, int constraintNumber,
                                                            string uniqueConstraint)
        {
            string uniqueSql = string.Concat("ALTER TABLE ", ConcatedSchema, TypeMapper.Quote(tableName)
                                             , " ADD CONSTRAINT ", tableName, "_UK", constraintNumber.ToString("00")
                                             , " UNIQUE (", uniqueConstraint, ");\n");
            return uniqueSql;
        }

        /// <summary>
        /// Returns the sql statements to create a given table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns>String to create a table</returns>
        protected virtual String GetTableSql(String tableName, Type parentType, IDictionary fieldTemplates)
        {
            var resultSql = new StringBuilder("CREATE TABLE " + TypeMapper.Quote(tableName) + " (");
            bool first = true;
            var subSql = new StringBuilder();

            /*
			 * Alle einfachen Felder durchlaufen und als Zeilen zur Tabelle hinzufügen
			 */
            var primaries = new List<FieldDescription>();
            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
                 * Hide "non" fields
                 */
                if (field == null) continue;

                /*
                 * Hide fields with select functions
                 */
                if (field.CustomProperty != null && field.CustomProperty.MetaInfo.SelectFunction.IsNotNullOrEmpty())
                    continue;

                /*
				 * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
				 */
                string fieldString = GetFieldDescriptionForDDL(field);
                if (fieldString.Length > 0)
                {
                    if (!first) resultSql.Append(", ");
                    resultSql.Append(fieldString);
                    first = false;
                }

                /*
                 * Add Primary Keys
                 */
                if (field.IsPrimary)
                    primaries.Add(field);

                /*
				 * Ist das Objekt über einen Hash verlinkt gewesen ?
				 */
                if (field.FieldType.Equals(typeof(ListLink)))
                {
                    string subTable = string.Concat(tableName + "_" + field.Name);
                    if (field.CustomProperty != null)
                    {
                        Type linkedPrimaryKey = field.CustomProperty.MetaInfo.LinkedPrimaryKeyType;
                        bool generalLinked = field.CustomProperty.MetaInfo.IsGeneralLinked;

                        if (field.ContentType.IsListType())
                            subSql.Append(GetTableSql(subTable, parentType,
                                                      ListLink.GetListTemplates(parentType, generalLinked, linkedPrimaryKey)));
                        else if (field.ContentType.IsDictionaryType())
                            subSql.Append(GetTableSql(subTable, parentType,
                                                      ListLink.GetHashTemplates(typeof(string), parentType, generalLinked,
                                                                                linkedPrimaryKey)));
                    }
                }
            }

            AddPrimaryKeyDefinitionToCreateTableSql(tableName, primaries, resultSql);
            resultSql.Append(");\n");

            return subSql.Append(resultSql).ToString();
        }

        /// <summary>
        /// Adds the primary key definition to create table SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="primaries">The primaries.</param>
        /// <param name="resultSql">The result SQL.</param>
        protected virtual void AddPrimaryKeyDefinitionToCreateTableSql(string tableName,
                                                                       List<FieldDescription> primaries,
                                                                       StringBuilder resultSql)
        {
            // Primarys hinzufügen
            if (primaries.Count > 0)
            {
                IEnumerator primaryEnumerator = primaries.GetEnumerator();
                resultSql.Append(", PRIMARY KEY(");
                bool first = true;
                while (primaryEnumerator.MoveNext())
                {
                    if (!first) resultSql.Append(", ");
                    resultSql.Append(TypeMapper.Quote(((FieldDescription)primaryEnumerator.Current).Name));
                    first = false;
                }
                resultSql.Append(")");
            }
        }

        /// <summary>
        /// Gets the field description for DDL.
        /// </summary>
        protected virtual string GetFieldDescriptionForDDL(FieldDescription field)
        {
            string result = "";
            string contentType;

            /*
			 * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
			 */
            if (field.FieldType.Equals(typeof(Field)))
            {
                var columnType = new StringBuilder();

                if (field.CustomProperty != null)
                {
                    contentType = TypeMapper.GetStringForDDL(field);
                    columnType.Append(contentType);
                    if ((field.CustomProperty.MetaInfo.IsRequiered) && (!contentType.EndsWith("NOT NULL")))
                        columnType.Append(" NOT NULL");
                }
                else
                {
                    contentType = TypeMapper.GetStringForDDL(field);
                    columnType.Append(contentType);
                }

                result = string.Concat(TypeMapper.Quote(field.Name), " ", columnType);
            }

            /*
			 * Link
			 */
            if (field.FieldType.Equals(typeof(Link)))
            {
                contentType = TypeMapper.GetStringForDDL(field);
                result = string.Concat(" ", TypeMapper.Quote(field.Name), " ", contentType);
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsRequiered) &&
                    (!contentType.EndsWith("NOT NULL")))
                    result = string.Concat(result, " NOT NULL");

                string addField = field.Name + DBConst.TypAddition;
                result = string.Concat(result, ", ", TypeMapper.Quote(addField), " ",
                                       TypeMapper.GetStringForDDL(new FieldDescription(addField, field.ContentType,
                                                                                       typeof(string), false)));
            }

            /*
			 * Specialized Link
			 */
            if (field.FieldType.Equals(typeof(SpecializedLink)))
            {
                contentType = TypeMapper.GetStringForDDL(field);
                result = string.Concat(" ", TypeMapper.Quote(field.Name), " ", contentType);
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsRequiered) &&
                    (!contentType.EndsWith("NOT NULL")))
                    result = string.Concat(result, " NOT NULL");
            }

            /*
             * Add "Auto Increment" for Integer Primary Keys
             */
            if (field.IsAutoIncrement)
                result = string.Concat(result, " ", TypeMapper.AutoIncrementIdentifier);

            return result;
        }

        /// <summary>
        /// Creates the name of the child table.
        /// </summary>
        protected static string CreateChildTableName(string tableName, string child)
        {
            return string.Concat(tableName, "_", child);
        }
    }
}
