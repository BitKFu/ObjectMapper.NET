using System;
using System.Collections;
using System.Collections.Generic;

namespace AdFactum.Data.Util
{
    ///<summary>
    /// That's an implementation for an ultra fast dictionary, because it does not need to be initialized
    ///</summary>
    ///<typeparam name="TKey"></typeparam>
    ///<typeparam name="TValue"></typeparam>
    public class UltraFastDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal Dictionary<TKey, TValue> InnerDictionary = new Dictionary<TKey, TValue>();

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the dictionary enumerator.
        /// </summary>
        /// <returns></returns>
        public IDictionaryEnumerator GetDictionaryEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }
    }

    /// <summary>
    /// Implementation for the Ultra Fast Dictionary
    /// </summary>
    public static class UltraFastDictionaryImpl
    {
        /// <summary>
        /// Valueses the specified dict.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue>.ValueCollection Values<TKey, TValue>(
            this UltraFastDictionary<TKey, TValue> dict)
        {
            return dict != null
                       ? dict.InnerDictionary.Values
                       : null;
        }

        /// <summary>
        /// Adds the specified dict.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static UltraFastDictionary<TKey, TValue> Add<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict,
                                                                          TKey key, TValue value)
        {
            if (dict == null)
                dict = new UltraFastDictionary<TKey, TValue>();

            dict.InnerDictionary.Add(key, value);
            return dict;
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetValue<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key,
                                                     out TValue value)
        {
            if (dict == null)
            {
                value = default(TValue);
                return false;
            }

            return dict.InnerDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the specified dict.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return default(TValue);

            return dict.InnerDictionary[key];
        }

        /// <summary>
        /// Removes the specified dict.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        public static void Remove<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return;

            dict.InnerDictionary.Remove(key);
        }

        /// <summary>
        /// Determines whether [contains] [the specified dict].
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dict.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified dict]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return false;

            return dict.InnerDictionary.ContainsKey(key);
        }
    }
}