using System.IO;
using AdFactum.Data.Access;
using AdFactum.Data.Util;
using NUnit.Framework;

namespace Tutorial02
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
			File.Copy(@"..\..\emptyAccessDb.mdb", "accessDb.mdb", true);

			/*
			 * Create the schema
			 */
			new ExportSchema().ExportAccessDDL();

			/*
			 * Open the access db and execute the created script file
			 */
			AccessPersister persister = GetAccessPersister();
			SqlFile file = new SqlFile("Marketplace.access.sql");
			file.ExecuteScript(persister);
			file.Dispose();
			persister.Dispose();
		}
	}
}