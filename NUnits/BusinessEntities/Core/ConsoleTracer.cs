using System;
using System.Data;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Core
{
    public class ConsoleTracer : ISqlTracer
    {
        /// <summary>
        /// Must return true, if an error logging shall be enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if traceing of errors shall be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool TraceErrorEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// Must return true, if the SQL tracing shall be enabled
        /// </summary>
        /// <value>
        /// 	<c>true</c> if tracing of sql shall be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool TraceSqlEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// Traces Information when opening a connection
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        /// <param name="connection">The connection.</param>
        public void OpenConnection(string serverVersion, string connection)
        {
        }

        /// <summary>
        /// The persister uses this method to publish sql information
        /// </summary>
        /// <param name="original">The original string without the parameter extension.</param>
        /// <param name="extended">The extended string with parameter</param>
        /// <param name="affactedRows">The rows which are touched by the sql statement.</param>
        /// <param name="duration">Duration of an sql statement</param>
        public void SqlCommand(IDbCommand original, string extended, int affactedRows, TimeSpan duration)
        {
            Console.WriteLine(extended);
        }

        /// <summary>
        /// The persister uses this method to publish info messages, like warnings and so on.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="source">Source</param>
        public void ErrorMessage(string message, string source)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Send if a new transaction begins
        /// </summary>
        public void BeginTransaction()
        {
        }

        /// <summary>
        /// Marks that a transaction is committed
        /// </summary>
        public void Commit()
        {
        }

        /// <summary>
        /// Logs the rollback of a transaction
        /// </summary>
        public void Rollback()
        {
        }

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            
        }
    }
}
