using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Access;
using AdFactum.Data.Sql;
using AdFactum.Data.Util;
using AdFactum.Data.XmlPersister;
using BusinessEntities;
using NUnit.Framework;

namespace Tutorial04
{
    /// <summary>
    /// NUnit Testcase for exporting the database schema
    /// </summary>
    [TestFixture]
    public class ExportSchema : BaseTest
    {
        /// <summary>
        /// Gets the business entity types.
        /// </summary>
        /// <value>The business entity types.</value>
        private static List<Type> BusinessEntityTypes
        {
            get
            {
                List<Type> result = new List<Type>();

                /*
				 * Application objects
				 */
                result.Add(typeof (User));
                result.Add(typeof (MarketplaceItem));
                result.Add(typeof (Bid));

                return result;
            }
        }

        /// <summary>
        /// Exports the XML schema.
        /// </summary>
        [Test]
        public void ExportXMLSchema()
        {
            IPersister xmlPersister = new XmlPersister("Marketplace", ".");

            ObjectMapper mapper = new ObjectMapper(new UniversalFactory(), xmlPersister);
            mapper.WriteSchema("Marketplace.xsd", BusinessEntityTypes);
            mapper.Dispose();

            ShowFile("Marketplace.xsd");
        }

        /// <summary>
        /// Exports the access DDL.
        /// </summary>
        [Test]
        public void ExportAccessDDL()
        {
            IPersister accessPersister = new AccessPersister();
            ObjectMapper mapper = new ObjectMapper(new UniversalFactory(), accessPersister);
            mapper.WriteSchema("Marketplace.access.sql", BusinessEntityTypes);
            mapper.Dispose();

            ShowFile("Marketplace.access.sql");
        }

        /// <summary>
        /// Exports the SQL server DDL.
        /// </summary>
        [Test]
        public void ExportSQLServerDDL()
        {
            IPersister sqlPersister = new SqlPersister();
            ObjectMapper mapper = new ObjectMapper(new UniversalFactory(), sqlPersister);
            mapper.WriteSchema("Marketplace.sqlServer.sql", BusinessEntityTypes);
            mapper.Dispose();

            ShowFile("Marketplace.sqlServer.sql");
        }

        /// <summary>
        /// Shows the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private static void ShowFile(string fileName)
        {
            Console.WriteLine("******************************************************");
            Console.WriteLine(fileName);
            Console.WriteLine("******************************************************\n");
            StreamReader reader = File.OpenText(fileName);
            Console.Write(reader.ReadToEnd());
            reader.Close();
        }
    }
}