using System;
using System.Collections.Generic;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Access;
using AdFactum.Data.Oracle;
using AdFactum.Data.Sql;
using AdFactum.Data.XmlPersister;
using BusinessEntities;

namespace Tutorial01
{
	/// <summary>
	/// This class shows how to export a DDL file for Oracle, Mircosoft SQL Server and Microsoft Access.
	/// </summary>
	class DDLExport
	{
		/// <summary>
		/// Starting point
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			DDLExport ddlExport = new DDLExport();

			ddlExport.ExportSQLServerDDL ();
			ddlExport.ExportAccessDDL ();
			ddlExport.ExportXMLSchema ();

			/*
			 * The next command does only work with a configured Oracle Client
			 * 
			 * ddlExport.ExportOracleDDL ();
			 */
		}

		/// <summary>
		/// Factory that creates the business entities
		/// </summary>
		private IObjectFactory	objectFactory = new ObjectFactory();

		/// <summary>
		/// Gets the business entity types.
		/// </summary>
		/// <value>The business entity types.</value>
        private List<Type> BusinessEntityTypes
		{
			get
			{
                List<Type> result = new List<Type>();
				result.Add(typeof(User));
				result.Add(typeof(MarketplaceItem));
				result.Add(typeof(Bid));
				
				return result;
			}
		}

		/// <summary>
		/// Creates the mapper.
		/// </summary>
		/// <param name="persister">The persister.</param>
		/// <returns></returns>
		public ObjectMapper CreateMapper(IPersister persister)
		{
			return new ObjectMapper(
				objectFactory, 
				persister, 
				Transactions.Manual);
		}

		/// <summary>
		/// Exports the XML schema.
		/// </summary>
		private void ExportXMLSchema()
		{
			IPersister xmlPersister = new XmlPersister("Marketplace", ".");
			ObjectMapper mapper = CreateMapper(xmlPersister);
			mapper.WriteSchema("Marketplace.xsd", BusinessEntityTypes);
			mapper.Dispose();

			ShowFile("Marketplace.xsd");
		}

		/// <summary>
		/// Exports the access DDL.
		/// </summary>
		private void ExportAccessDDL()
		{
			IPersister accessPersister = new AccessPersister();
			ObjectMapper mapper = CreateMapper(accessPersister);
			mapper.WriteSchema("Marketplace.access.sql", BusinessEntityTypes);
			mapper.Dispose();

			ShowFile("Marketplace.access.sql");
		}

		/// <summary>
		/// Exports the SQL server DDL.
		/// </summary>
		private void ExportSQLServerDDL()
		{
			IPersister sqlPersister = new SqlPersister();
			ObjectMapper mapper = CreateMapper(sqlPersister);
			mapper.WriteSchema("Marketplace.sqlServer.sql", BusinessEntityTypes);
			mapper.Dispose();

			ShowFile("Marketplace.sqlServer.sql");
		}

		/// <summary>
		/// Exports the oracle DDL.
		/// </summary>
		private void ExportOracleDDL()
		{
			IPersister oraclePersister = new OraclePersister();
			ObjectMapper mapper = CreateMapper(oraclePersister);
			mapper.WriteSchema("Marketplace.oracle.sql", BusinessEntityTypes);
			mapper.Dispose();

			ShowFile("Marketplace.oracle.sql");
		}

		/// <summary>
		/// Shows the file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		private void ShowFile(string fileName)
		{
			Console.WriteLine("******************************************************");
			Console.WriteLine(fileName);
			Console.WriteLine("******************************************************\n");
			StreamReader reader = File.OpenText(fileName);
			Console.Write(reader.ReadToEnd());

			Console.WriteLine("\nPress return to continue\n\n");
			Console.ReadLine();
		}
	}
}
