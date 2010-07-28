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

        public Grouping(TKey key, IEnumerable<TElement> group)
        {
            this.key = key;
            this.group = group;
        }

        public Grouping(TKey key, IList group)
        {
            this.key = key;
            this.group = new List<TElement>(new ListAdapter<TElement>(group));
        }

        #region IGrouping<TKey,TElement> Members

        public TKey Key
        {
            get { return key; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            if (!(group is List<TElement>))
                group = group.ToList();
            return group.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return group.GetEnumerator();
        }

        #endregion
    }
}