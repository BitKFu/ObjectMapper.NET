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
		/// Using an Microsoft Access Database
		/// </summary>
		Access,

		/// <summary>
		/// Using an Sql Server 
		/// </summary>
		SqlServer,

		/// <summary>
		/// Using an Oracle database
		/// </summary>
		Oracle,

        /// <summary>
        /// Using an Sql Server 2000
        /// </summary>
        SqlServer2000,

        /// <summary>
        /// Using an XML File
        /// </summary>
        Xml,

        /// <summary>
        /// Postgres Database
        /// </summary>
        Postgres
    }
}