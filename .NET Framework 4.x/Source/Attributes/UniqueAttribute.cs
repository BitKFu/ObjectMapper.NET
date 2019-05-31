using System;
using AdFactum.Data.Core.Attributes;

namespace AdFactum.Data
{
	/// <summary>
	/// If this attribute is set, the database field is marked as unique
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	[Serializable]
    public sealed class UniqueAttribute : KeyGroupAttribute
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueAttribute"/> class.
        /// </summary>
	    public UniqueAttribute ()
            :base(0,0)
	    {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        public UniqueAttribute(int keyGroupParameter)
            :base(keyGroupParameter, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="orderInKeyGroupParameter">The order in key group parameter.</param>
        public UniqueAttribute(int keyGroupParameter, int orderInKeyGroupParameter)
            :base(keyGroupParameter, orderInKeyGroupParameter)
	    {
	    }
	}
}