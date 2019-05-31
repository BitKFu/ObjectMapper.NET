using System;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
    /// Class Attribute to define that the class is mapped to a database table.
    /// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	[Serializable]
    public class TableAttribute : Attribute
	{
		/// <summary>
		/// Table Name
		/// </summary>
		private readonly string name;

        /// <summary>
        /// Specifies that the name is only valid for the given database Type
        /// </summary>
	    private DatabaseType? databaseType;

	    /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
	    internal TableAttribute()
	    {
	    }
	    
		/// <summary>
		/// Attribute Constructor
		/// </summary>
		/// <param name="pName">Table Name</param>
		public TableAttribute(String pName)
		{
			name = pName; 
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dbType">Type of the db.</param>
        public TableAttribute(string tableName, DatabaseType dbType)
            :this(tableName)
        {
            databaseType = dbType;     
        }

        /// <summary>
        /// Gets the database type1.
        /// </summary>
        /// <value>The database type1.</value>
	    public DatabaseType? DatabaseType
	    {
	        get { return databaseType; }
	    }

	    /// <summary>
		/// Returns the table name
		/// </summary>
		public string Name
		{
			get { return name; }
		}

	}
}