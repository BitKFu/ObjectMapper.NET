
namespace AdFactum.Data.Linq
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TParent">The type of the parent.</typeparam>
    /// <typeparam name="TClient">The type of the client.</typeparam>
    public class LinkBridge <TParent, TClient>
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        [Column("PARENTOBJECT")]
        public TParent Parent { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Column("PROPERTY")]
        public TClient Client { get; set; }
    }
}
