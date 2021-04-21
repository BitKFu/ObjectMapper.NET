using System;
using System.Collections.Generic;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Access;
using AdFactum.Data.Util;
using BusinessEntities;
using NUnit.Framework;
using Tutorial02;

namespace Tutorial03
{
	/// <summary>
	/// This test creates a new database
	/// </summary>
	[TestFixture]
	public class DatabaseCreation : BaseTest
	{
        /// <summary>
        /// Gets the business entity types.
        /// </summary>
        /// <value>The business entity types.</value>
        private List<Type> BusinessEntityTypes
        {
            get
            {
                List<Type> result = new List<Type>();

                /*
                 * Application objects
                 */
                result.Add(typeof(User));
                result.Add(typeof(MarketplaceItem));
                result.Add(typeof(Bid));

                return result;
            }
        }

        /// <summary>
        /// Exports the access DDL.
        /// </summary>
        private void ExportAccessDDL()
        {
            IPersister accessPersister = new AccessPersister();
            accessPersister.Schema.WriteSchema("Marketplace.access.sql", BusinessEntityTypes);
        }


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
            ExportAccessDDL();

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