using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Interface that is internally used for interacting with the object cache.
	/// </summary>
	public interface IObjectHash
	{
		/// <summary>
		/// This method checks, if a value object is stored within the object cache.
		/// </summary>
		/// <param name="vo">Fieldvalue Object</param>
		/// <returns>
		/// Returns true, if the value object is stored within the object cache.
		/// </returns>
		bool Contains(object vo);

        /// <summary>
        /// Returns the value object with the primary key given by the paramter ID.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <returns>Fieldvalue Object</returns>
		object GetVO(Type type, object primaryKey);
	}
}