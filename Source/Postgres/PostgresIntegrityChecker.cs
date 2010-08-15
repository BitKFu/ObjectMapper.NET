using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;

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

        /// <summary>
        /// Gets the integrity of an object type.
        /// </summary>
        protected override IEnumerable<IntegrityInfo> CheckIntegrity(IntegrityInfo info)
        {
            var infoCollection = base.CheckIntegrity(info);
            info = infoCollection.First();

            /*
             * Remove all Required Failures for Microsoft Access, because the 
             * ADO .NET Provider does not read the AllowDBNull Value in Schema correctly
             */
            info.MismatchedFields.RemoveAll(field => field.RequiredFailure);
            info.MismatchedFields.RemoveAll(field => field.UniqueFailure);

            return infoCollection;
        }

        protected override int CalculateSize(int size)
        {
            if (size == -1)
                size = int.MaxValue;

            return size;
        }

        protected override int CalculateUnicodeSize(int size)
        {
            if (size == -1)
                size = int.MaxValue/2;

            return size;
        }

    }
}
