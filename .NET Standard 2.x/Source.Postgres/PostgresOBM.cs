using AdFactum.Data.Postgres;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
	/// This class is used for simple connection mangement
	/// </summary>
	public class PostgresOBM
    {
		/// <summary>
		/// Initialize the Postgres objectmapper
		/// </summary>
        static void Init()
        {
            OBM.connectionMapping.Add(DatabaseType.Postgres, OpenPostgresConnection);
		}

		/// <summary>
		/// Opens an Postgres Connection 
		/// </summary>
		/// <param name="connection">Database connection</param>
		/// <param name="tracer">Trace object</param>
        private static IPersister OpenPostgresConnection(DatabaseConnection connection, ISqlTracer tracer)
        {
            var postgresDb = new PostgresPersister { SqlTracer = tracer };
            postgresDb.Connect(connection.ServerName, connection.UserName, connection.Password, connection.DatabaseName);
            return postgresDb;
        }
    }
}