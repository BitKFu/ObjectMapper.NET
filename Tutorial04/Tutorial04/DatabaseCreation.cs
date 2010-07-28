using System.IO;
using AdFactum.Data;
using AdFactum.Data.Util;
using NUnit.Framework;

namespace Tutorial04
{
	/// <summary>
	/// This test creates a new database
	/// </summary>
	[TestFixture]
	public class DatabaseCreation : BaseTest
	{
		/// <summary>
		/// Creates a new access database.
		/// </summary>
		[Test]
		public void CreateAccessDatabase()
		{
			/*
			 * Copy the template database as the new database
			 */
			File.Copy(@"..\..\emptyAccessDb.mdb", Connection.DatabaseFile, true);

			/*
			 * Create the schema
			 */
			new ExportSchema().ExportAccessDDL();

			/*
			 * Open the access db and execute the created script file
			 */
            using (ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                IPersister persister = mapper.Persister;
                SqlFile file = new SqlFile("Marketplace.access.sql");
                file.ExecuteScript(persister as INativePersister);
                file.Dispose();
            }
		}
	}
}