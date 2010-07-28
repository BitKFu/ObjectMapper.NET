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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

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
        public static Dictionary<TKey, TValue>.ValueCollection Values<TKey, TValue>(
            this UltraFastDictionary<TKey, TValue> dict)
        {
            return dict != null
                       ? dict.InnerDictionary.Values
                       : null;
        }

        public static UltraFastDictionary<TKey, TValue> Add<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict,
                                                                          TKey key, TValue value)
        {
            if (dict == null)
                dict = new UltraFastDictionary<TKey, TValue>();

            dict.InnerDictionary.Add(key, value);
            return dict;
        }

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

        public static TValue Get<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return default(TValue);

            return dict.InnerDictionary[key];
        }

        public static void Remove<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return;

            dict.InnerDictionary.Remove(key);
        }

        public static bool Contains<TKey, TValue>(this UltraFastDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
                return false;

            return dict.InnerDictionary.ContainsKey(key);
        }
    }
}