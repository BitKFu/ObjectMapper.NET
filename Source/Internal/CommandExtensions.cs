using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Extension methods for IDbCommands
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Disposes all parameters of a command (if the command is disposable)
        /// </summary>
        /// <param name="command">Command with parameters to dispose</param>
        public static void DisposeSafe(this IDbCommand command)
        {
            if (null == command) return;

            foreach (var commandParameter in command.Parameters)
            {
                if (commandParameter is IDisposable)
                {
                    (commandParameter as IDisposable).Dispose();
                }
            }

            command.Dispose();
        }
    }
}
