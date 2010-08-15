using System;
using System.Collections;
using System.Collections.Generic;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Repository
{
	/// <summary>
	/// This class stores information about the integrity of an database table compared to a class object
	/// </summary>
	public class IntegrityInfo
	{ 
		private string						tableName;
		private Type						objectType;
		private bool						tableExists		= true;
        private Dictionary<string, FieldDescription> fields = null;

		private List<FieldIntegrity>        mismatchedFields  = new List<FieldIntegrity>();   // Contains fields with wrong datatypes

		/// <summary>
		/// Initializes a new instance of the <see cref="IntegrityInfo"/> class.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="fields">The fields.</param>
        public IntegrityInfo(Type objectType, Dictionary<string, FieldDescription> fields)
		{
            TableName = Table.GetTableInstance(objectType).DefaultName;
			ObjectType = objectType;
			Fields = fields;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IntegrityInfo"/> class.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="fields">The fields.</param>
        public IntegrityInfo(string tableName, Dictionary<string, FieldDescription> fields)
		{
			TableName = tableName;
			Fields = fields;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is valid.
		/// </summary>
		/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
		public bool IsValid
		{
			get
			{
				bool result = tableExists;
				if (result) result = (MismatchedFields.Count == 0);

				return result;
			}
		}

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>The name of the table.</value>
		public string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		/// <summary>
		/// Gets or sets the type of the class.
		/// </summary>
		/// <value>The type of the class.</value>
		public Type ObjectType
		{
			get { return objectType; }
			set { objectType = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [table exists].
		/// </summary>
		/// <value><c>true</c> if [table exists]; otherwise, <c>false</c>.</value>
		public bool TableExists
		{
			get { return tableExists; }
			set { tableExists = value; }
		}

		/// <summary>
		/// Gets or sets the fields.
		/// </summary>
		/// <value>The fields.</value>
        public Dictionary<string, FieldDescription> Fields
		{
			get { return fields; }
			set { fields = value; }
		}

		/// <summary>
		/// Gets or sets the mismatched fields.
		/// </summary>
		/// <value>The mismatched fields.</value>
		public List<FieldIntegrity> MismatchedFields
		{
			get { return mismatchedFields; }
			set { mismatchedFields = value; }
		}

	}
}
