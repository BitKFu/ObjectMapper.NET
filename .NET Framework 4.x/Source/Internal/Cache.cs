using System.Collections.Generic;
using System.Threading;
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
        private Dictionary<TKey, TElement> cacheDictionary = new Dictionary<TKey,TElement>();
        
        /// <summary>
        /// Reader writer lock
        /// </summary>
        private ReaderWriterLock locked = new ReaderWriterLock();

        /// <summary>
        /// Wait infinite 
        /// </summary>
        private const int INFINITE = -1;

#if DEBUG
        /// <summary>
        /// Get Counter, tells the developer how many cache access occured.
        /// </summary>
        private int getCounter = 0;
#endif

        /// <summary>
        /// Name of the Cache
        /// </summary>
        private string cacheName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache&lt;TKey, TElement&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Cache (string name)
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
#if DEBUG
                return string.Concat(cacheName, " Cache <" + typeof (TKey), ",", typeof (TElement), ">\n",
                                     "Count :", cacheDictionary.Count,"\n",
                                     "Gets  :", getCounter,"\n");
#else
                return string.Concat(cacheName, " Cache <" + typeof (TKey), ",", typeof (TElement), ">\n",
                                     "Count :", cacheDictionary.Count, "\n");
#endif
            }
        }

        /// <summary>
        /// Tries the and get value in a single lock session.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TElement value)
        {
            locked.AcquireReaderLock(INFINITE);
            try
            {
                return cacheDictionary.TryGetValue(key, out value);
            }
            finally
            {
                locked.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TKey key)
        {
            locked.AcquireReaderLock(INFINITE);
            try
            {
                return cacheDictionary.ContainsKey(key);
            }
            finally
            {
                locked.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TElement Get(TKey key)
        {
#if DEBUG
            getCounter++;
#endif
            locked.AcquireReaderLock(INFINITE);
            try
            {
                TElement result;
                return cacheDictionary.TryGetValue(key, out result) ? result : null;
            }
            finally
            {
                locked.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            locked.AcquireReaderLock(INFINITE);
            try
            {
                if (cacheDictionary.ContainsKey(key))
                {
                    LockCookie lc = locked.UpgradeToWriterLock(INFINITE);
                    try
                    {
                        if (cacheDictionary.ContainsKey(key))
                        {
                            cacheDictionary.Remove(key);
                        }
                    }
                    finally
                    {
                        locked.DowngradeFromWriterLock(ref lc);
                    }
                }
            }
            finally
            {
                locked.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Inserts the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="current">The current.</param>
        public void Insert(TKey key, TElement current)
        {
            locked.AcquireWriterLock(INFINITE);
            try
            {
                if (cacheDictionary.ContainsKey(key))
                    cacheDictionary.Remove(key);

                cacheDictionary.Add(key, current);
            }
            finally
            {
                locked.ReleaseWriterLock();
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
                locked.AcquireReaderLock(INFINITE);
                try
                {
                    return cacheDictionary.Values.Count;
                }
                finally
                {
                    locked.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        public override void RemoveAll()
        {
            locked.AcquireReaderLock(INFINITE);
            try
            {
                LockCookie lc = locked.UpgradeToWriterLock(INFINITE);
                try
                {
                    cacheDictionary = new Dictionary<TKey, TElement>();
                }
                finally
                {
                    locked.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                locked.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Locks the cache.
        /// </summary>
        public override void LockCache()
        {
            locked.AcquireWriterLock(INFINITE);
        }

        /// <summary>
        /// Unlocks the cache.
        /// </summary>
        public override void UnlockCache()
        {
            locked.ReleaseWriterLock();
        }
    }
}
