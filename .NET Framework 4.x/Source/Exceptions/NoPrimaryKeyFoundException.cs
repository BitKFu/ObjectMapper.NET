using System;


namespace AdFactum.Data.Exceptions
{
    /// <summary>
    /// This exception will be thrown if no primary key can be found
    /// </summary>
    [Serializable]
    public class NoPrimaryKeyFoundException : MapperBaseException
    {
		private const string MESSAGE = "No primary key found for type {0}";

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingSetterException"/> class.
		/// </summary>
        public NoPrimaryKeyFoundException(Type objectType)
			:base(string.Format(MESSAGE, objectType.Name))
		{
		}
    }
}
