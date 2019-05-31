using System;
using System.Collections.Generic;

namespace AdFactum.Data.Interfaces
{
	/// <summary>
	/// This interface provides methods to write the repository meta information
	/// </summary>
	public interface IRepository
	{
		/// <summary>
		/// Writes the repository.
		/// </summary>
		/// <param name="mapper">The mapper.</param>
		/// <param name="types">The types.</param>
        void WriteRepository(ObjectMapper mapper, IEnumerable<Type> types);

        /// <summary>
        /// Gets the repository types.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetRepositoryTypes();
	}
}
