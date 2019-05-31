using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Summary description for ValidSinceAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	[Serializable]
    [Obsolete("This flag won't supported anymore in future versions.")]
    public sealed class ValidSinceAttribute : Attribute
	{
		private readonly int majorVersion;
		private readonly int minorVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidSinceAttribute"/> class.
        /// </summary>
	    internal ValidSinceAttribute()
	    {
	        
	    }
	    
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="_version"></param>
		public ValidSinceAttribute(double _version)
		{
			majorVersion = (int) Math.Floor(_version);							
			minorVersion = (int) ((_version - majorVersion)*10000+50)/100;		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidUntilAttribute"/> class.
		/// </summary>
		/// <param name="_majorVersion">The _major version.</param>
		/// <param name="_minorVersion">The _minor version.</param>
		public ValidSinceAttribute(string _majorVersion, string _minorVersion)
		{
			majorVersion = int.Parse(_majorVersion);

			if (_minorVersion.Length == 1)
				_minorVersion += "0";

			minorVersion = int.Parse(_minorVersion);
		}

		/// <summary>
		/// Returns the version information
		/// </summary>
		public int MajorVersion
		{
			get { return majorVersion; }
		}

		/// <summary>
		/// Returns the version information
		/// </summary>
		public int MinorVersion
		{
			get { return minorVersion; }
		}
	}
}