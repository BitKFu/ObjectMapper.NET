using System;
using Oracle.ManagedDataAccess.Client;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Extension methods for IDbCommands
    /// </summary>
    public static class OracleCommandExtensions
    {
        /// <summary>
        /// Disposes all parameters of a command (if the command is disposable)
        /// </summary>
        /// <param name="command">Command with parameters to dispose</param>
        public static void DisposeSafe(this OracleCommand command)
        {
            if (null == command) return;

            foreach (var commandParameter in command.Parameters)
            {
                // Check for OracleParameter and dispose value explicitly
                var oracleParam = commandParameter as OracleParameter;
                if (oracleParam != null && oracleParam.Value is IDisposable)
                    ((IDisposable)oracleParam.Value).Dispose();

                // Dispose command
                if (commandParameter is IDisposable)
                    ((IDisposable)commandParameter).Dispose();
            }

            command.Dispose();
        }
    }
}
