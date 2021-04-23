using AdFactum.Data.Oracle;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
	/// This class is used for simple connection mangement
	/// </summary>
	public partial class OracleOBM
    {
		/// <summary>
		/// Initialize the oracle objectmapper
		/// </summary>
        static void Init()
        {
            OBM.connectionMapping.Add(DatabaseType.Oracle, OpenOracleConnection);
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
    }
}