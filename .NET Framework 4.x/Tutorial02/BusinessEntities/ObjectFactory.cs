using System;
using AdFactum.Data;

namespace BusinessEntities
{
	/// <summary>
	/// Factory class
	/// </summary>
	public class ObjectFactory : IObjectFactory
	{
		/// <summary>
		/// Creates a new object from a given type name
		/// </summary>
		/// <param name="typeName">type name</param>
		/// <returns>Returns the created object</returns>
		public object Create(string typeName)
		{
			return Create(Type.GetType(typeName));
		}

		/// <summary>
		/// Creates a new object from a given object type
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>Returns the created object</returns>
		public object Create(Type type)
		{
			return Activator.CreateInstance(type);
		}

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <param name="typeName">Name of the type.</param>
		/// <returns></returns>
		public Type GetType(string typeName)
		{
			return Type.GetType(typeName);
		}

	}
}