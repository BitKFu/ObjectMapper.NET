using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Xml
{
    /// <summary>
    /// Defines a xml persister
    /// </summary>
    [Serializable]
    public class XmlPersister : IPersister, ISchemaWriter
    {
        /// <summary>
        /// Database name
        /// </summary>
        private readonly String dbName;

        /// <summary>
        /// XML File that is used for storing xml data.
        /// </summary>
        private readonly String dbPath;

        /// <summary>
        /// XSD File that is used for storing the xsd schema data.
        /// </summary>
        private String dbSchema;

        /// <summary>
        /// Dataset
        /// </summary>
        private DataSet dataSet;

        /// <summary>
        /// XML Persister Constructor
        /// </summary>
        /// <param name="pDbName">Database Name (not the file name)</param>
        /// <param name="pDbPath">XML File that is used for storing xml data.</param>
        /// <param name="pSchemaName">XSD File that is used for storing the xsd schema data.</param>
        public XmlPersister(String pDbName, String pDbPath, String pSchemaName)
        {
            dbName = pDbName;
            dbPath = pDbPath;
            DatabaseSchema = pSchemaName;

            ReadXml();
        }

        /// <summary>
        /// Loads the data from a xml file into a dataset
        /// </summary>
        private void ReadXml()
        {
            dataSet = new DataSet(dbName) {Locale = CultureInfo.InvariantCulture};

            /*
			 * Wenn ein Schema Name angegeben wird, dann sollte auch einer
			 * vorhanden sein
			 */
            if (DatabaseSchema != null)
            {
                if (File.Exists(DatabaseSchema))
                    dataSet.ReadXmlSchema(DatabaseSchema);

                if (File.Exists(dbPath))
                    dataSet.ReadXml(dbPath, XmlReadMode.ReadSchema);
            }
            else if (File.Exists(dbPath))
                dataSet.ReadXml(dbPath, XmlReadMode.ReadSchema);
        }

        /// <summary>
        /// XML Persister Constructor
        /// </summary>
        /// <param name="pDbName">Database Name (not the file name)</param>
        /// <param name="pDbPath">XML File name that is used for storing xml data</param>
        public XmlPersister(String pDbName, String pDbPath)
            : this(pDbName, pDbPath, null)
        {
        }

        private delegate void PersistDelegate(IDictionaryEnumerator enumerator);

        /// <summary>
        /// Private method to store a dataset 
        /// </summary>
        /// <param name="objectType">Table name</param>
        /// <param name="id">Objekt-ID</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="parent">Parent Datarow</param>
        /// <param name="update">true, if the data is only updated</param>
        private void Persist(String objectType, Object id, PersistentProperties fields,
                             Dictionary<string, FieldDescription> fieldTemplates, DataRow parent, Boolean update)
        {
            var isNew = false;

            /*
			 * Beinhaltet das Dataset die Tabelle des Objekt-Typs ? 
			 */
            if ((DatabaseSchema == null) && (!dataSet.Tables.Contains(objectType)))
            {
                var desc = GetPrimaryKeyDescription(fieldTemplates);
                CreateTable(objectType, desc.ParentType, fieldTemplates, dataSet);
            }

            var table = dataSet.Tables[objectType];

            /*
			 * Existiert die Zeile bereits ? 
			 */
            DataRow newRow = null;
            if (update)
            {
                string primaryKeyColumn = GetPrimaryKeyColumn(fieldTemplates);

                if (parent != null)
                    newRow =
                        table.Rows.Find(new[]
                                            {
                                                parent.ItemArray.GetValue(parent.Table.Columns.IndexOf(primaryKeyColumn)),
                                                id
                                            });
                else
                    newRow = table.Rows.Find(id);
            }

            /*
			 * Falls der Eintrag noch nicht existiert, dann einen anlegen
			 */
            if (newRow == null)
            {
                newRow = table.NewRow();
                isNew = true;

                if (parent != null)
                    newRow.SetParentRow(parent);
            }

            newRow.BeginEdit();

            /*
			 * Jetzt die Werte hinzufügen
			 */
            if (fields.FieldProperties != null)
            {
                var enumerator = fields.FieldProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    /*
                     * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
                     */
                    if (enumerator.Current.Value is Field)
                    {
                        var field = (Field) enumerator.Current.Value;
                        if (field.IsModified)
                        {
                            newRow[field.Name] = GetNv(field.Value);
                        }

                        continue;
                    }


                    /*
                     * ... oder eine Verknüpfung zu einem anderen VO
                     */
                    if (enumerator.Current.Value is Link)
                    {
                        var link = (Link) enumerator.Current.Value;
                        if (link.IsModified)
                        {
                            newRow[link.Property.Name] = GetNv(link.Property.Value);
                            newRow[link.LinkedTo.Name] = GetNv(link.LinkedTo.Value);
                        }

                        continue;
                    }

                    /*
                     * ... oder eine Verknüpfung zu einem anderen VO
                     */
                    if (enumerator.Current.Value is SpecializedLink)
                    {
                        var link = (SpecializedLink) enumerator.Current.Value;
                        if (link.IsModified)
                        {
                            newRow[link.Property.Name] = GetNv(link.Property.Value);
                        }

                        continue;
                    }
                }
            }

            newRow.EndEdit();
            if (isNew)
                table.Rows.Add(newRow);

            /*
			 * Nochmal durchlaufen und nur die über Linklisten verknüpften Objekte speichern
			 */
            PersistDelegate persist = delegate(IDictionaryEnumerator enumerator1)
              {
                  while (enumerator1.MoveNext())
                  {
                      /*
                       * Dictionary Link ?
                       */
                      var propertyList = (IDictionary) enumerator1.Value;
                      IDictionaryEnumerator listEnumerator = propertyList.GetEnumerator();
                      while (listEnumerator.MoveNext())
                      {
                          var dictionaryLink = (ListLink) listEnumerator.Entry.Value;
                          string tableName = objectType + "_" + enumerator1.Key;

                          if (dictionaryLink.Key != null)
                          {
                              /*
                               * Falls das Objekt gelöscht wurde, dann löschen
                               */
                              if (dictionaryLink.IsDeleted)
                              {
                                  Delete(tableName, dictionaryLink.Key.Value, newRow, fieldTemplates);
                                  continue;
                              }

                              /*
                               * Falls das Objekt keinen Wert besitzt, dann weiter mit dem nächsten
                               */
                              if ((!dictionaryLink.HasValue())
                                  || (!dictionaryLink.IsModified))
                                  continue;

                              Persist(
                                  tableName,
                                  dictionaryLink.Key.Value.ToString(),
                                  dictionaryLink.Fields(this),
                                  dictionaryLink.GetTemplates(),
                                  newRow,
                                  update);
                          }
                          else
                          {
                              /*
                               * Falls das Objekt gelöscht wurde, dann löschen
                               */
                              if (dictionaryLink.IsDeleted)
                              {
                                  Delete(tableName, dictionaryLink.Property.Value, newRow, fieldTemplates);
                                  continue;
                              }

                              /*
                               * Falls das Objekt keinen Wert besitzt, dann weiter mit dem nächsten
                               */
                              if ((!dictionaryLink.HasValue())
                                  || (!dictionaryLink.IsModified))
                                  continue;

                              Persist(
                                  tableName,
                                  dictionaryLink.Property.Value.ToString(),
                                  dictionaryLink.Fields(this),
                                  dictionaryLink.GetTemplates(),
                                  newRow,
                                  update);
                          }
                      }
                  }
              };

            if (fields.DictProperties != null)
                persist(fields.DictProperties.GetDictionaryEnumerator());

            if (fields.ListProperties != null)
                persist(fields.ListProperties.GetDictionaryEnumerator());
        }


        /// <summary>
        /// Method to insert a new row to database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        public object Insert(String tableName, PersistentProperties fields, Dictionary<string, FieldDescription> fieldTemplates)
        {
            object id = null;

            /*
             * Search the Primary Key
             */
            var enumerator = fields.FieldProperties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var field = enumerator.Current.Value as Field;
                if (field == null || !field.FieldDescription.IsPrimary) continue;

                id = field.Value;
                break;
            }

            Persist(tableName, id, fields, fieldTemplates, null, false);
            return id;
        }

        /// <summary>
        /// Method to update an existing row in database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        public void Update(String tableName, PersistentProperties fields, Dictionary<string, FieldDescription> fieldTemplates)
        {
            object id = null;

            /*
             * Search the Primary Key
             */
            var enumerator = fields.FieldProperties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var field = enumerator.Current.Value as Field;
                if (field == null || !field.FieldDescription.IsPrimary) continue;

                id = field.Value;
                break;
            }

            Persist(tableName, id, fields, fieldTemplates, null, true);
        }

        /// <summary>
        /// Method to delete an existing row from database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="id">Primary key</param>
        /// <param name="fieldTemplates">Field description.</param>
        public void Delete(String tableName, Object id, Dictionary<string, FieldDescription> fieldTemplates)
        {
            Delete(tableName, id, null, fieldTemplates);
        }

        /// <summary>
        /// Method to delete an existing row from database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="id">Primary key</param>
        /// <param name="parent">Parent Datarow</param>
        /// <param name="fieldTemplates">Field description.</param>
        public void Delete(String tableName, Object id, DataRow parent,
                           Dictionary<string, FieldDescription> fieldTemplates)
        {
            var primaryKeyColumn = GetPrimaryKeyColumn(fieldTemplates);

            /*
			 * Die Datentabelle holen
			 */
            DataTable table = dataSet.Tables[tableName];

            /*
			 * Existiert die Zeile bereits ? 
			 */
            DataRow row = parent != null 
                              ? table.Rows.Find(new[] {parent.ItemArray.GetValue(parent.Table.Columns.IndexOf(primaryKeyColumn)), id}) 
                              : table.Rows.Find(id);

            /*
			 * Wurde das Objekt bereits gelöscht, dann beenden
			 */
            if (row == null) return;

            /*
			* Die abhängigen Tabellen der Zeile auch laden und löschen
			*/
            var childRelations = table.ChildRelations.GetEnumerator();
            while (childRelations.MoveNext())
            {
                var relation = (DataRelation) childRelations.Current;
                var childRows = row.GetChildRows(relation);

                /*
				* Alle Kindelemente durchlaufen und ListLink Elemente löschen
				*/
                foreach (DataRow childRow in childRows)
                    childRow.Delete();
            }

            /*
			* ... und löschen
			*/
            row.Delete();
        }

        /// <summary>
        /// Retuns a list with primary keys that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <returns>Returns a list with IDs.</returns>
        public IList SelectIDs(ProjectionClass projection, string primaryKeyColumn, ICondition whereClause,
                               OrderBy orderBy)
        {
            var result = new ArrayList();

            /*
			 * Die Datentabelle holen
			 */
            var tableName = projection.Tables.GetTable(0);
            var table = dataSet.Tables[tableName];

            /*
             * Check if the table does exist
             */
            if (table != null)
            {
                DataRow[] rows =
                    table.Select(ReplaceStatics(BuildWhereClause(whereClause, true)),
                                 orderBy != null ? orderBy.ColumnsOnly + " " + orderBy.Ordering : "");

                /*
                 * Select ausführen
                 */
                foreach (DataRow row in rows)
                    result.Add(row[primaryKeyColumn]);
            }

            return result;
        }

        /// <summary>
        /// Replaces the statics within a sql statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        protected static string ReplaceStatics(string sql)
        {
            return sql
                .Replace(Condition.TRIM, "TRIM")
                .Replace(Condition.UPPER, "UPPER")
                .Replace(Condition.SCHEMA_REPLACE, "")
                .Replace(Condition.QUOTE_OPEN, "")
                .Replace(Condition.QUOTE_CLOSE, "");
        }

        /// <summary>
        /// Counts number of rows that matches the whereclause
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Number of rows</returns>
        public int Count(ProjectionClass projection, ICondition whereClause,
                         Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter)
        {
            var numberOfRows = 0;

            /*
			 * Die Datentabelle holen
			 */
            var tableName = projection.Tables.GetTable(0);
            var table = dataSet.Tables[tableName];
            if (table != null)
            {
                var rows = table.Select(ReplaceStatics(BuildWhereClause(whereClause, true)));
                numberOfRows = rows.Length;
            }

            return numberOfRows;
        }

        /// <summary>
        /// Executes a page select and returns value objects that matches the search criteria and line number is within the min and max values.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="minLine">Minimum count</param>
        /// <param name="maxLine">Maximum count</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select distinct values only</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> PageSelect(ProjectionClass projection, ICondition whereClause, OrderBy orderBy, int minLine,
                                int maxLine, Dictionary<string, FieldDescription> fieldTemplates,
                                IDictionary globalParameter, bool distinct)
        {
            var result = new List<PersistentProperties>();
            var duplicates = new Hashtable();

            /*
			 * Min Line Dekrimieren, da der Count für XML bei 0 beginnt
			 */
            minLine = (minLine > 0) ? minLine - 1 : minLine;

            string primaryKeyColumn = GetPrimaryKeyColumn(fieldTemplates);

            /*
			 * Die Datentabelle holen
			 */

            IList ids = SelectIDs(projection, primaryKeyColumn, whereClause, orderBy);

            if (minLine >= ids.Count) minLine = ids.Count - 1;
            if (maxLine >= ids.Count) maxLine = ids.Count - 1;

            for (int counter = minLine; counter < maxLine; counter++)
            {
                if (distinct)
                {
                    if (duplicates.ContainsKey(ids[counter]))
                        continue;

                    duplicates.Add(ids[counter], null);
                }

                result.Add(Load(projection, ids[counter], fieldTemplates, globalParameter));
            }

            return result;
        }

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> Select(ProjectionClass projection, ICondition whereClause, OrderBy orderBy,
                            Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter,
                            bool distinct)
        {
            var result = new List<PersistentProperties>();
            var duplicates = new Hashtable();

            string tableName = projection.Tables.GetTable(0);

            /*
			 * Die Datentabelle holen
			 */
            var table = dataSet.Tables[tableName];
            if (table != null)
            {
                string where = ReplaceStatics(BuildWhereClause(whereClause, true));
                DataRow[] rows = table.Select(where, orderBy != null ? orderBy.ColumnsOnly + " " + orderBy.Ordering : "");

                string primaryKey = GetPrimaryKeyColumn(fieldTemplates);

                /*
				* Select ausführen
				*/
                foreach (DataRow row in rows)
                {
                    if (distinct)
                    {
                        if (duplicates.ContainsKey(row[primaryKey]))
                            continue;

                        duplicates.Add(row[primaryKey], null);
                    }

                    result.Add(Load(projection, row[primaryKey], fieldTemplates, globalParameter));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="selectSql">Complete select string which can be executed directly.</param>
        /// <param name="selectParameter">Parameter used for the placeholders within the select string.
        /// A placeholder always begins with an @ followed by a defined key.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> Select(string tableName, string selectSql, SortedList selectParameter,
                            Dictionary<string, FieldDescription> fieldTemplates)
        {
            var result = new List<PersistentProperties>();

            /*
			 * Die Datentabelle holen
			 */
            DataTable table = dataSet.Tables[tableName];
            if (table != null)
            {
                if (selectParameter != null)
                {
                    IDictionaryEnumerator enumerator = selectParameter.GetEnumerator();
                    while (enumerator.MoveNext())
                        selectSql.Replace(((string) enumerator.Key), "'" + enumerator.Value + "'");
                }

                var rows = table.Select(selectSql);

                string primaryKey = GetPrimaryKeyColumn(fieldTemplates);

                /*
				* Select ausführen
				*/
                foreach (DataRow row in rows)
                    result.Add(Load(tableName, row[primaryKey], fieldTemplates));
            }

            return result;
        }

        #region IPersister Members

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="command">The command.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> Select(string tableName, IDbCommand command, Dictionary<string, FieldDescription> fieldTemplates)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Checks if the given primary key is stored within the database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="id">Primary Key</param>
        /// <returns>true, if the record exists in database.</returns>
        public bool Contains(String tableName, string primaryKeyColumn, Object id)
        {
            /*
			 * Die Datentabelle holen
			 */
            if (!dataSet.Tables.Contains(tableName))
                return false;

            var table = dataSet.Tables[tableName];

            /*
			 * Das entsprechende Id prüfen
			 */
            var result = table.Rows.Contains(id);
            return result;
        }

        /// <summary>
        /// Loads the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="id">The id.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <returns></returns>
        private PersistentProperties Load(string tableName, object id, IDictionary fieldTemplates)
        {
            var resultFields = new PersistentProperties();

            var table = dataSet.Tables[tableName];
            if (table == null) return null;

            /*
             * Das entsprechende Zeile laden
             */
            var row = table.Rows.Find(id);
            if (row == null) return null;

            /*
             * Jetzt die Daten aus der Zeile auslesen
             */
            var columns = table.Columns.GetEnumerator();
            while (columns.MoveNext())
            {
                var column = (DataColumn) columns.Current;
                string columnName = column.ColumnName.ToUpper();
                var fieldDescription = (FieldDescription) fieldTemplates[columnName];

                if (fieldDescription == null)
                    continue;

                if (fieldDescription.FieldType.Equals(typeof (VirtualLinkAttribute)))
                    throw new InvalidOperationException("Virtual Links are not supported by the XmlPersister");

                if (columnName.EndsWith(DBConst.TypAddition))
                    continue;

                Object persistField = row[column];
                if (persistField.Equals(DBNull.Value))
                    continue;

                /*
                 * Gehört das Feld zu einem Link ?
                 */
                if (ContainsColumn(table, columnName + DBConst.TypAddition))
                    resultFields.FieldProperties = resultFields.FieldProperties.Add(columnName,
                                     new Link(fieldDescription, persistField,
                                              (String) row[columnName + DBConst.TypAddition]));
                else
                {
                    if (fieldDescription.FieldType == typeof (SpecializedLink))
                        resultFields.FieldProperties = resultFields.FieldProperties.Add(columnName, new SpecializedLink(fieldDescription, persistField));
                    else
                        resultFields.FieldProperties = resultFields.FieldProperties.Add(columnName, new Field(fieldDescription, persistField));
                }
            }

            return resultFields;
        }

        /// <summary>
        /// Method to load an existing row from database.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="id">Primary key</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Hashtable with loaded fields</returns>
        public PersistentProperties Load(ProjectionClass projection, object id,
                                Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter)
        {
            /*
			 * Die Datentabelle holen
			 */
            string tableName = projection.Tables.GetTable(0);
            return Load(tableName, id, fieldTemplates);
        }

        /// <summary>
        /// Determines whether the specified table contains column.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>
        /// 	<c>true</c> if the specified table contains column; otherwise, <c>false</c>.
        /// </returns>
        private static bool ContainsColumn(DataTable table, string columnName)
        {
            bool result = false;

            IEnumerator columns = table.Columns.GetEnumerator();
            while (columns.MoveNext())
            {
                string column = ((DataColumn) columns.Current).ColumnName;
                if (column.ToUpper().Equals(columnName.ToUpper()))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Method to load child objects from an existing object
        /// </summary>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="id">The id.</param>
        /// <param name="typeOfLinkId">The type of link id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked object.</param>
        /// <returns></returns>
        public IDictionary LoadHashChilds(Type parentType, string tableName, Object id, Type typeOfLinkId,
                                          Type linkedPrimaryKeyType, Type linkedObjectType)
        {
            /*
			 * Die Datentabelle holen
			 */
            DataTable table = dataSet.Tables[tableName];
            DataRow[] childRows = table.Select(DBConst.ParentObjectField + " = '" + id + "'");

            var list = new SortedList();
            /*
				* Alle Kindelemente durchlaufen und ListLink Elemente erzeugen
				*/
            foreach (DataRow childRow in childRows)
            {
                list.Add(childRow[DBConst.LinkIdField], new ListLink(
                                                            null,
                                                            childRow[DBConst.LinkIdField],
                                                            parentType,
                                                            childRow[DBConst.ParentObjectField],
                                                            childRow[DBConst.PropertyField],
                                                            linkedObjectType == null
                                                                ? (String) childRow[DBConst.LinkedToField]
                                                                : linkedObjectType.FullName
                                                            ));
            }

            return list;
        }

        /// <summary>
        /// Method to load child objects from an existing object
        /// </summary>
        /// <param name="parentObjectType">Type of the parent.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="objectId">The id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked.</param>
        /// <returns></returns>
        public IList LoadListChilds(Type parentObjectType, string tableName, Object objectId, Type linkedPrimaryKeyType,
                                    Type linkedObjectType)
        {
            /*
             * Die Datentabelle holen
             */
            DataTable table = dataSet.Tables[tableName];
            DataRow[] childRows = table.Select(DBConst.ParentObjectField + " = '" + objectId + "'");

            IList list = new ArrayList();

            /*
             * Alle Kindelemente durchlaufen und ListLink Elemente erzeugen
             */
            foreach (DataRow childRow in childRows)
            {
                list.Add(new ListLink(
                             null,
                             linkedObjectType == null
                                 ? (String) childRow[DBConst.LinkedToField]
                                 : linkedObjectType.FullName,
                             parentObjectType,
                             childRow[DBConst.ParentObjectField],
                             childRow[DBConst.PropertyField]
                             ));
            }

            return list;
        }

        /// <summary>
        /// Starts a transaction
        /// </summary>
        public void BeginTransaction()
        {
        }

        /// <summary>
        /// Commits a transaction
        /// </summary>
        public void Commit()
        {
            File.Delete(dbPath);

            if (DatabaseSchema != null)
                dataSet.WriteXml(dbPath);
            else
                /*
				 * Inline Schema schreiben, wenn keine Externe Datei angegeben wird
				 */
                dataSet.WriteXml(dbPath, XmlWriteMode.WriteSchema);
        }

        /// <summary>
        /// Rollback the changes, if no commit has been done.
        /// </summary>
        public void Rollback()
        {
            ReadXml();
        }

        /// <summary>
        /// Gets the name of the primary key.
        /// </summary>
        /// <param name="templates">The templates.</param>
        /// <returns></returns>
        private static string GetPrimaryKeyColumn(IDictionary templates)
        {
            foreach (DictionaryEntry de in templates)
            {
                var field = de.Value as Field;
                if ((field != null) && (field.FieldDescription.IsPrimary))
                    return (string) de.Key;

                var fieldDesc = de.Value as FieldDescription;
                if ((fieldDesc != null) && (fieldDesc.IsPrimary))
                    return (string) de.Key;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the primary key.
        /// </summary>
        /// <param name="templates">The templates.</param>
        /// <returns></returns>
        private static FieldDescription GetPrimaryKeyDescription(IDictionary templates)
        {
            foreach (DictionaryEntry de in templates)
            {
                var field = de.Value as Field;
                if ((field != null) && (field.FieldDescription.IsPrimary))
                    return field.FieldDescription;

                var fieldDesc = de.Value as FieldDescription;
                if ((fieldDesc != null) && (fieldDesc.IsPrimary))
                    return fieldDesc;
            }

            return null;
        }

        /// <summary>
        /// Erzeugt eine Tabellendefinition für das DataSet
        /// </summary>
        /// <param name="objectType">Objekttyp</param>
        /// <param name="fields">Feld-Definition</param>
        /// <param name="usedDataSet">Add the table defintion to the used dataset</param>
        /// <param name="parentType">Parent Type</param>
        /// <returns>Returns the updated data set</returns>
        private static void CreateTable(String objectType, Type parentType, IDictionary fields, DataSet usedDataSet)
        {
            DataTable newTable = usedDataSet.Tables.Add(objectType);

            /*
			 * Alle einfachen Felder durchlaufen und als Zeilen zur Tabelle hinzufügen
			 */
            var newColumn = new DataColumn[1];
            IDictionaryEnumerator enumerator = fields.GetEnumerator();
            var primaries = new ArrayList();
            while (enumerator.MoveNext())
            {
                if (!(enumerator.Value is FieldDescription))
                    continue;

                var fieldDesc = (FieldDescription) enumerator.Value;

                /*
				 * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
				 */
                if (fieldDesc.FieldType == typeof (Field))
                {
                    newColumn[0] = newTable.Columns.Add(fieldDesc.Name);
                    newColumn[0].DataType = fieldDesc.ContentType.IsDerivedFrom(typeof (Stream)) 
                        ? typeof (byte[]) 
                        : TypeHelper.GetBaseType(fieldDesc.ContentType);

                    /*
                     * Add Primary Keys
                     */
                    if (fieldDesc.IsPrimary)
                        primaries.Add(newColumn[0]);
                }
            }

            /*
			 * Aus der Liste ins Array übertragen
			 */
            var primaryKeys = new DataColumn[primaries.Count];
            IEnumerator primaryEnum = primaries.GetEnumerator();
            int counter = 0;
            while (primaryEnum.MoveNext())
                primaryKeys[counter++] = (DataColumn) primaryEnum.Current;

            /*
			 * Primary Keys setzen
			 */
            newTable.PrimaryKey = primaryKeys;

            /*
			 * Alle komplexen Felder durchlaufen und als Zeilen zur Tabelle hinzufügen
			 */
            enumerator = fields.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!(enumerator.Value is FieldDescription))
                    continue;

                var complexField = (FieldDescription) enumerator.Value;

                /*
  				 * ... oder eine Verknüpfung zu einem anderen VO
				 */
                if (complexField.FieldType == typeof (Link))
                {
                    var link = new Link(complexField, null, "");
                    var projection = ReflectionHelper.GetProjection(link.Property.Type, null);
                    newColumn[0] = newTable.Columns.Add(link.Property.Name);
                    newColumn[0].DataType = projection.GetPrimaryKeyDescription().ContentType;

                    newColumn[0] = newTable.Columns.Add(link.LinkedTo.Name);
                    newColumn[0].DataType = link.LinkedTo.Type;

                    continue;
                }

                if (complexField.FieldType == typeof (SpecializedLink))
                {
                    var link = new SpecializedLink(complexField, null);
                    var projection = ReflectionHelper.GetProjection(complexField.ContentType, null);

                    newColumn[0] = newTable.Columns.Add(link.Property.Name);
                    newColumn[0].DataType = projection.GetPrimaryKeyDescription().ContentType;
                    continue;
                }

                /*
				 * Ist das Objekt über einen Hash verlinkt gewesen ?
				 */
                if (complexField.FieldType == typeof (ListLink))
                {
                    string subTable = string.Concat(objectType + "_" + complexField.Name);
                    Type linkedPrimaryKey = complexField.CustomProperty.MetaInfo.LinkedPrimaryKeyType;
                    bool generalLinked = complexField.CustomProperty.MetaInfo.IsGeneralLinked;

                    if (complexField.ContentType.IsListType())
                        CreateTable(subTable, parentType,
                                    ListLink.GetListTemplates(parentType, generalLinked, linkedPrimaryKey), usedDataSet);
                    else if (complexField.ContentType.IsDictionaryType())
                        CreateTable(subTable, parentType,
                                    ListLink.GetHashTemplates(typeof (string), parentType, generalLinked,
                                                              linkedPrimaryKey), usedDataSet);
                }
            }

            return;
        }

        /// <summary>
        /// Rückgabe eines DBNull Objekts, anstatt null wenn das Objekt nicht belegt ist
        /// </summary>
        /// <param name="value">zu prüfendes Fieldvalue Object</param>
        /// <returns>Object oder DBNull</returns>
        private static Object GetNv(Object value)
        {
            if (value == null)
                return DBNull.Value;

            var stream = value as Stream;
            if (stream != null)
            {
                var content = new byte[stream.Length];
                var length = (int) stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                int readed = stream.Read(content, 0, length);
                Debug.Assert(readed == length, "Could not read stream.");

                return content;
            }

            return value;
        }

        /// <summary>
        /// Creates the schema set.
        /// </summary>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <returns></returns>
        private DataSet CreateSchemaSet(IEnumerable<Type> persistentTypes)
        {
            var schemaSet = new DataSet(dbName) {Locale = CultureInfo.InvariantCulture};
            IEnumerator enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type) enumerator.Current;
                CreateTable(
                    Table.GetTableInstance(type).DefaultName, type,
                    ReflectionHelper.GetProjection(type, null).GetFieldTemplates(false),
                    schemaSet);
            }
            return schemaSet;
        }

        /// <summary>
        /// Export a database schema file. 
        /// </summary>
        /// <param name="schemaFile">File name for the schema export</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        public void WriteSchema(String schemaFile, IEnumerable<Type> persistentTypes)
        {
            var schemaSet = CreateSchemaSet(persistentTypes);
            schemaSet.WriteXmlSchema(schemaFile);
        }

        /// <summary>
        /// Export a database schema file. 
        /// </summary>
        /// <param name="outputStream">Output Stream</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        public void WriteSchema(TextWriter outputStream, IEnumerable<Type> persistentTypes)
        {
            var schemaSet = CreateSchemaSet(persistentTypes);
            schemaSet.WriteXmlSchema(outputStream);
        }

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        public void WriteSchemaDif(string schemaFile, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrity)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        public void WriteSchemaDif(TextWriter ouputStream, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrity)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Checks the integrity.
        /// </summary>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public List<IntegrityInfo> CheckIntegrity(IEnumerable<Type> persistentTypes, ObjectMapper mapper)
        {
            throw new NotSupportedException();
        }

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// </summary>
        ~XmlPersister()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnecting the database
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                dataSet.Dispose();
                dataSet = null;
            }

            // free unmanaged resources
        }

        #endregion

        /// <summary>
        /// Builds a where Clause and returns the string
        /// </summary>
        /// <param name="constraintInterface">Instance of a where clause object</param>
        /// <param name="first"></param>
        /// <returns>Where clause represented as string</returns>
        private static string BuildWhereClause(ICondition constraintInterface, bool first)
        {
            if (constraintInterface == null)
                return "";

            var result = new StringBuilder();

            var condition = constraintInterface as Condition;

            if (condition != null)
                condition.TableName = "";

            /*
				* Den Condition string holen
				*/
            string conditionString = constraintInterface.ConditionString;
            if (!first)
                result.Append(constraintInterface.Type.Operator());

            /*
			 * Handelt es sich zum eine Abfrage?
			 */
            if (condition != null)
            {
                if (conditionString.Contains(Condition.ParameterValue))
                    conditionString = conditionString.ReplaceFirst(Condition.ParameterValue,
                                                                   "'" + condition.Field.Value + "'");

                var inCondition = condition as InCondition;
                if (inCondition != null)
                {
                    IList subSelects = inCondition.SubSelects;
                    if (subSelects != null)
                        for (int counter = 1; counter <= subSelects.Count; counter++)
                        {
                            var subSelect = (SubSelect) subSelects[counter - 1];

                            ICondition[] additionals = subSelect.AdditionalConditions;
                            for (int subCounter = 0; subCounter < additionals.Length; subCounter++)
                                conditionString = conditionString.ReplaceFirst(Condition.NestedCondition,
                                                                               BuildWhereClause(
                                                                                   additionals[subCounter],
                                                                                   subCounter == 0));
                        }
                }
            }

            /*
			 * Does the string contains nested conditions ? 
			 */
            bool nextFirst = false;
            if (constraintInterface is ConditionList) nextFirst = true;
            if (constraintInterface is Parenthesize) nextFirst = true;

            ICondition[] additional = constraintInterface.AdditionalConditions;
            for (int subCounter = 0; subCounter < additional.Length; subCounter++, nextFirst = false)
                conditionString = conditionString.ReplaceFirst(Condition.NestedCondition,
                                                               BuildWhereClause(additional[subCounter], nextFirst));

            /*
			 * De a schema replacement
			 */
            result.Append(conditionString);
            return result.ToString();
        }

        /// <summary>
        /// Property to check if a database connection could be established
        /// </summary>
        public bool IsConnected
        {
            get { return ((dataSet != null) && (!dataSet.HasErrors)); }
        }

        /// <summary>
        /// Sets a new sql tracer to the persister
        /// </summary>
        ISqlTracer IPersister.SqlTracer
        {
            get { throw new NotSupportedException(); }
            set { }
        }

        /// <summary>
        /// XSD File that is used for storing the xsd schema data.
        /// </summary>
        public string DatabaseSchema
        {
            get { return dbSchema; }
            set { dbSchema = value; }
        }

        /// <summary>
        /// Gets the type mapper.
        /// </summary>
        /// <value>The type mapper.</value>
        ITypeMapper IPersister.TypeMapper
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns the Schema Writer
        /// </summary>
        public ISchemaWriter Schema
        {
            get { return this;}
        }

        /// <summary>
        /// Returns the Repository
        /// </summary>
        IRepository IPersister.Repository
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns the Repository
        /// </summary>
        IIntegrity IPersister.Integrity
        {
            get { throw new NotSupportedException(); }
        }
    }
}