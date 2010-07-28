using System;
using System.Data;

namespace AdFactum.Data
{
	/// <summary>
	/// This interface offers simple methods to trace the sql output of a persister
	/// </summary>
	public interface ISqlTracer : IDisposable
	{
		/// <summary>
		/// Must return true, if an error logging shall be enabled.
		/// </summary>
		/// <value><c>true</c> if traceing of errors shall be enabled; otherwise, <c>false</c>.</value>
		bool TraceErrorEnabled { get ; }

		/// <summary>
		/// Must return true, if the SQL tracing shall be enabled
		/// </summary>
		/// <value><c>true</c> if tracing of sql shall be enabled; otherwise, <c>false</c>.</value>
		bool TraceSqlEnabled { get ; }

		/// <summary>
		/// Traces Information when opening a connection
		/// </summary>
		/// <param name="serverVersion">The server version.</param>
		/// <param name="connection">The connection.</param>
		void OpenConnection(string serverVersion, string connection);

        /// <summary>
        /// The persister uses this method to publish sql information
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="extended">The extended string with parameter</param>
        /// <param name="affactedRows">The rows which are touched by the sql statement.</param>
        /// <param name="duration">Duration of an sql statement</param>
		void SqlCommand(IDbCommand command, string extended, int affactedRows, TimeSpan duration);

		/// <summary>
		/// The persister uses this method to publish info messages, like warnings and so on.
		/// </summary>
		/// <param name="message">Message text</param>
		/// <param name="source">Source</param>
		void ErrorMessage(string message, string source);

		/// <summary>
		/// Send if a new transaction begins
		/// </summary>
		void BeginTransaction();

		/// <summary>
		/// Marks that a transaction is committed
		/// </summary>
		void Commit();

		/// <summary>
		/// Logs the rollback of a transaction
		/// </summary>
		void Rollback ();
	}
}