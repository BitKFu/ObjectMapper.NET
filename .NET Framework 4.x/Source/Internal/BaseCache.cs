using System;
using System.Collections.Generic;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Registerd Cache
    /// </summary>
    internal interface IRegisteredCache
    {
        /// <summary>
        /// Removes all.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Gets the cache info.
        /// </summary>
        /// <value>The cache info.</value>
        string CacheInfo { get; }

        /// <summary>
        /// Locks the cache.
        /// </summary>
        void LockCache();

        /// <summary>
        /// Unlocks the cache.
        /// </summary>
        void UnlockCache();
    }

    /// <summary>
    /// Base class for caching
    /// </summary>
    public abstract class BaseCache : IRegisteredCache
    {
#if DEBUG
        /// <summary>
        /// List with all registered caches
        /// </summary>
        private static List<IRegisteredCache> registeredCaches = new List<IRegisteredCache>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCache"/> class.
        /// </summary>
        public BaseCache()
        {
            lock (registeredCaches)
                registeredCaches.Add(this);
        }

        /// <summary>
        /// Outputs the cache info.
        /// </summary>
        public static void OutputAllCacheInfos()
        {
            lock (registeredCaches)
            {
                Console.WriteLine("Registered Caches Count : " + registeredCaches.Count +"\n");

                foreach (IRegisteredCache cache in registeredCaches)
                    Console.WriteLine(cache.CacheInfo);
            }
        }
#endif

        #region IRegisteredCache Members

        /// <summary>
        /// Removes all.
        /// </summary>
        public abstract void RemoveAll();

        /// <summary>
        /// Gets the cache info.
        /// </summary>
        /// <value>The cache info.</value>
        public abstract string CacheInfo { get; }

        /// <summary>
        /// Locks the cache.
        /// </summary>
        public abstract void LockCache();

        /// <summary>
        /// Unlocks the cache.
        /// </summary>
        public abstract void UnlockCache();

        #endregion


        ///// <summary>
        ///// Clears all caches.
        ///// </summary>
        //public static void ClearAllCaches()
        //{
        //    lock (registeredCaches)
        //    {
        //        try
        //        {
        //            // Lock all caches
        //            foreach (IRegisteredCache cache in registeredCaches)
        //                cache.LockCache();

        //            // Delete all caches
        //            foreach (IRegisteredCache cache in registeredCaches)
        //                cache.RemoveAll();
        //        }
        //        finally
        //        {
        //            // Unlock all caches
        //            foreach (IRegisteredCache cache in registeredCaches)
        //                cache.UnlockCache();
        //        }
        //    }
        //}
    }

}
