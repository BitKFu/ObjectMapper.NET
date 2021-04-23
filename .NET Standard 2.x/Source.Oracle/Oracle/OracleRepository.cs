using System;
using System.Collections.Generic;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Oracle
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleRepository : BaseRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracer"></param>
        public OracleRepository(ISqlTracer tracer) 
            : base(tracer)
        {
        }

        /// <summary>
        /// Gets the repository types.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Type> GetRepositoryTypes()
        {
            var repositoryTypes = new List<Type>
                                      {
                                          typeof (VersionInfo),
                                          typeof (TableStorage),
                                          typeof (Tablespace),
                                          typeof (EntityInfo),
                                          typeof (EntityPredicate),
                                          typeof (EntityRelation)
                                      };
            return repositoryTypes;
        }
    }
}
