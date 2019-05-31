using System;
using AdFactum.Data.Interfaces;

namespace AdFactum.Data.Projection.Attributes
{
    /// <summary>
    /// Returns the number of rows (without a NULL value) of a column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class CountAttribute : Attribute, IAggregate
    {
        private bool distinct;

        ///<summary>
        /// Default Constructor
        ///</summary>
        public CountAttribute()
        {
        }

        ///<summary>
        /// Default Constructor
        ///</summary>
        public CountAttribute(bool countDistinct)
        {
            distinct = countDistinct;
        }

        /// <summary>
        /// Gets the aggregation.
        /// </summary>
        /// <value>The aggregation.</value>
        public string Aggregation
        {
            get { return distinct ? "Count(distinct {0})" : "Count({0})"; }
        }

    }

}
