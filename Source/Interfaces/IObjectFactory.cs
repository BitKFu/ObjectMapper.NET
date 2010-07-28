using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Interface für eine Object Factory
	/// </summary>
	public interface IObjectFactory
	{
		/// <summary>
		/// Creates a new object from a given type name
		/// </summary>
		/// <param name="typeName">type name</param>
		/// <returns>Returns the created object</returns>
		object Create(string typeName);

		/// <summary>
		/// Creates a new object from a given object type
		/// </summary>
		/// <param name="typeName">Object type</param>
		/// <returns>Returns the created object</returns>
		object Create(Type typeName);

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
	    Type GetType(string typeName);
	}
}