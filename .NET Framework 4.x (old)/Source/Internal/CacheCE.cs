using System.Collections.Generic;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Util
{
    ///<summary>
    ///</summary>
    public class Cache<TKey, TElement> : BaseCache where TElement : class
    {
        /// <summary>
        /// Defines the private cache
        /// </summary>
        private Dictionary<TKey, TElement> cacheDictionary = new Dictionary<TKey, TElement>();

        /// <summary>
        /// Get Counter, tells the developer how many cache access occured.
        /// </summary>
        private int getCounter = 0;

        /// <summary>
        /// Name of the Cache
        /// </summary>
        private readonly string cacheName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache&lt;TKey, TElement&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Cache(string name)
        {
            cacheName = name;
        }

        /// <summary>
        /// Outputs the info.
        /// </summary>
        public override string CacheInfo
        {
            get
            {
                return string.Concat(getCounter, " Gets on ", cacheName, " Cache <" + typeof(TKey), ",", typeof(TElement));
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            lock(this)
            {
                bool contains = cacheDictionary.ContainsKey(key);
                return contains;
            }
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TElement Get(TKey key)
        {
            lock(this)
            {
                getCounter++;
                
                TElement result;
                if (cacheDictionary.TryGetValue(key, out result))
                    return result;

                return null;
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            lock(this)
            {
                if (cacheDictionary.ContainsKey(key))
                {
                    cacheDictionary.Remove(key);
                }
            }
        }

        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="current">The current.</param>
        public void Insert(TKey key, TElement current)
        {
            lock(this)
            {
                if (cacheDictionary.ContainsKey(key))
                    cacheDictionary.Remove(key);
                
                cacheDictionary.Add(key, current);
            }
        }

        /// <summary>
        /// Gets the count active objects.
        /// </summary>
        /// <value>The count active objects.</value>
        public int CountActiveObjects
        {
            get
            {
                lock (this)
                {
                    return cacheDictionary.Values.Count;
                }
            }
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        public override void RemoveAll()
        {
            lock (this)
            {
                cacheDictionary = new Dictionary<TKey, TElement>();
            }
        }


        /// <summary>
        /// Locks the cache.
        /// </summary>
        public override void LockCache()
        {
            // No transactional locking mechanism available for CE
        }

        /// <summary>
        /// Unlocks the cache.
        /// </summary>
        public override void UnlockCache()
        {
            // No transactional locking mechanism available for CE 
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TElement result)
        {
            lock (this)
            {
                return cacheDictionary.TryGetValue(key, out result);
            }
        }
    }
}
