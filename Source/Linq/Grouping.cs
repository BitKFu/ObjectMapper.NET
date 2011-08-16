using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq
{
    /// <summary>
    /// Grouping Implementation
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private IEnumerable<TElement> group;
        private TKey key;

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping&lt;TKey, TElement&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="group">The group.</param>
        public Grouping(TKey key, IEnumerable<TElement> group)
        {
            this.key = key;
            this.group = group;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping&lt;TKey, TElement&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="group">The group.</param>
        public Grouping(TKey key, IList group)
        {
            this.key = key;
            this.group = new List<TElement>(new ListAdapter<TElement>(group));
        }

        #region IGrouping<TKey,TElement> Members

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public TKey Key
        {
            get { return key; }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            if (!(group is List<TElement>))
                group = group.ToList();
            return group.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return group.GetEnumerator();
        }

        #endregion
    }
}