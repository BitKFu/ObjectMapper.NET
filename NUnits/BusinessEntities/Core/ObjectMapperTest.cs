using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AdFactum.Data;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
using NUnit.Framework;

namespace ObjectMapper.NUnits.Core
{
    /// <summary>
    /// Base class for the NUnit Tests
    /// </summary>
    public abstract class ObjectMapperTest 
    {
        private readonly DatabaseType whatToTest = DatabaseType.SqlServer;
        private readonly DatabaseConnection connection;


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMapperTest"/> class.
        /// </summary>
        protected ObjectMapperTest ()
        {
            /*
             * read Configuration file
             */
            string databaseTypeSettings = ConfigurationManager.AppSettings["DatabaseType"] ?? string.Empty;
            
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
                    connection.DatabaseName = "objectmapper";
                    connection.ServerName = "localhost";
                    connection.UserName = "postgres";
                    connection.Password = "admin";
                    break;

                case DatabaseType.SqlServer:
                    Connection.DatabaseName = "ObjectMapper";
                    Connection.ServerName = @".";
                    Connection.UserName = "sa";
                    Connection.Password = "admin";
                    break;

                case DatabaseType.Access:
                    Connection.DatabaseFile = "TestDb.mdb";
                    break;

                case DatabaseType.Oracle:
                    Connection.UserName  = "system";
                    Connection.Password  = "SzxU8D9gslXA";
                    Connection.DbAlias   = "XE";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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
