using System.IO;
using AdFactum.Data.Util;

namespace Tutorial04
{
    /// <summary>
    /// Base class for all testing szenarios
    /// </summary>
    public class BaseTest
    {
        /// <summary>
        /// Defines if the configuration has already been initialized
        /// </summary>
        private static bool isInitialized = false;
        private static DatabaseConnection connection = new DatabaseConnection();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            if (!isInitialized)
            {
                /*
				* Log4Net
				*/
                string log4NetConfig = Path.GetDirectoryName("..\\..\\") + "\\logging.config";
                log4net.Config.XmlConfigurator.Configure(new FileInfo(log4NetConfig));

                isInitialized = true;

                Connection.DatabaseType = DatabaseType.Access;
                Connection.DatabaseFile = "testdb.mdb";
            }
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public static DatabaseConnection Connection
        {
            get { return connection; }
        }

    }
}