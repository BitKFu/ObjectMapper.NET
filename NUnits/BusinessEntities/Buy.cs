using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Defines a buy
    /// </summary>
    public class Buying : MarkedAutoIncValueObject
    {
        private int count;
        private string item;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buying"/> class.
        /// </summary>
        public Buying()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buying"/> class.
        /// </summary>
        /// <param name="countParam">The count param.</param>
        /// <param name="itemParam">The item param.</param>
        public Buying(int countParam, string itemParam)
        {
            count = countParam;
            item = itemParam;
        }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public string Item
        {
            get { return item; }
            set { item = value; }
        }
    }
}
