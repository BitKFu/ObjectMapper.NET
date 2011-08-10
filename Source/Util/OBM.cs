using System;
using AdFactum.Data.Access;
using AdFactum.Data.Oracle;
using AdFactum.Data.SqlServer;
using AdFactum.Data.Xml;

#if !CLIENT_PROFILE
using AdFactum.Data.Postgres;
#endif

namespace AdFactum.Data.Util
{
	/// <summary>
	/// This class is used for simple connection mangement
	/// </summary>
	public class OBM
    {
        #region ObjectMapper

        /// <summary>
        /// Creates the mapper using the standard factory and persister of the tutorial.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public static ObjectMapper CreateMapper(DatabaseConnection connection)
        {
            ObjectMapper mapper;
            if (CurrentTransaction == null)
                CurrentTransaction = new ThreadTransaction();

            ThreadTransaction transaction = CurrentTransaction;

            /*
             * Check if a valid transaction Context does exist, if not - create a new mapper object
             */
            if ((transaction.TransactionContext == null) || (transaction.TransactionContext.IsValid == false))
                mapper = new ObjectMapper(transaction.ObjectFactory, GetPersister(connection, transaction.SqlTracer), Transactions.Manual);
            else
                mapper = new ObjectMapper(transaction.ObjectFactory, transaction.TransactionContext);

            /*
             * Store the transaction Context
             */
            transaction.TransactionContext = mapper.TransactionContext;
            return mapper;
        }

#endregion

        #region Persister Management

        /// <summary>
		/// This method is used for creating a persister from a connection object
		/// </summary>
		/// <param name="connection">Connection information</param>
		/// <param name="tracer">Trace Object</param>
		/// <returns>Database dependend persister</returns>
		public static IPersister GetPersister(DatabaseConnection connection, ISqlTracer tracer)
		{
			IPersister persister ;

            // Set the global SQL Casing
            DBConst.GlobalCasing = connection.SqlCasing;

			switch (connection.DatabaseType)
			{
			    case DatabaseType.SqlServer:
			        persister = OpenSqlConnection(connection, tracer);
			        break;

                case DatabaseType.SqlServer2000:
			        persister = OpenSql2000Connection(connection, tracer);
			        break;

                case DatabaseType.Xml:
			        persister = OpenXmlConnection(connection);
			        break;
                case DatabaseType.Access:
					persister = OpenAccessConnection(connection, tracer);
					break;

				case DatabaseType.Oracle:
					persister = OpenOracleConnection(connection, tracer);
					break;

#if !CLIENT_PROFILE
                case DatabaseType.Postgres:
			        persister = OpenPostgresConnection(connection, tracer);
			        break;
#endif
                default:
					throw new ArgumentOutOfRangeException("connection", connection.DatabaseType, "This value is not supported.");
			}
  
			return persister;
        }

        /// <summary>
        /// Opens the XML connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
	    private static IPersister OpenXmlConnection(DatabaseConnection connection)
	    {
            var persister = new XmlPersister(connection.DataSet, connection.XmlFile, connection.XsdFile);
            return persister;
	    }

	    /// <summary>
        /// Opens an Sql Database Connection 
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="tracer">Trace object</param>
        private static IPersister OpenSqlConnection(DatabaseConnection connection, ISqlTracer tracer)
        {
            var sqlDb = new SqlPersister {SqlTracer = tracer};

	        if (connection.DatabaseName != "")
            {
                if (connection.TrustedConnection)
                    sqlDb.Connect(connection.DatabaseName, connection.ServerName);
                else
                    sqlDb.Connect(connection.DatabaseName, connection.ServerName, connection.UserName, connection.Password);
            }
            return sqlDb;
        }

        /// <summary>
        /// Opens an Sql Database Connection 
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="tracer">Trace object</param>
        private static IPersister OpenSql2000Connection(DatabaseConnection connection, ISqlTracer tracer)
        {
            var sqlDb = new Sql2000Persister {SqlTracer = tracer};

            if (connection.DatabaseName != "")
            {
                if (connection.TrustedConnection)
                    sqlDb.Connect(connection.DatabaseName, connection.ServerName);
                else
                    sqlDb.Connect(connection.DatabaseName, connection.ServerName, connection.UserName, connection.Password);
            }
            return sqlDb;
        }

		/// <summary>
		/// Opens an Oracle Connection 
		/// </summary>
		/// <param name="connection">Database connection</param>
		/// <param name="tracer">Trace object</param>
		private static IPersister OpenOracleConnection(DatabaseConnection connection, ISqlTracer tracer)
		{
			var oracleDb = new OraclePersister {SqlTracer = tracer};
		    oracleDb.Connect(connection.UserName, connection.Password, connection.DbAlias);
            if (!string.IsNullOrEmpty(connection.DatabaseSchema))
                oracleDb.DatabaseSchema = connection.DatabaseSchema;
			return oracleDb;
		}

#if !CLIENT_PROFILE

        /// <summary>
        /// Opens an Postgres Connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tracer"></param>
        /// <returns></returns>
        private static IPersister OpenPostgresConnection(DatabaseConnection connection, ISqlTracer tracer)
        {
            var postgresDb = new PostgresPersister {SqlTracer = tracer};
            postgresDb.Connect(connection.ServerName, connection.UserName, connection.Password, connection.DatabaseName);
            return postgresDb;
        }

#endif

		/// <summary>
		/// Opens an Access Databaseconnection 
		/// </summary>
		/// <param name="connection">Database connection</param>
		/// <param name="tracer">Trace object</param>
		private static IPersister OpenAccessConnection(DatabaseConnection connection, ISqlTracer tracer)
		{
			var accessDb = new AccessPersister {SqlTracer = tracer};

		    if (connection.DatabaseFile != "")
			{
				if (connection.Password == "")
					accessDb.Connect(connection.DatabaseFile, string.Empty);
				else
					accessDb.Connect(connection.DatabaseFile, connection.Password);
			}
			return accessDb;
		}

        #endregion

        #region Thread Management

        /// <summary>
        /// Class that encapsulates the treading data
        /// </summary>
        public class ThreadTransaction : IDisposable
        {
            private ITransactionContext     transactionContext;
            private ISqlTracer              sqlTracer;
            private IObjectFactory          objectFactory;

            /// <summary>
            /// Initializes a new instance of the <see cref="ThreadTransaction"/> class.
            /// </summary>
            public ThreadTransaction ()
            {
                ObjectFactory = new UniversalFactory();
            }

            /// <summary>
            /// Gets or sets the transaction context.
            /// </summary>
            /// <value>The transaction context.</value>
            public ITransactionContext TransactionContext
            {
                get { return transactionContext; }
                set { transactionContext = value; }
            }

            /// <summary>
            /// Gets or sets the SQL tracer.
            /// </summary>
            /// <value>The SQL tracer.</value>
            public ISqlTracer SqlTracer
            {
                get { return sqlTracer; }
                set { sqlTracer = value; }
            }

            /// <summary>
            /// Gets or sets the object factory.
            /// </summary>
            /// <value>The object factory.</value>
            public IObjectFactory ObjectFactory
            {
                get { return objectFactory; }
                set { objectFactory = value; }
            }

            /// <summary>
            /// Releases unmanaged resources and performs other cleanup operations before the
            /// </summary>
            ~ThreadTransaction()
            {
                Dispose(false);
            }

            /// <summary>
            /// Disposes this instance.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disconnecting the database
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // free managed resources
                    if (transactionContext != null)
                    {
                        transactionContext.Dispose();
                        transactionContext = null;
                    }

                    if (sqlTracer != null)
                    {
                        sqlTracer.Dispose();
                        sqlTracer = null;
                    }

                    objectFactory = null;
                }

                // free unmanaged resources
            }
        }

	    /// <summary>
	    /// </summary>
	    /// <value>The thread transaction.</value>
	    [ThreadStatic] public static ThreadTransaction CurrentTransaction;

        /// <summary>
        /// Gets or sets the SQL tracer.
        /// </summary>
        /// <value>The SQL tracer.</value>
        [Ignore]
        public static ISqlTracer CurrentSqlTracer
        {
            set
            {
                if (CurrentTransaction == null)
                    CurrentTransaction = new ThreadTransaction();

                CurrentTransaction.SqlTracer = value;
            }
        }

        /// <summary>
        /// Gets or sets the object factory.
        /// </summary>
        /// <value>The object factory.</value>
        [Ignore]
        public static IObjectFactory CurrentObjectFactory
        {
            set
            {
                if (CurrentTransaction == null)
                    CurrentTransaction = new ThreadTransaction();

                CurrentTransaction.ObjectFactory = value;
            }
        }
        
        #endregion

        #region Nested Transactions

        /// <summary>
        /// This method is used for opening an transaction.
        /// If a parent transaction is open, the return value is set to true.
        /// </summary>
        /// <param name="mapper">Database Mapper</param>
        /// <returns>
        /// Returns True, if it's a nested transaction
        /// </returns>
        public static bool BeginTransaction(ObjectMapper mapper)
        {
            bool nestedTransaction = mapper.IsTransactionOpen;
            if (!nestedTransaction)
                mapper.BeginTransaction();

            return nestedTransaction;
        }

        /// <summary>
        /// Flushes the content to database. This method does not close the transaction
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public static void Flush(ObjectMapper mapper)
        {
            try
            {
                mapper.Flush();
            }
            catch (Exception)
            {
                mapper.Rollback();
                throw;
            }
        }

        /// <summary>
        /// This method executes an guarded commit. That means errors will be cached and resolved.
        /// </summary>
        /// <param name="mapper">Database Mapper</param>
        /// <param name="nestedTransaction">True, if it's a nested transaction.</param>
        public static void Commit(ObjectMapper mapper, bool nestedTransaction)
        {
            /*
             * If a surrounding transaction is already open - don't commit the transaction
             */
            if (nestedTransaction)
                return;

            try
            {
                /*
                 * Commit work
                 */
                mapper.Commit();
            }
            catch (Exception)
            {
                mapper.Rollback();
                throw;
            }
        }

        #endregion

    }
}