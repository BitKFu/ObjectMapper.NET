﻿using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Access
{
    /// <summary>
    /// 
    /// </summary>
    public class AccessIntegrityChecker : BaseIntegrityChecker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AccessIntegrityChecker(INativePersister persister, ITypeMapper typeMapper, string databaseSchema) 
            : base(persister, typeMapper, databaseSchema)
        {
        }


        /// <summary>
        /// Gets the integrity URL for Microsoft Access
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected override string GetIntegritySql(string schema, string tableName) => string.Concat("SELECT * FROM ", ConcatedSchema, TypeMapper.Quote(tableName));


        /// <summary>
        /// Gets the integrity of an object type.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
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

            // Everything that is longer than 255, will get a memo
            info.MismatchedFields.RemoveAll(field => field.FieldIsShorter && field.Field.CustomProperty.MetaInfo.Length > 255);

            return infoCollection;
        }

        /// <summary>
        /// Calculates the size of the unicode.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected override int CalculateUnicodeSize(int size)
        {
            if (size == int.MaxValue / 4-1)
                size = int.MaxValue/2;

            return size;
        }

        /// <summary>
        /// Calculates the size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected override int CalculateSize(int size)
        {
            if (size == int.MaxValue / 4 - 1)
                size = int.MaxValue ;

            return size;
        }
    }
}
