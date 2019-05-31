using System;
using AdFactum.Data.Interfaces;

namespace AdFactum.Data.Projection.Attributes
{
    /// <summary>
    /// Returns the total sum of a column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class SumAttribute : Attribute, IAggregate
    {
        /// <summary>
        /// Gets the aggregation.
        /// </summary>
        /// <value>The aggregation.</value>
        public string Aggregation
        {
            get { return "Sum({0})"; }
        }
    }
}
