using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Oracle
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleIntegrityChecker : BaseIntegrityChecker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OracleIntegrityChecker(INativePersister persister, ITypeMapper typeMapper, string databaseSchema) 
            : base(persister, typeMapper, databaseSchema)
        {
        }
    }
}
