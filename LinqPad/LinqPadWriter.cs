using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper2LinqPad
{
    /// <summary>
    /// 
    /// </summary>
    public class LinqPadWriter : ISqlTracer
    {
        private TextWriter Writer { get; set; }
        
        public LinqPadWriter (TextWriter writer)
        {
            Writer = writer;
        }

        public void Dispose()
        {
        }

        public bool TraceErrorEnabled
        {
            get { return true; }
        }

        public bool TraceSqlEnabled
        {
            get { return true; }
        }

        public void OpenConnection(string serverVersion, string connection)
        {
        }

        public void SqlCommand(IDbCommand command, string extended, int affactedRows, TimeSpan duration)
        {
            Writer.WriteLine("Extended: " + extended);
            Writer.WriteLine("Original: " + command.CommandText);
            Writer.WriteLine("");
        }

        public void ErrorMessage(string message, string source)
        {
            Writer.WriteLine("ERROR : " + message);
            Writer.WriteLine("SOURCE: " + source);
            Writer.WriteLine("");
        }

        public void BeginTransaction()
        {
            
        }

        public void Commit()
        {
            
        }

        public void Rollback()
        {
            
        }
    }
}
