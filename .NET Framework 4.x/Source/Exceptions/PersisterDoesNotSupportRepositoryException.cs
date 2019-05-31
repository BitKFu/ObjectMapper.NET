using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Summary description for PersisterDoesNotSupportRepositoryException.
	/// </summary>
    [Serializable]
    public class PersisterDoesNotSupportRepositoryException : MapperBaseException
	{
		private const string MESSAGE = "The used persister does not support the IRepository interface.";

		/// <summary>
		/// Initializes a new instance of the <see cref="PersisterDoesNotSupportRepositoryException"/> class.
		/// </summary>
		public PersisterDoesNotSupportRepositoryException()
			:base(MESSAGE)
		{
		}
	}
}
