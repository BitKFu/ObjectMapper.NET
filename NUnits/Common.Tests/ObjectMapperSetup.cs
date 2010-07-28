﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Internal;
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
            if (Connection.DatabaseType == DatabaseType.SqlServer || Connection.DatabaseType == DatabaseType.SqlServer2000)
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
                    persister.Execute("if db_id('ObjectMapper') is not null\n drop database ObjectMapper;");
                    persister.Execute("create database ObjectMapper;");
                }
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
                                });
            }
        }

        [TearDown]
        public void UnloadCaches()
        {
#if DEBUG
            BaseCache.OutputAllCacheInfos();
#endif
        }
    }
}