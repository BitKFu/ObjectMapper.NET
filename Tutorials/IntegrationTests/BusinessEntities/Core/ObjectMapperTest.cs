using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AdFactum.Data;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;

namespace ObjectMapper.NUnits.Core
{
    /// <summary>
    /// Base class for the NUnit Tests
    /// </summary>
    public abstract class ObjectMapperTest 
    {
        private readonly DatabaseType whatToTest = DatabaseType.SqlServer;
        private readonly DatabaseConnection connection;

        protected static FullFeaturedCompany fullFeaturedCompany = new FullFeaturedCompany("ManyToMany")
        {
            Employees = new List<Employee>()
            {
                new Employee("Sven", "Björndal"),
                new Employee("Marius", "Uhlen"),
                new Employee("Bonsai", "Bandito"),
                new Employee("Hermann", "Kuckuck"),
                new Employee("Peter", "Witzigmann"),
            },

            PhoneBook = new List<PhoneBookEntry>()
            {
                new PhoneBookEntry(null, null, "0172-82956789")        ,
                new PhoneBookEntry(null, null, "0151-58786245")
            }
        };

        protected static Company company = new Company("Huste mir was")
        {
            Employees = new List<Employee>()
            {
                new Employee("Sven", "Björndal"),
                new Employee("Marius", "Uhlen"),
                new Employee("Bonsai", "Bandito"),
                new Employee("Hermann", "Kuckuck"),
                new Employee("Peter", "Witzigmann"),
            }
        };


        protected ObjectMapperTest (string testPostfix)
        {
            /*
             * read Configuration file
             */
            string databaseTypeSettings = ConfigurationManager.AppSettings["DatabaseType"];
            ClassicAssert.IsNotNull(databaseTypeSettings, "No Database Type has been selected.");
            
            if (databaseTypeSettings != string.Empty)
                whatToTest = (DatabaseType) Enum.Parse(typeof(DatabaseType),databaseTypeSettings);

            Console.WriteLine("Test case for " + whatToTest);
                    
            /*
             * Create Connection
             */
            connection = new DatabaseConnection();
            Connection.DatabaseType = whatToTest;
            
            OBM.CurrentSqlTracer = new ConsoleTracer();
            OBM.CurrentObjectFactory = new UniversalFactory();

            switch (whatToTest)
            {
                case DatabaseType.Postgres:
                    connection.DatabaseName = ConfigurationManager.AppSettings["DatabaseName" + testPostfix];
                    connection.ServerName = ConfigurationManager.AppSettings["ServerName" + testPostfix];
                    connection.UserName = ConfigurationManager.AppSettings["UserName" + testPostfix];
                    connection.Password = ConfigurationManager.AppSettings["Password" + testPostfix];
                    break;

                case DatabaseType.SqlServer:
                    Connection.DatabaseName = ConfigurationManager.AppSettings["DatabaseName" + testPostfix];
                    Connection.ServerName = ConfigurationManager.AppSettings["ServerName" + testPostfix];
                    Connection.UserName = ConfigurationManager.AppSettings["UserName" + testPostfix];
                    Connection.Password = ConfigurationManager.AppSettings["Password" + testPostfix];
                    connection.TrustedConnection = bool.Parse(ConfigurationManager.AppSettings["TrustedConnection" + testPostfix] ?? "false");
                    break;

                case DatabaseType.Access:
                    Connection.DatabaseFile = ConfigurationManager.AppSettings["DatabaseFile" + testPostfix];
                    break;

                case DatabaseType.Oracle:
                    Connection.UserName = ConfigurationManager.AppSettings["UserName" + testPostfix];
                    Connection.Password = ConfigurationManager.AppSettings["Password" + testPostfix];
                    Connection.DbAlias = ConfigurationManager.AppSettings["DbAlias" + testPostfix];
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMapperTest"/> class.
        /// </summary>
        protected ObjectMapperTest ()
            :this(string.Empty)
        {
        }


        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        protected DatabaseConnection Connection
        {
            get { return connection; }
        }


        /// <summary>
        /// Return all used types
        /// </summary>
        /// <returns></returns>
        protected List<Type> GetTypes()
        {
            return new List<Type>
            {
              typeof (VersionInfo),
              typeof (EntityInfo),
              typeof (EntityPredicate),
              typeof (EntityRelation)
            }.Concat(GetType().Assembly.GetTypes().Where(
                t => t.Namespace == "ObjectMapper.NUnits.BusinessEntities"
                     && typeof (IValueObject).IsAssignableFrom(t)
                     && t.IsAbstract == false && t.IsInterface == false)).OrderBy(type=>type.FullName).ToList();
        }
    }
}
