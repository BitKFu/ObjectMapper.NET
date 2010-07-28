using System;
using System.Collections;
using System.Collections.Generic;

namespace AdFactum.Data.Internal
{
	/// <summary>
	/// The backlist provides a way to store persistent objects in order to make constraint safe inserts.
	/// </summary>
	[Serializable]
    public class ConstraintSaveList 
	{
		/// <summary>
		/// List which holds the list entries
		/// </summary>
        private List<HashEntry> innerList = new List<HashEntry>();

		/// <summary>
		/// Hashtable that holds the guids in order to have a quick access
		/// to the list
		/// </summary>
        private Dictionary<string, int> guidHash = new Dictionary<string, int>();

        /// <summary>
        /// List which holds the list entries
        /// </summary>
        /// <value>The inner list.</value>
	    public List<HashEntry> InnerList
	    {
	        get { return innerList; }
	    }

	    /// <summary>
		/// Adds am object to the set
		/// </summary>
		/// <param name="objectToAdd">object which has to add to set</param>
		public int Add(HashEntry objectToAdd)
		{
            InnerList.Add(objectToAdd);
		    int position = InnerList.Count - 1;

            if (objectToAdd.Id != null)
            {
                string calcKey = CalculateKey(objectToAdd);
                if (guidHash.ContainsKey(calcKey))
                    guidHash.Remove(calcKey);
    
                guidHash.Add(calcKey, position);
            }

		    return position;
		}


        /// <summary>
        /// Checks if a special object is within the list
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
		public bool Contains(Type type, object key)
		{
			return (key != null) && guidHash.ContainsKey(CalculateKey(type,key));
		}

        /// <summary>
        /// Determines whether [contains] [the specified vo].
        /// </summary>
        /// <param name="vo">The vo.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified vo]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(object vo)
        {
            IValueObject ivo = vo as IValueObject;
            if (ivo == null)
                return Contains(vo.GetType(), vo.GetHashCode());

            if (ivo.Id != null)
                return Contains(vo.GetType(), ivo.Id);
            else
                return Contains(vo.GetType(), ivo.InternalId);
        }

        /// <summary>
        /// Removes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        public void Remove(Type type, object key)
		{
            if (key == null)
                return;

		    string hashKey = CalculateKey(type, key);
            int position = guidHash[hashKey];
			InnerList[position] = null;
            guidHash.Remove(hashKey);
		}

        /// <summary>
        /// Checks if a special object is within the list
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public int IndexOf(Type type, object key)
		{
            if (key != null)
            {
                string hashKey = CalculateKey(type, key);
                if (guidHash.ContainsKey(hashKey))
                    return guidHash[hashKey];
            }

            return -1;
		}

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="hashKey">The hash key.</param>
        /// <returns></returns>
        public int IndexOf(string hashKey)
        {
            if (guidHash.ContainsKey(hashKey))
                return guidHash[hashKey];

            return -1;
        }

		/// <summary>
		/// Indexer
		/// </summary>
		public HashEntry this[int index]
		{
			get { return InnerList[index]; }
			set { InnerList[index] = value; }
		}

		/// <summary>
		/// Indexer
		/// </summary>
		public HashEntry this[string key]
		{
			get { return InnerList[guidHash[key]]; }
			set { InnerList[guidHash[key]] = value; }
		}

		/// <summary>
		/// Returns an enumerator
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}

		/// <summary>
		/// Removes an element with the index
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
            HashEntry entry = InnerList[index];
			InnerList[index] = null;
			guidHash.Remove(CalculateKey(entry));
		}

        /// <summary>
        /// Calculates the key.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <returns></returns>
        public static string CalculateKey (Type type, object primaryKey)
        {
            return string.Concat(type.FullName, ",", primaryKey.ToString());
        }
        
        /// <summary>
        /// Calculates the key.
        /// </summary>
        /// <param name="add">The add.</param>
        private static string CalculateKey(HashEntry add)
        {
            return CalculateKey(add.Type, add.Id);
        }

        /// <summary>
        /// Adds the specified vo.
        /// </summary>
        /// <param name="vo">The vo.</param>
	    public int Add(IValueObject vo)
	    {
            HashEntry entry = new HashEntry(null, vo);
            
            InnerList.Add(entry);
            int position = InnerList.Count - 1;
            guidHash.Add(CalculateKey(entry), position);

            return position;
        }

        /// <summary>
        /// Calculates the key.
        /// </summary>
        /// <param name="vo">The vo.</param>
	    public static string CalculateKey(object vo)
	    {
            IValueObject ivo = vo as IValueObject;
            if (ivo == null)
                return vo.GetHashCode().ToString();

            if (ivo.Id != null)
                return CalculateKey(vo.GetType(), ivo.Id);
            else
                return CalculateKey(vo.GetType(), ivo.InternalId);
	    }
	}
}