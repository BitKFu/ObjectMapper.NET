using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Postgres
{
    public class PostgresRepository : BaseRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresRepository(ISqlTracer tracer) 
            : base(tracer)
        {
        }
    }
}
