using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.SqlServer
{
    public class SqlRepository : BaseRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SqlRepository(ISqlTracer tracer) 
            : base(tracer)
        {
        }
    }
}
