using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Summary description for TransactionAlreadyOpenException.
	/// </summary>
    [Serializable]
    public class TransactionAlreadyOpenException : MapperBaseException
	{
		private const string MESSAGE = "A transaction is already open.";

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionAlreadyOpenException"/> class.
		/// </summary>
		public TransactionAlreadyOpenException()
			:base(MESSAGE)
		{
		}
	}
}
