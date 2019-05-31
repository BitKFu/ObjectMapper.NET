using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Summary description for NoOpenTransactionException.
	/// </summary>
    [Serializable]
    public class NoOpenTransactionException : MapperBaseException
	{
		private const string MESSAGE = "You have to open a transaction first.";

		/// <summary>
		/// Initializes a new instance of the <see cref="NoOpenTransactionException"/> class.
		/// </summary>
		public NoOpenTransactionException()
			:base(MESSAGE)
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
