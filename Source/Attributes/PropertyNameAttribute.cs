using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Summary description for FieldLengthAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public sealed class PropertyNameAttribute : Attribute
	{
		private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNameAttribute"/> class.
        /// </summary>
	    internal PropertyNameAttribute()
	    {
	        
	    }
	    
		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="name"></param>
		public PropertyNameAttribute(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Returns the valid property name
		/// </summary>
		public string Name
		{
			get { return name; }
		}
	}
}