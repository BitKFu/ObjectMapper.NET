namespace AdFactum.Data.Repository
{
	/// <summary>
	/// This class is used to indentify a tablespace 
	/// It is used by the Oracle Repository Persister to store the meta model of a table.
	/// </summary>
	[Table("OMRE_TABLESPACES")]
	public class Tablespace : MarkedValueObject
	{
		#region Private members

		/// <summary>
		/// Name of the tablespace
		/// </summary>
		private string tablespaceName;

		/// <summary>
		/// Name of the tablespace class
		/// </summary>
		private string className;

		/// <summary>
		/// Name of the application
		/// </summary>
		private string application;

		#endregion

		#region Public Properties

		/// <summary>
		/// Name of the tablespace
		/// </summary>
		[PropertyLength(32)]
		[PropertyName("TABLESPACE_NAME")]
		public string TablespaceName
		{
			get { return tablespaceName; }
			set { tablespaceName = value; }
		}

		/// <summary>
		/// Name of the tablespace class
		/// </summary>
		[PropertyLength(64)]
		[PropertyName("CLASS_NAME")]
		public string ClassName
		{
			get { return className; }
			set { className = value; }
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