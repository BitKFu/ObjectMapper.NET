using System.Data;
using System.Diagnostics;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// The SQL StopWatch will be used in order to trace the time from execution until all objects have been mapped
    /// </summary>
    public class SqlStopwatch 
    {
        /// <summary>
        /// Gets or sets a valid Sql Tracer
        /// </summary>
        private ISqlTracer SqlTracer { get; set; }
        
        /// <summary>
        /// Creates a stopwatch
        /// </summary>
        private readonly Stopwatch stopwatch;

        /// <summary>
        /// Creates a new Sql Stopwatch
        /// </summary>
        /// <param name="tracer"></param>
        public SqlStopwatch(ISqlTracer tracer)
        {
            SqlTracer = tracer;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        /// <summary>
        /// Set the SQL Trace data, before the Stopwatch gets disposed
        /// </summary>
        /// <param name="command">command that has been executed</param>
        /// <param name="extendedSql">Extended Sql</param>
        /// <param name="rows">Amount of rows</param>
        public void Stop(IDbCommand command, string extendedSql, int rows)
        {
            stopwatch.Stop();
            if (SqlTracer == null || command == null || !SqlTracer.TraceSqlEnabled)
                return;

            SqlTracer.SqlCommand(command, extendedSql + ";", rows, stopwatch.Elapsed);
        }
    }
}
