using System;
using System.Collections.Generic;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Interfaces
{
    /// <summary>
    /// This interface provides methods to check the integrity of the datamodel
    /// </summary>
    public interface IIntegrity
    {
        /// <summary>
        /// Checks the integrity.
        /// </summary>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        IEnumerable<IntegrityInfo> CheckIntegrity(IEnumerable<Type> persistentTypes, ObjectMapper mapper);
    }
}
