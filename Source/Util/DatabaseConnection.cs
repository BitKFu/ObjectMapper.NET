using System;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// Diese Klasse speichert alle Informationen die eine Datenbankverbindung beschreiben
	/// und zum öffnen derer notwendig sind.
	/// </summary>
	[Serializable]
	public class DatabaseConnection : ValueObject, ICloneable
	{
		#region Private Members

		private DatabaseType databaseType;

		/*
		 * Common Types
		 */
	    private string userName = string.Empty;
        private string password = string.Empty;

		/*
		 * SQL Server Types
		 */
		private bool trustedConnection  = false;
        private string serverName       = string.Empty;
        private string databaseName     = string.Empty;
        private string dsnName          = string.Empty;

		/*
		 * Access Types
		 */
        private string databaseFile = string.Empty;

		/*
		 * Oracle Types
		 */
        private string dbAlias = string.Empty;

        /*
         * Xml Types
         */
        private string dataSet = string.Empty;
        private string xmlFile = string.Empty;
        private string xsdFile = string.Empty;

		/// <summary>
		/// Expected Database version
		/// </summary>
		private double databaseVersion;

		/// <summary>
		/// Physical Database version
		/// This field has to be filled by the application
		/// </summary>
		private double physicalDatabaseVersion;

		#endregion

		#region Öffentliche Zugriffsmethoden

		/// <summary>
		/// Gets or sets the type of the database.
		/// </summary>
		/// <value>The type of the database.</value>
        [PropertyName("DatabaseType")]
        public DatabaseType DatabaseType
		{
			get { return databaseType; }
			set { databaseType = value; }
		}

		/// <summary>
		/// Gets or sets the name of the server.
		/// </summary>
		/// <value>The name of the server.</value>
		public string ServerName
		{
			get { return serverName; }
			set { serverName = value; }
		}

		/// <summary>
		/// Gets or sets the name of the database.
		/// </summary>
		/// <value>The name of the database.</value>
		public string DatabaseName
		{
			get { return databaseName; }
			set { databaseName = value; }
		}

		/// <summary>
		/// Gets or sets the database file.
		/// </summary>
		/// <value>The database file.</value>
		public string DatabaseFile
		{
			get { return databaseFile; }
			set { databaseFile = value; }
		}

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		/// <summary>
		/// Gets or sets the name of the DSN.
		/// </summary>
		/// <value>The name of the DSN.</value>
		public string DsnName
		{
			get { return dsnName; }
			set { dsnName = value; }
		}

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the connection is a trusted connection.
		/// </summary>
		/// <value><c>true</c> if the connection is a trusted connection; otherwise, <c>false</c>.</value>
		public bool TrustedConnection
		{
			get { return trustedConnection; }
			set { trustedConnection = value; }
		}

		/// <summary>
		/// Gets or sets the db alias.
		/// </summary>
		/// <value>The db alias.</value>
		public string DbAlias
		{
			get { return dbAlias; }
			set { dbAlias = value; }
		}

		/// <summary>
		/// Gets or sets the database version.
		/// </summary>
		/// <value>The database version.</value>
		public double DatabaseVersion
		{
			get { return databaseVersion; }
			set { databaseVersion = value; }
		}

		/// <summary>
		/// Gets or sets the physical database version.
		/// </summary>
		/// <value>The physical database version.</value>
		public double PhysicalDatabaseVersion
		{
			get { return physicalDatabaseVersion; }
			set { physicalDatabaseVersion = value; }
		}

        /// <summary>
        /// Gets or sets the data set.
        /// </summary>
        /// <value>The data set.</value>
	    public string DataSet
	    {
	        get { return dataSet; }
	        set { dataSet = value; }
	    }

        /// <summary>
        /// Gets or sets the XML file.
        /// </summary>
        /// <value>The XML file.</value>
	    public string XmlFile
	    {
	        get { return xmlFile; }
	        set { xmlFile = value; }
	    }

        /// <summary>
        /// Gets or sets the XSD file.
        /// </summary>
        /// <value>The XSD file.</value>
	    public string XsdFile
	    {
	        get { return xsdFile; }
	        set { xsdFile = value; }
	    }

	    #endregion


		/// <summary>
		/// Creates a new object which is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// New Database Connection Instance
		/// </returns>
		public object Clone()
		{
			DatabaseConnection newConnection = new DatabaseConnection();

			newConnection.DatabaseType = DatabaseType;

			newConnection.UserName = UserName;
			newConnection.Password = Password;
	
			/*
			* SQL Server Types
			*/
			newConnection.TrustedConnection = TrustedConnection;
			newConnection.ServerName = ServerName;
			newConnection.DatabaseName = DatabaseName;
			newConnection.DsnName = DsnName;

			/*
			* Access Types
			*/
			newConnection.DatabaseFile = DatabaseFile;

			/*
			* Oracle Types
			*/
			newConnection.DbAlias = DbAlias;

            /*
             * Xml Types
             */
		    newConnection.XmlFile = XmlFile;
		    newConnection.XsdFile = XsdFile;
		    newConnection.DataSet = DataSet;

			/*
			 * Database version
			 */
			newConnection.DatabaseVersion = DatabaseVersion;
			newConnection.PhysicalDatabaseVersion = PhysicalDatabaseVersion;
	
			return newConnection;
		}
	}
}