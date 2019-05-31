namespace AdFactum.Data.Repository
{
	/// <summary>
	/// This class contains the information about a table storage.
	/// </summary>
	[Table("OMRE_ORM_STORAGES")]
	public class TableStorage : MarkedValueObject
	{
		#region Private members

		/// <summary>
		/// Tablename
		/// </summary>
		private string tableName;

		/// <summary>
		/// Y or N, depends if the table shall be created by the mapper
		/// </summary>
		private string generate;

		/// <summary>
		/// Tablespace ClassName of Table
		/// </summary>
		private string dataTsClassName;

		/// <summary>
		/// Tablespace ClassName of Indexes of Table
		/// </summary>
		private string indexTsClassName;

		/// <summary>
		/// Tablespace Name for Tables
		/// </summary>
		private string dataTablespace;

		/// <summary>
		/// Tablespace Name for Indexes of Tables
		/// </summary>
		private string indexTablespace;

		/// <summary>
		/// Version informations
		/// </summary>
		private string application;

		#endregion

		#region Public properties

		/// <summary>
		/// Tablename
		/// </summary>
		[PropertyLength(32)]
		[PropertyName("TABLE_NAME")]
		[Required]
		public string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		/// <summary>
		/// Y or N, depends if the table shall be created by the mapper
		/// </summary>
		[PropertyLength(1)]
		[PropertyName("GENERATE")]
		public string Generate
		{
			get { return generate; }
			set { generate = value; }
		}

		/// <summary>
		/// Tablespace ClassName of Table
		/// </summary>
		[PropertyLength(32)]
		[PropertyName("DATA_TS_CLASS_NAME")]
		public string DataTsClassName
		{
			get { return dataTsClassName; }
			set { dataTsClassName = value; }
		}

		/// <summary>
		/// Tablespace ClassName of Indexes of Table
		/// </summary>
		[PropertyLength(32)]
		[PropertyName("INDEX_TS_CLASS_NAME")]
		public string IndexTsClassName
		{
			get { return indexTsClassName; }
			set { indexTsClassName = value; }
		}

		/// <summary>
		/// Tablespace Name for Tables
		/// </summary>
		[VirtualLink(typeof (Tablespace), "TablespaceName", "ClassName", "DataTsClassName", "Application", "!!APP!!")]
		public string DataTablespace
		{
			get { return dataTablespace; }
			set { dataTablespace = value; }
		}

		/// <summary>
		/// Tablespace Name for Indexes of Tables
		/// </summary>
		[VirtualLink(typeof (Tablespace), "TablespaceName", "ClassName", "IndexTsClassName", "Application", "!!APP!!")]
		public string IndexTablespace
		{
			get { return indexTablespace; }
			set { indexTablespace = value; }
		}

		/// <summary>
		/// Name of the application
		/// </summary>
		/// <value>The application.</value>
		[PropertyLength(64)]
		[PropertyName("APPLICATION")]
		public string Application
		{
			get { return application; }
			set { application = value; }
		}
		#endregion
	}
}