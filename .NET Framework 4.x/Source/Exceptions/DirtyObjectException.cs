using System;
using AdFactum.Data.Exceptions;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// The dirty object exception will be thrown if the mapper tries to store object that has been modified by an other person.
	/// Technical explanation: <br/>
	/// This exception will be thrown if a marked value object is changed, but the database has an other last update timestamp.
	/// </summary>
	[Serializable]
    public class DirtyObjectException : MapperBaseException
	{
		/// <summary>
		/// Constructor for creating a new dirty object exception
		/// </summary>
		/// <param name="description"></param>
		public DirtyObjectException(string description)
			: base(description)
		{
		}
	}
}