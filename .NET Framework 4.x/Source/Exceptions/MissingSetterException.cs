using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Summary description for MissingSetterException.
	/// </summary>
    [Serializable]
    public class MissingSetterException : MapperBaseException
	{
		private const string MESSAGE = "Missing property setter for {0}.{1}";

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingSetterException"/> class.
		/// </summary>
		public MissingSetterException(Type objectType, string propertyName)
			:base(string.Format(MESSAGE, objectType.Name, propertyName))
		{
		}
	}
}
