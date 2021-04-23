using System;
using System.Collections;
using System.IO;
using AdFactum.Data.Exceptions;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// Sql File Failed Delegate, informs the user, if the excetion of an sql failed.
    /// If the out parameter "continueExcetution" is false, than the execution will stop immediatly.
    /// </summary>
    public delegate void SqlFileFailed(string sql, SqlCoreException exception, out bool continueExecution);

	/// <summary>
	/// Liest ein SQL File und extrahiert daraus die SQL Befehle
	/// </summary>
	public class SqlFile : IDisposable
	{
		private readonly IList sqlCommands;
		private readonly IList sqlFailures;
		private readonly string sqlFile;
		private StreamReader input;

		/// <summary>
		/// Standardkonstruktor
		/// </summary>
        /// <param name="sqlFileParameter"></param>
		public SqlFile(string sqlFileParameter)
		{
            sqlFile = sqlFileParameter;
			sqlCommands = new ArrayList();
			sqlFailures = new ArrayList();
			input = File.OpenText(sqlFile);

			Read();
		}

        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        /// <param name="sqlStream"></param>
        public SqlFile(Stream sqlStream)
        {
            sqlCommands = new ArrayList();
            sqlFailures = new ArrayList();
            input = new StreamReader(sqlStream);

            Read();
        }
        
        /// <summary>
		/// Rückgabe der Liste mit nicht ausführbaren SQL Befehlen
		/// </summary>
		public IList SqlFailures => sqlFailures;

        /// <summary>
		/// Liest die Datei ein und hängt die gesammelten SQL Befehle in
		/// ein Array
		/// </summary>
		protected virtual void Read()
		{
			string sqlCommand = "";
			string line;
		    char[] replacement = new char[] {' ', ';'};

			while ((line = input.ReadLine()) != null)
			{
				line = line.Trim();
				if ((!line.StartsWith("--")) && (line.Length > 0))
				{
					sqlCommand += line + " ";
					if (line.EndsWith(";"))
					{
					    sqlCommand = sqlCommand.Trim(replacement);
						sqlCommands.Add(sqlCommand);
						sqlCommand = "";
					}
				}
			}
		}

        /// <summary>
        /// Called when [SQL file failed].
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="continueExecution">if set to <c>true</c> [continue execution].</param>
        protected virtual void OnSqlFileFailed(string sql, SqlCoreException exception, out bool continueExecution)
        {
            continueExecution = sql.ToLower().StartsWith("drop") || sql.ToLower().StartsWith("create");
        }

        /// <summary>
        /// Führt das Script auf der übergebenen Datenbank aus
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True, if the execution succeeded</returns>
		public void ExecuteScript(INativePersister database)
		{
            ExecuteScript(database, OnSqlFileFailed);
		}

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="sqlFileFailedDelegate">The SQL file failed delegate.</param>
        /// <returns>True, if the execution succeeded</returns>
        public virtual void ExecuteScript(INativePersister database, SqlFileFailed sqlFileFailedDelegate)
        {
            foreach (string sql in sqlCommands)
            {
                try
                {
                    database.Execute(sql);
                }
                catch (SqlCoreException exc)
                {
                    sqlFailures.Add("Sql: " + exc.Message);
              
                    bool continueExecution;
                    sqlFileFailedDelegate(sql, exc, out continueExecution);

                    if (!continueExecution)
                        throw;
                }
            }
        }

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:AdFactum.Data.XmlPersister.XmlPersister"/> is reclaimed by garbage collection.
        /// </summary>
        ~SqlFile()
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
                if (input != null)
                {
                    input.Close();
					input.Dispose();
                    input = null;
                }
            }

            // free unmanaged resources
        }

		#endregion
	}
}