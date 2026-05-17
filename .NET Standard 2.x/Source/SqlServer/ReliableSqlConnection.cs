using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Connection wrapper that applies SQL retry logic.
    /// </summary>
    public class ReliableSqlConnection : IDbConnection
    {
        private readonly SqlConnection connection;
        private readonly SqlRetryLogicBaseProvider connectionRetryProvider;
        private readonly SqlRetryLogicBaseProvider commandRetryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableSqlConnection"/> class.
        /// </summary>
        public ReliableSqlConnection(string connectionString, SqlRetryLogicOption connectionRetryPolicy, SqlRetryLogicOption commandRetryPolicy)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));

            connection = new SqlConnection(connectionString);
            connectionRetryProvider = CreateRetryProvider(connectionRetryPolicy);
            commandRetryProvider = CreateRetryProvider(commandRetryPolicy) ?? connectionRetryProvider;
            connection.RetryLogicProvider = connectionRetryProvider;
        }

        /// <summary>
        /// Gets the current SQL connection.
        /// </summary>
        public SqlConnection Current => connection;

        /// <summary>
        /// Creates a retry provider from the given option.
        /// </summary>
        public static SqlRetryLogicBaseProvider CreateRetryProvider(SqlRetryLogicOption retryLogicOption)
        {
            if (retryLogicOption == null)
                return null;

            return SqlConfigurableRetryFactory.CreateExponentialRetryProvider(retryLogicOption);
        }

        /// <summary>
        /// Applies command retry logic to the given command.
        /// </summary>
        public void ConfigureCommand(SqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.RetryLogicProvider = commandRetryProvider;
        }

        public string ConnectionString
        {
            get => connection.ConnectionString;
            set => connection.ConnectionString = value;
        }

        public int ConnectionTimeout => connection.ConnectionTimeout;

        public string Database => connection.Database;

        public ConnectionState State => connection.State;

        public string ServerVersion => connection.ServerVersion;

        public IDbTransaction BeginTransaction()
        {
            return connection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return connection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            connection.Close();
        }

        public IDbCommand CreateCommand()
        {
            var command = connection.CreateCommand();
            ConfigureCommand(command);
            return command;
        }

        public void Open()
        {
            connection.Open();
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
