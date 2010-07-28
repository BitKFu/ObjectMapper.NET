using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Postgres
{
    public class PostgresIntegrityChecker : BaseIntegrityChecker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresIntegrityChecker(INativePersister persister, ITypeMapper typeMapper, string databaseSchema) 
            : base(persister, typeMapper, databaseSchema)
        {
        }
    }
}
