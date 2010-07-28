using System;

namespace AdFactum.Data.Core.Attributes
{
    /// <summary>
    /// Base class used to define attribute groups and sortings within that group.
    /// Used for Unique Keys, Foreign Keys and aggregation attributes.
    /// </summary>
    [Serializable]
    public abstract class KeyGroupAttribute : PositionAttribute
    {
        private readonly int keyGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyGroupAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="orderInKeyGroupParamter">The order in key group paramter.</param>
        protected KeyGroupAttribute(int keyGroupParameter, int orderInKeyGroupParamter)
            :base(orderInKeyGroupParamter)
        {
            keyGroup = keyGroupParameter;
        }

        /// <summary>
        /// Gets the key group.
        /// </summary>
        /// <value>The key group.</value>
        public int KeyGroup
        {
            get { return keyGroup; }
        }

    }
}
