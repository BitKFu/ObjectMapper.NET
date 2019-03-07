﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace AdFactum.Data.Access
{
    /// <summary>
    /// This class implements an access connection cache, in order to have a better performance
    /// </summary>
    public class AccessConnectionCache : IDisposable
    {
        private static AccessConnectionCache singleton;

        public static AccessConnectionCache Get => singleton ?? (singleton = new AccessConnectionCache());

        private AccessConnectionCache() { }

        public class Connection
        {
            public Connection(OleDbConnection connection, string connectionString)
            {
                AccessConnection = connection;
                ConnectionString = connectionString;

                IsInUse = true;
            }

            public bool IsInUse { get; set; }
            public OleDbConnection AccessConnection { get; }
            public string ConnectionString { get; }
        }


        private List<Connection> Connections { get; } = new List<Connection>();

        public IDbConnection GetFreeConnection(string connectionString)
        {
            lock (Connections)
            {
                var connection = Connections.FirstOrDefault(c => c.ConnectionString == connectionString && !c.IsInUse);
                if (connection != null)
                {
                    connection.IsInUse = true;
                    return connection.AccessConnection;
                }
                else
                {
                    connection = new Connection(new OleDbConnection(connectionString), connectionString);
                    Connections.Add(connection);
                    return connection.AccessConnection;
                }
            }
        }

        public void FreeConnection(IDbConnection connection)
        {
            lock (Connections)
            {
                var con = Connections.FirstOrDefault(c => c.AccessConnection == connection);
                if (con != null)
                {
                    con.IsInUse = false;
                }
            }
        }
        
        #region IDisposable

        ~AccessConnectionCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnecting the database
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            lock (Connections)
            {
                foreach (var con in Connections)
                {
                    try
                    {
                        con.AccessConnection.Dispose();
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore this exception, if the connection is already disposed
                    }
                }

                Connections.Clear();
            }
        }

        #endregion
    }
}
