using System;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// Enumeration of the possible database types
	/// </summary>
	public enum DatabaseType
	{
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

		/// <summary>
		/// Using an Sql Server 
		/// </summary>
		SqlServer,

		/// <summary>
		/// Using an Oracle database
		/// </summary>
		Oracle,

        /// <summary>
        /// Using an XML File
        /// </summary>
        Xml,

        /// <summary>
        /// Postgres Database
        /// </summary>
        Postgres,

        /// <summary>
        /// Reliable SQL Server connection used for aszure db
        /// </summary>
        ReliableSqlServer
    }
}