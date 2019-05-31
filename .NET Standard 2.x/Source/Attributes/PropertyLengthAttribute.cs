using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Summary description for FieldLengthAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public sealed class PropertyLengthAttribute : Attribute
	{
		private readonly int length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyLengthAttribute"/> class.
        /// </summary>
	    internal PropertyLengthAttribute()
	    {
	        
	    }
	    
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="length"></param>
		public PropertyLengthAttribute(int length)
		{
			this.length = length;
		}

		/// <summary>
		/// Returns the valid field length
		/// </summary>
		public int Length
		{
			get { return length; }
		}
	}
}