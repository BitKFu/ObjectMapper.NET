using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace AdFactum.Data
{
	/// <summary>
	/// This interface contains methods for direct database access
	/// </summary>
	public interface INativePersister : IDisposable
	{
        /// <summary>
        /// Executes the sql command and
        /// fills the data table object with the returned result set.
        /// </summary>
        /// <param name="execSql">The SQL command which shall be executed.</param>
        /// <returns>Data Table</returns>
		DataTable FillTable(string execSql);

        /// <summary>
        /// Fills the table.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
	    DataTable FillTable(IDbCommand command);

		/// <summary>
		/// Executes an sql statement and returns the number of affacted rows.
		/// </summary>
		/// <param name="execSql">The SQL command which shall be executed.</param>
		/// <returns>Affacted rows</returns>
		int Execute(string execSql);

		/// <summary>
		/// Executes an sql statement and make use of the native parameters
		/// </summary>
		/// <param name="execSql">The SQL command which shall be executed.</param>
		/// <param name="parameter">Native parameter array.</param>
		/// <returns>Affacted rows</returns>
		/// <code>
		/// // Oracle Example
		/// int affactedRows = ExecuteWithParameter ("SELECT * FROM TableName where columnName = :p1", parameter);
		///
		/// // SQL Example
		/// int affactedRows = ExecuteWithParameter ("SELECT * FROM TableName where columnName = @p1", parameter);
		///
		/// // Access Example
		/// int affactedRows = ExecuteWithParameter ("SELECT * FROM TableName where columnName = ?", parameter);
		/// </code>
		int ExecuteWithParameter(string execSql, params object[] parameter);

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateCommand();

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="numberOfParameter">The number of parameter.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, bool isUnicode);

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string parameterName, Type type, object value, bool isUnicode);

        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        string GetParameterString(IDbDataParameter parameter);

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
	    IDataReader ExecuteReader(IDbCommand command);

	    /// <summary>
	    /// Return the columns
	    /// </summary>
	    void GetColumns(
	        IDataReader reader,
	        Dictionary<string, FieldDescription> fieldTemplates,

	        out Dictionary<string, int> fieldIndexDict,
	        out Dictionary<int, string> indexFieldDict);
	}
}