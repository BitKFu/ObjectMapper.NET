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

        /// <summary>
        /// Calculates the size of the unicode.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected override int CalculateUnicodeSize(int size)
        {
            return size == int.MaxValue ? size/2 : size;
        }
    }
}
