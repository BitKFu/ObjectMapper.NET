using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Access;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Internal;
using AdFactum.Data.Oracle;
using AdFactum.Data.Util;
using Npgsql;
using NUnit.Framework;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [SetUpFixture]
    public class ObjectMapperSetup : ObjectMapperTest 
    {
        /// <summary>
        /// Sets the up.
        /// </summary>
        [SetUp]
        public void RunBeforeAnyTests()
        {
            List<Type> types = GetTypes();

            /*
             * Recreate the SQL Server Database
             */
            if (Connection.DatabaseType == DatabaseType.SqlServer)
            {
                SqlConnection.ClearAllPools();

                var masterConnection = new DatabaseConnection
                                           {
                                               DatabaseType = Connection.DatabaseType,
                                               DatabaseName = "Master",
                                               ServerName = Connection.ServerName,
                                               UserName = Connection.UserName,
                                               Password = Connection.Password,
                                               TrustedConnection =  Connection.TrustedConnection
                                           };

                using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(masterConnection))
                {
                    var persister = (INativePersister) mapper.Persister;
                    persister.Execute(string.Format("if db_id('{0}') is not null\n drop database {0};", Connection.DatabaseName));
                    persister.Execute(string.Format("create database {0};", Connection.DatabaseName));
                }
            }

            /*
             * Recopy the Access Database
             */
            if (Connection.DatabaseType == DatabaseType.Access)
            {
                OleDbConnection.ReleaseObjectPool();

                var cat = new ADOX.CatalogClass();

                string path = Path.Combine(Directory.GetCurrentDirectory(), Connection.DatabaseFile); 

                if (File.Exists(path))
                    File.Delete(path);

                cat.Create(string.Format("Provider=Microsoft.Jet.OLEDB.4.0;" +
                       "Data Source={0};" +
                       "Jet OLEDB:Engine Type=5", path));
            }

            /*
             * Recreate the Postgres Database
             */
            if (Connection.DatabaseType == DatabaseType.Postgres)
            {
                NpgsqlConnection.ClearAllPools();

                var masterConnection = new DatabaseConnection()
                {
                    DatabaseType = Connection.DatabaseType,
                    DatabaseName = "postgres",
                    ServerName = Connection.ServerName,
                    UserName = Connection.UserName,
                    Password = Connection.Password,
                };

                using (var mapper = OBM.CreateMapper(masterConnection))
                {
                    var persister = (INativePersister)mapper.Persister;

                    var command = persister.CreateCommand();
                    command.CommandText = "select count(*) from pg_catalog.pg_database where datname='"+Connection.DatabaseName+"';";
                    var reader = persister.ExecuteReader(command);
                    reader.Read();
                    var counter = reader.GetInt64(0);
                    if (counter>0)
                    {
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                        NpgsqlConnection.ClearAllPools();
                        persister.Execute("drop database " + Connection.DatabaseName + ";");
                    }

                    persister.Execute("create database " + Connection.DatabaseName + ";");
                }
            }

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                IPersister persister = mapper.Persister;

                /*
                 * Create the tables instantly
                 */
                var writer = new StreamWriter(new MemoryStream());
                mapper.Schema.WriteSchema(writer, types);

                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    using (var file = new SqlFile(writer.BaseStream))
                        file.ExecuteScript(persister as INativePersister, 
                            delegate(string sql, SqlCoreException exception, out bool doContinue)
                                {
                                    Console.WriteLine("FAILED: " + sql);
                                    doContinue = false;

                                    // Continue when using Access and SQL is a DROP Statement
                                    if (persister is AccessPersister || persister is OraclePersister)
                                        if (sql.StartsWith("DROP"))
                                            doContinue = true;
                                        else
                                            doContinue = false;
                                });
            }
        }

        [TearDown]
        public void UnloadCaches()
        {
#if DEBUG
            //BaseCache.OutputAllCacheInfos();
#endif
        }
    }
}