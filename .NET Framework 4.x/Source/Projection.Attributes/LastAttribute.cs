using System;
using AdFactum.Data.Interfaces;

namespace AdFactum.Data.Projection.Attributes
{
    /// <summary>
    /// Returns the value of the last record in a specified field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class LastAttribute : Attribute, IAggregate
    {
        /// <summary>
        /// Gets the aggregation.
        /// </summary>
        /// <value>The aggregation.</value>
        public string Aggregation
        {
            get { return "Last({0})"; }
        }
    }
}
