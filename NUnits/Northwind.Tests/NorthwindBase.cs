using AdFactum.Data.Util;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    /// <summary>
    /// Base class for the northwind testing
    /// </summary>
    public class NorthwindBase
    {
        private DatabaseConnection connection;

        /// <summary>
        /// Constructor
        /// </summary>
        public NorthwindBase()
        {
            connection = new DatabaseConnection();
            
            // -- Access DB --
            //connection.DatabaseType = DatabaseType.Access;
            //connection.DatabaseFile = "Nwind.mdb";

            // -- Sql Server --
            //connection.DatabaseType = DatabaseType.SqlServer;
            //connection.ServerName = "AP489406\\";
            //connection.DatabaseName = "Northwind";
            //connection.UserName = "ztb\\eh2gard";
            //connection.TrustedConnection = true;

             //-- Oracle DB --
            connection.DatabaseType = DatabaseType.Oracle;
            connection.DbAlias = @"ISISD.DEV";
            connection.UserName = "Northwind";
            connection.Password = "nw";

            OBM.CurrentSqlTracer = new ConsoleTracer();
            OBM.CurrentObjectFactory = new UniversalFactory();
        }

        /// <summary>
        /// Returns the Connection
        /// </summary>
        public DatabaseConnection Connection
        {
            get { return connection; }
        }
    }
}
