using System.Collections.Generic;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;

namespace AdFactum.Data.SqlServer
{
    public class SqlIntegrityChecker : BaseIntegrityChecker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SqlIntegrityChecker(INativePersister persister, ITypeMapper typeMapper, string databaseSchema) 
            : base(persister, typeMapper, databaseSchema)
        {
        }

        /// <summary>
        /// Gets the integrity of an object type.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        protected override IEnumerable<IntegrityInfo> CheckIntegrity(IntegrityInfo info)
        {
            IEnumerable<IntegrityInfo> result = base.CheckIntegrity(info);

            // The SQL Server does not deliver correct Informations about the Unique keys 
            info.MismatchedFields.RemoveAll(field => field.UniqueFailure);

            return result;
        }

    }
}
