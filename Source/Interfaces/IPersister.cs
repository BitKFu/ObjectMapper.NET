using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;

namespace AdFactum.Data
{
    /// <summary>
    /// Database interface
    /// </summary>
	public interface IPersister : IDisposable
	{
        /// <summary>
        /// Method to insert a new row to database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns></returns>
        object Insert(string tableName, PersistentProperties fields, Dictionary<string, FieldDescription> fieldTemplates);

        /// <summary>
        /// Method to update an existing row in database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        void Update(string tableName, PersistentProperties fields, Dictionary<string, FieldDescription> fieldTemplates);

        /// <summary>
        /// Method to load an existing row from database.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="id">Primary key</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Hashtable with loaded fields</returns>
        PersistentProperties Load(ProjectionClass projection, object id, Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter);

        /// <summary>
        /// Method to load child objects from an existing object
        /// </summary>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="objectId">The object id.</param>
        /// <param name="typeOfLinkId">The type of link id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked object.</param>
        /// <returns></returns>
        IDictionary LoadHashChilds(Type parentType, string tableName, Object objectId, Type typeOfLinkId, Type linkedPrimaryKeyType, Type linkedObjectType);

        /// <summary>
        /// Loads the list childs.
        /// </summary>
        /// <param name="parentObjectType">Type of the parent object.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="objectId">The object id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked object.</param>
        /// <returns></returns>
        IList LoadListChilds(Type parentObjectType, string tableName, Object objectId, Type linkedPrimaryKeyType, Type linkedObjectType);
        
	    /// <summary>
		/// Method to delete an existing row from database.
		/// </summary>
		/// <param name="tableName">Table Name</param>
		/// <param name="id">Primary key</param>
		/// <param name="fieldTemplates">Field description.</param>
        void Delete(string tableName, Object id, Dictionary<string, FieldDescription> fieldTemplates);

        /// <summary>
        /// Retuns a list with primary keys that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <returns>Returns a list with IDs.</returns>
        IList SelectIDs(ProjectionClass projection, string primaryKeyColumn, ICondition whereClause, OrderBy orderBy);

        /// <summary>
        /// Checks if the given primary key is stored within the database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="id">Primary Key</param>
        /// <returns>true, if the record exists in database.</returns>
        bool Contains(string tableName, string primaryKeyColumn, object id);

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="conditions">Condition collection</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List of value objects</returns>
        List<PersistentProperties> Select(ProjectionClass projection, ICondition conditions, OrderBy orderBy, Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter, bool distinct);

		/// <summary>
		/// Returns a list with value objects that matches the search criteria.
		/// </summary>
		/// <param name="tableName">Table Name</param>
		/// <param name="selectSql">Complete select string which can be executed directly.</param>
		/// <param name="selectParameter">Parameter used for the placeholders within the select string. 
		/// A placeholder always begins with an @ followed by a defined key.</param>
		/// <param name="fieldTemplates">Field description.</param>
		/// <returns>List of value objects</returns>
        List<PersistentProperties> Select(string tableName, string selectSql, SortedList selectParameter, Dictionary<string, FieldDescription> fieldTemplates);

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="command">The command.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns>List of value objects</returns>
        List<PersistentProperties> Select(string tableName, IDbCommand command, Dictionary<string, FieldDescription> fieldTemplates);

        /// <summary>
        /// Executes a page select and returns value objects that matches the search criteria and line number is within the min and max values.
        /// </summary>
        List<PersistentProperties> PageSelect(ProjectionClass projection, ICondition whereClause, OrderBy orderBy, int minLine, int maxLine, Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter, bool distinct);

        /// <summary>
        /// Counts number of rows that matches the whereclause
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Number of rows</returns>
        int Count(ProjectionClass projection, ICondition whereClause, Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter);

       
        /// <summary> Starts a transaction </summary>
		void BeginTransaction();

		/// <summary> Commits a transaction </summary>
		void Commit();

		/// <summary> Rollback the changes, if no commit has been done. </summary>
		void Rollback();

		/// <summary> Returns true, if a connection is established </summary>
		bool IsConnected { get; }

        /// <summary> Gets or sets the database schema. </summary>
        string DatabaseSchema { get; set; }

		/// <summary> Set's an SQL tracer </summary>
		ISqlTracer SqlTracer	{ get; set; }

        /// <summary> Gets the type mapper. </summary>
        ITypeMapper TypeMapper { get;  }

        /// <summary> Gets the Schema Writer  </summary>
        ISchemaWriter Schema { get; }

        /// <summary> Gets the Repository </summary>
        IRepository Repository { get; }

        /// <summary> Gets the Integrity Information Class </summary>
        IIntegrity Integrity { get; }
	}
}