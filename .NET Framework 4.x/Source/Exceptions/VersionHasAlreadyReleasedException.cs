using System;

namespace AdFactum.Data.Exceptions
{
	/// <summary>
	/// Summary description for VersionHasAlreadyReleasedException.
	/// </summary>
    [Serializable]
    public class VersionHasAlreadyReleasedException : MapperBaseException
	{
		private const string MESSAGE = "The repository can't be written, because the release flag of version {0}.{1} has already been set.";
		
		/// <summary>
		/// Initializes a new instance of the <see cref="VersionHasAlreadyReleasedException"/> class.
		/// </summary>
		/// <param name="majorVersion">The major version.</param>
		/// <param name="minorVersion">The minor version.</param>
		public VersionHasAlreadyReleasedException(int majorVersion, int minorVersion)
			:base(string.Format(MESSAGE, majorVersion, minorVersion))
		{
		}
	}
}
