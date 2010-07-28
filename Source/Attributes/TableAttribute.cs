using System;

namespace AdFactum.Data
{
	/// <summary>
    /// Class Attribute to define that the class is mapped to a database table.
    /// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public class TableAttribute : Attribute
	{
		/// <summary>
		/// Table Name
		/// </summary>
		private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute"/> class.
        /// </summary>
	    internal TableAttribute()
	    {
	    }
	    
		/// <summary>
		/// Attribute Constructor
		/// </summary>
		/// <param name="pName">Table Name</param>
		public TableAttribute(String pName)
		{
			name = pName.ToUpper();
		}

		/// <summary>
		/// Returns the table name
		/// </summary>
		public string Name
		{
			get { return name; }
		}

	}
}