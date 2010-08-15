using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Base implementation in order to check the integrity of the database
    /// </summary>
    public abstract class BaseIntegrityChecker : IIntegrity
    {
        /// <summary> Used Type Mapper </summary>
        public ITypeMapper TypeMapper { get; private set; }

        /// <summary>Database Schema </summary>
        public string DatabaseSchema { get; private set; }

        /// <summary>Native Persister </summary>
        public INativePersister NativePersister { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected BaseIntegrityChecker(INativePersister persister, ITypeMapper typeMapper, string databaseSchema)
        {
            NativePersister = persister;
            TypeMapper = typeMapper;
            DatabaseSchema = databaseSchema;
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
        /// Gets the schema table.
        /// </summary>
        protected static DataTable GetSchemaTable(IDataReader reader)
        {
            return reader.GetSchemaTable();
        }

        #region Integrity checks

        /// <summary>
        /// Checks the integrity of a current database and compares the meta model with the persistent types.
        /// </summary>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public virtual IEnumerable<IntegrityInfo> CheckIntegrity(IEnumerable<Type> persistentTypes, ObjectMapper mapper)
        {
            ITransactionContext transactionContext = mapper.TransactionContext;
            var result = new List<IntegrityInfo>();
            foreach (Type type in persistentTypes)
            {
                if (
                    Table.GetTableInstance(type).IsAccessible(transactionContext.DatabaseMajorVersion,
                                                              transactionContext.DatabaseMinorVersion) == false)
                    continue;

                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                result.AddRange(CheckIntegrity(new IntegrityInfo(type, projection.GetFieldTemplates(false))));
            }

            return result;
        }

        /// <summary>
        /// Gets the integrity of an object type.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        protected virtual IEnumerable<IntegrityInfo> CheckIntegrity(IntegrityInfo info)
        {
            var resultList = new List<IntegrityInfo> { info };

            /*
			 * Check if the table does exist
			 */
            string sql = string.Concat("SELECT * FROM ", ConcatedSchema, TypeMapper.Quote(info.TableName));

            IDataReader reader;
            try
            {
                IDbCommand sqlCommand = NativePersister.CreateCommand();
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
            }
            catch (Exception)
            {
                info.TableExists = false;
                return resultList;
            }

            /*
			 * Get all columns of the table
			 */
            Dictionary<string, int> fieldIndexDict;
            Dictionary<int, string> indexFieldDict;
            NativePersister.GetColumns(reader, info.Fields, out fieldIndexDict, out indexFieldDict);

            DataTable schemaTable = GetSchemaTable(reader);

            Dictionary<string, FieldDescription> fields = info.Fields;
            reader.Close();

            /*
			 * Enumerate all fields of the object
			 */
            var fieldEnumerator = fields.GetEnumerator();
            while (fieldEnumerator.MoveNext())
            {
                var fieldDescription = fieldEnumerator.Current.Value;
                if (fieldDescription == null)
                    continue;

                /*
				 * we want to validate a field
				 */
                if ((fieldDescription.FieldType.Equals(typeof(Field)))
                    || (fieldDescription.FieldType.Equals(typeof(Link)))
                    || (fieldDescription.FieldType.Equals(typeof(SpecializedLink))))
                {
                    /*
                     * If there are no property informations, than continue;
                     */
                    if (fieldDescription.CustomProperty == null)
                        continue;

                    PropertyMetaInfo metaInfo = fieldDescription.CustomProperty.MetaInfo;

                    /*
                     * Fields that have a select function aren't persistent
                     */
                    if (metaInfo.SelectFunction.IsNotNullOrEmpty())
                        continue;

                    /*
                     * Check missing properties
                     */
                    if (fieldIndexDict.ContainsKey(fieldEnumerator.Current.Key) == false)
                    {
                        info.MismatchedFields.Add(new FieldIntegrity(fieldDescription));
                        continue;
                    }

                    /*
                     * Get Column
                     */
                    bool uniqueFailure = false;
                    bool requiredFailure = false;
                    bool typeFailure = false;
                    bool fieldIsShorter = false;
                    bool fieldIsLonger = false;

                    DataRow[] row = schemaTable.Select(string.Concat("ColumnName='", fieldEnumerator.Current.Key, "'"));
                    if (row.Length == 0)
                        continue;

                    DataRow columnDescription = row[0];


                    /*
                         * Check for GeneralLink Attribute and the required #typ field
                         */
                    if (metaInfo.IsGeneralLinked)
                    {
                        string typeField = string.Concat(fieldEnumerator.Current.Key, DBConst.TypAddition);
                        if (!fieldIndexDict.ContainsKey(typeField))
                        {
                            var typeFieldDescription =
                                new FieldDescription(typeField, fieldDescription.ParentType, typeof(string), false);
                            info.MismatchedFields.Add(new FieldIntegrity(typeFieldDescription));
                        }
                    }

                    /*
        				 * Check all
						 */
                    if (columnDescription["IsUnique"] != DBNull.Value)
                    {
                        uniqueFailure = ((bool)columnDescription["IsUnique"] != metaInfo.IsUnique);

                        /*
                             * Check if we really have an unique failure
                             * When using combined unique keys, .NET does not retrieve the correct value
                             */
                        if (uniqueFailure && metaInfo.IsUnique)
                        {
                            bool hasDefaultGroup = false;
                            foreach (KeyGroup group in metaInfo.UniqueKeyGroups)
                                if (group.Number == 0)
                                {
                                    hasDefaultGroup = true;
                                    break;
                                }

                            if (!hasDefaultGroup)
                                uniqueFailure = false;
                        }
                    }

                    if (columnDescription["AllowDBNull"] != DBNull.Value)
                    {
                        bool primaryKey = metaInfo.IsPrimaryKey;
                        bool typeRequires = TypeMapper.GetStringForDDL(fieldDescription).IndexOf("NOT NULL") >= 0;
                        bool required = metaInfo.IsRequiered || primaryKey || typeRequires;
                        requiredFailure = ((bool)columnDescription["AllowDBNull"] == required);
                    }

                    Type checkType = TypeMapper.GetTypeForDatabase(TypeHelper.GetBaseType(fieldDescription.ContentType));
                    if (columnDescription["DataType"] != DBNull.Value)
                    {
                        typeFailure = (columnDescription["DataType"] != checkType);

                        // correct char to string type failure
                        if (typeFailure && columnDescription["DataType"] == typeof(string) &&
                            checkType == typeof(char))
                            typeFailure = false;
                    }

                    int size = metaInfo.IsUnicode
                                   ? CalculateUnicodeSize((int) columnDescription["ColumnSize"])
                                   : CalculateSize((int)columnDescription["ColumnSize"]);

                    if (fieldDescription.ContentType.Equals(typeof(string)))
                        fieldIsShorter = (metaInfo.Length < size);

                    if (fieldDescription.ContentType.Equals(typeof(string)))
                        fieldIsLonger = (metaInfo.Length > size);

                    if (uniqueFailure || requiredFailure || typeFailure || fieldIsShorter || fieldIsLonger)
                    {
                        var fieldIntegrity = new FieldIntegrity(fieldDescription, uniqueFailure, requiredFailure,
                                                                typeFailure, fieldIsShorter, fieldIsLonger);
                        info.MismatchedFields.Add(fieldIntegrity);
                    }
                }

                /*
				 * We want to validate a link
				 */
                if (fieldDescription.FieldType.Equals(typeof(ListLink)))
                {
                    string subTable = string.Concat(Table.GetTableInstance(info.ObjectType).DefaultName, "_",
                                                    fieldEnumerator.Current.Key);
                    Type linkedPrimaryKey = fieldDescription.CustomProperty.MetaInfo.LinkedPrimaryKeyType;
                    bool generalLinked = fieldDescription.CustomProperty.MetaInfo.IsGeneralLinked;

                    if (fieldDescription.ContentType.IsListType())
                        resultList.AddRange(
                            CheckIntegrity(new IntegrityInfo(subTable,
                                                             ListLink.GetListTemplates(info.ObjectType, generalLinked,
                                                                                       linkedPrimaryKey))));
                    else if (fieldDescription.ContentType.IsDictionaryType())
                        resultList.AddRange(
                            CheckIntegrity(new IntegrityInfo(subTable,
                                                             ListLink.GetHashTemplates(typeof(string), info.ObjectType,
                                                                                       generalLinked, linkedPrimaryKey))));
                }
            }

            /*
			 * Enumerate all table columns
			 */
            var columnEnumerator = fieldIndexDict.GetEnumerator();
            while (columnEnumerator.MoveNext())
            {
                var column = columnEnumerator.Current.Key;
                bool remove = !fields.ContainsKey(column);

                if (!remove)
                {
                    var fieldDescription = fields[column];
                    remove = fieldDescription.FieldType.Equals(typeof(ListLink));
                }
                else
                {
                    /*
                     * Check for GeneralLink attributes
                     */
                    if (column.EndsWith(DBConst.TypAddition))
                    {
                        string mainColumn = column.Substring(0, column.Length - DBConst.TypAddition.Length);
                        var mainField = fields[mainColumn];
                        remove = !((mainField != null)
                                   && (mainField.CustomProperty != null)
                                   && (mainField.CustomProperty.MetaInfo.IsGeneralLinked));
                    }
                }

                /*
				 * Check unmatched properties
				 */
                if (remove)
                    info.MismatchedFields.Add(new FieldIntegrity(column));
            }

            return resultList;
        }

        /// <summary>
        /// Calculates the size of the unicode.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected virtual int CalculateUnicodeSize(int size)
        {
            return size;
        }

        /// <summary>
        /// Calculates the size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected virtual int CalculateSize(int size)
        {
            return size;
        }

        #endregion

    }
}
