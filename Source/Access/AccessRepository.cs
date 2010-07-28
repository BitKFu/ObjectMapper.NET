using AdFactum.Data.Internal;

namespace AdFactum.Data.Access
{
    /// <summary>
    /// Used to write the repository for Access Databasees
    /// </summary>
    public class AccessRepository : BaseRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracer"></param>
        public AccessRepository(ISqlTracer tracer) 
            : base(tracer)
        {
        }
    }
}
