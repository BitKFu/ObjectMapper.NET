using System;
using System.Runtime.Serialization;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Base Exception used by all Mapper Exceptions
	/// </summary>
    [Serializable]
    public class MapperBaseException : Exception
    {
        /// <summary>
		/// Bases the exception.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="baseException">The base exception.</param>
		public MapperBaseException(string message, Exception baseException)
			:base(message, baseException)
		{
		}

		/// <summary>
		/// Bases the exception.
		/// </summary>
		/// <param name="message">The message.</param>
		public MapperBaseException(string message)
			:base(message)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperBaseException"/> class.
        /// </summary>
        public MapperBaseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCoreException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        public MapperBaseException(SerializationInfo info, StreamingContext context)
            :base(info.GetString("Message"))
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is a null reference (Nothing in Visual Basic). </exception>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Message", Message);
        }
	}
}
