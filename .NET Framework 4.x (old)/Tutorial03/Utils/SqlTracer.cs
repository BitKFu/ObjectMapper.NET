using System;
using AdFactum.Data;
using log4net;
using System.Data;
 
namespace Utils
{
	/// <summary>
	/// This method traces the SQL Commands using log4net
	/// </summary>
	public class SqlTracer : ISqlTracer
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (SqlTracer));

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
			if (extended.StartsWith("SELECT"))
				log.Debug(extended);
			else
				log.Info(extended);
		}

		/// <summary>
		/// The persister uses this method to publish info messages, like warnings and so on.
		/// </summary>
		/// <param name="message">Message text</param>
		/// <param name="source">Source</param>
		public void ErrorMessage(string message, string source)
		{
			log.Error(string.Concat(message, "\nSource: ", source));
		}

		/// <summary>
		/// Send if a new transaction begins
		/// </summary>
		public void BeginTransaction()
		{
			log.Info("Begin Transaction");
		}

		/// <summary>
		/// Marks that a transaction is committed
		/// </summary>
		public void Commit()
		{
			log.Info("Commit");
		}

		/// <summary>
		/// Logs the rollback of a transaction
		/// </summary>
		public void Rollback()
		{
			log.Error("Rollback");
		}

		/// <summary>
		/// Must return true, if an error logging shall be enabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if traceing of errors shall be enabled; otherwise, <c>false</c>.
		/// </value>
		public bool TraceErrorEnabled
		{
			get { return log.IsErrorEnabled; }
		}

		/// <summary>
		/// Must return true, if the SQL tracing shall be enabled
		/// </summary>
		/// <value>
		/// 	<c>true</c> if tracing of sql shall be enabled; otherwise, <c>false</c>.
		/// </value>
		public bool TraceSqlEnabled
		{
			get { return log.IsInfoEnabled; }
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or
		/// resetting unmanaged resources.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}