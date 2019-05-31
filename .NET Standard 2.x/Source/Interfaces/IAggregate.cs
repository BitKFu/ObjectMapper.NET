
namespace AdFactum.Data.Interfaces
{
    /// <summary>
    /// Specifies that the attribute is used as an aggregation function
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Gets the aggregation.
        /// </summary>
        /// <value>The aggregation.</value>
        string Aggregation { get; }
    }
}
