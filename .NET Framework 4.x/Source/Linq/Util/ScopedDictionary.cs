using System.Collections.Generic;

namespace AdFactum.Data.Linq.Util
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ScopedDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> map;
        private readonly ScopedDictionary<TKey, TValue> previous;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        public ScopedDictionary(ScopedDictionary<TKey, TValue> previous)
        {
            this.previous = previous;
            map = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="pairs">The pairs.</param>
        public ScopedDictionary(ScopedDictionary<TKey, TValue> previous, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
            : this(previous)
        {
            foreach (var p in pairs)
            {
                map.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            map.Add(key, value);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            for (ScopedDictionary<TKey, TValue> scope = this; scope != null; scope = scope.previous)
            {
                if (scope.map.TryGetValue(key, out value))
                    return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            for (ScopedDictionary<TKey, TValue> scope = this; scope != null; scope = scope.previous)
            {
                if (scope.map.ContainsKey(key))
                    return true;
            }
            return false;
        }
    }
}