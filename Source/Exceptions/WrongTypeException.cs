using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// This exception will be thrown if the factory creates a wrong object type
	/// </summary>
    [Serializable]
    public class WrongTypeException : MapperBaseException
	{
		private const string MESSAGE = "The ObjectMapper .NET found an object of a wrong type. \nExpected Type: {0}\nFound Type: {1}";

		/// <summary>
		/// Initializes a new instance of the <see cref="WrongTypeException"/> class.
		/// </summary>
		public WrongTypeException(Type expectedType, Type createdType)
			:base(string.Format(MESSAGE, expectedType.Name, createdType.Name))
		{
		}
	}
}
