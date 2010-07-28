using System;

namespace AdFactum.Data
{
    /// <summary>
    /// Class Attribute to define that the class is mapped to a database view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class ViewAttribute : TableAttribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
	    internal ViewAttribute()
	    {
	    }
	    
		/// <summary>
		/// Attribute Constructor
		/// </summary>
		/// <param name="pName">Table Name</param>
        public ViewAttribute(String pName)
            :base(pName)
		{
		}

    }
}
