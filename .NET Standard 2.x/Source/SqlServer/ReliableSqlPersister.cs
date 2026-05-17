using System;
using System.Data;
using System.Data.Common;
using AdFactum.Data.Exceptions;
using Microsoft.Data.SqlClient;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Persister that is used for azure DBs
    /// </summary>
    public class ReliableSqlPersister : SqlPersister
    {
        private static readonly SqlRetryLogicOption defaultConnectionRetryPolicy = new SqlRetryLogicOption
        {
            NumberOfTries = 4,
            MinTimeInterval = TimeSpan.FromMilliseconds(100),
            MaxTimeInterval = TimeSpan.FromSeconds(30),
            DeltaTime = TimeSpan.FromSeconds(1),
        };

        private static readonly SqlRetryLogicOption defaultCommandRetryPolicy = new SqlRetryLogicOption
        {
            NumberOfTries = 4,
            MinTimeInterval = TimeSpan.FromMilliseconds(100),
            MaxTimeInterval = TimeSpan.FromSeconds(30),
            DeltaTime = TimeSpan.FromSeconds(1),
        };

        /// <summary>
        /// Gets or sets the retry policy for the connection.
        /// </summary>
        public SqlRetryLogicOption ConnectionRetryPolicy { get; set; } = defaultConnectionRetryPolicy;

        /// <summary>
        /// Gets or sets the retry policy for the command.
        /// </summary>
        public SqlRetryLogicOption CommandRetryPolicy { get; set; } = defaultCommandRetryPolicy;

        /// <summary>
        /// Connects to a Microsoft SQL Server using an Connection String
        /// </summary>
        /// <param name="connectionString"></param>
        public override void Connect(string connectionString)
        {
            Connection = new ReliableSqlConnection(connectionString,
                ConnectionRetryPolicy ?? defaultConnectionRetryPolicy,
                CommandRetryPolicy ?? defaultCommandRetryPolicy);
            ((ReliableSqlConnection)Connection).Open();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((ReliableSqlConnection)Connection).Current.ServerVersion, Connection.ConnectionString);
        }

        /// <summary>
        /// Creates the command object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override IDbCommand CreateCommand(string sql)
        {
            var sqlConnection = ((ReliableSqlConnection)Connection).Current;
            var command = new SqlCommand(sql, sqlConnection)
            {
                Transaction = (SqlTransaction)Transaction
            };

            ((ReliableSqlConnection)Connection).ConfigureCommand(command);

            if (CommandTimeout != null)
                command.CommandTimeout = CommandTimeout.Value;

            return command;
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public override IDbCommand CreateCommand()
        {
            var sqlConnection = ((ReliableSqlConnection)Connection).Current;
            var command = new SqlCommand
            {
                Connection = sqlConnection,
                Transaction = (SqlTransaction)Transaction
            };

            ((ReliableSqlConnection)Connection).ConfigureCommand(command);

            if (CommandTimeout != null)
                command.CommandTimeout = CommandTimeout.Value;

            return command;
        }

        /// <summary>
        /// Executes the secure db call.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="nonQuery">if set to <c>true</c> [non query].</param>
        /// <returns></returns>
        protected override object ExecuteSecureDbCall(IDbCommand command, bool nonQuery)
        {
            var sqlCommand = command as SqlCommand;
            if (sqlCommand == null)
                return base.ExecuteSecureDbCall(command, nonQuery);

            sqlCommand.CommandText = ReplaceStatics(sqlCommand.CommandText);
            try
            {
                return nonQuery
                    ? (object)sqlCommand.ExecuteNonQuery()
                    : sqlCommand.ExecuteReader();
            }
            catch (DbException exc)
            {
                ErrorMessage(exc);
                throw new SqlCoreException(exc, exc.ErrorCode, CreateSql(sqlCommand));
            }
            catch (Exception exc)
            {
                ErrorMessage(exc);
                throw new SqlCoreException(exc, 0, CreateSql(sqlCommand));
            }
        }
    }
}
