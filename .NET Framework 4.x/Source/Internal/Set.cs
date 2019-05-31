using System;
using System.Collections;
using AdFactum.Data.Queries;

namespace AdFactum.Data.Internal
{
	/// <summary>
	/// This class defines a SET
	/// </summary>
	[Serializable]
    public class Set : CollectionBase
	{
		/// <summary>
		/// Private Class that holds the set member and a additional object with informations
		/// </summary>
		public class Tupel
		{
			/// <summary>
			/// Key in order to access the set member
			/// </summary>
			private string key;

			/// <summary>
			/// Condition which belongs to the key
			/// </summary>
			private ICondition member;

			/// <summary>
			/// Condition if a schema replacement takes place
			/// </summary>
			private bool schemaReplacement = true;

			/// <summary>
			/// Overwrite field
			/// </summary>
			private string overwrite = null;

			/// <summary>
			/// Key in order to access the set member
			/// </summary>
			public string Key
			{
				get { return key; }
				set { key = value; }
			}

			/// <summary>
			/// Condition which belongs to the key
			/// </summary>
			public ICondition Member
			{
				get { return member; }
			}

			/// <summary>
			/// Condition if a schema replacement takes place
			/// </summary>
			public bool SchemaReplacement
			{
				get { return schemaReplacement; }
			}

			/// <summary>
			/// Constructor for a condition tupel
			/// </summary>
            /// <param name="keyParameter">Key</param>
            /// <param name="memberParameter">Condition </param>
            public Tupel(string keyParameter, ICondition memberParameter)
			{
                key = keyParameter;
                member = memberParameter;

                if (member != null)
                {
                    schemaReplacement = false;
                    string overwriteSql = member.ConditionString;
                    overwrite = overwriteSql.IndexOf(' ') >= 0 ? string.Concat("(", overwriteSql, ")") : overwriteSql;
                }
			}

			/// <summary>
			/// Returns the tupel key
			/// </summary>
			/// <returns></returns>
			public string TupelString()
			{
				return string.Concat(
					(SchemaReplacement ? AdFactum.Data.Queries.Condition.SCHEMA_REPLACE : ""),
					overwrite, (overwrite != null ? " " : ""),
					AdFactum.Data.Queries.Condition.QUOTE_OPEN, key, 
					AdFactum.Data.Queries.Condition.QUOTE_CLOSE);
			}

            /// <summary>
            /// Gets the table.
            /// </summary>
            /// <value>The table.</value>
		    public string Table
		    {
		        get
		        {
                    return string.Concat(
                        (SchemaReplacement ? AdFactum.Data.Queries.Condition.SCHEMA_REPLACE : ""),
                        AdFactum.Data.Queries.Condition.QUOTE_OPEN, key,
                        AdFactum.Data.Queries.Condition.QUOTE_CLOSE);
		            
		        }
		    }

			/// <summary>
			/// Gets or sets the overwrite.
			/// </summary>
			/// <value>The overwrite.</value>
			public string Overwrite
			{
				get { return overwrite; }
				set { overwrite = value; }
			}

            /// <summary>
            /// Returns the values of the nested condition
            /// </summary>
            public IList Values
            {
                get
                {
                    Condition condition = member as Condition;
                    return condition != null ? condition.Values : new ArrayList();
                }
            }
		}

		/// <summary>
		/// Adds a key to the set
		/// </summary>
		/// <param name="keyToAdd">object which has to add to set</param>
		/// <param name="member">Stores member information for a key</param>
		public void Add(string keyToAdd, ICondition member)
		{
			/*
			 * If there is no key, wen can definitly add the key
			 */
			if (!Contains(keyToAdd))
				InnerList.Add(new Tupel(keyToAdd, member));
			else if ((member != null) && (Condition(keyToAdd) == null))
			{
				/*
				 * If there's a key, a key with a member is preferred	
				 */
				Remove(keyToAdd);
				InnerList.Add(new Tupel(keyToAdd, member));
			}
		}

		/// <summary>
		/// Adds a key to the set 
		/// </summary>
		/// <param name="keyToAdd"></param>
		public void Add(string keyToAdd)
		{
			Add(keyToAdd, null);
		}

		/// <summary>
		/// This method contains true, if the object is already contained within the set.
		/// </summary>
		/// <param name="keyToCheck">Key to check</param>
		/// <returns>True, if the object already exists</returns>
		public bool Contains(string keyToCheck)
		{
			IEnumerator tupelEnumerator = InnerList.GetEnumerator();
			while (tupelEnumerator.MoveNext())
			{
				Tupel t = tupelEnumerator.Current as Tupel;
				if (t != null && t.Key.Equals(keyToCheck))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the object from the set
		/// </summary>
		/// <param name="keyToRemove">Object to remove</param>
		public void Remove(string keyToRemove)
		{
			IEnumerator tupelEnumerator = InnerList.GetEnumerator();
			while (tupelEnumerator.MoveNext())
			{
				Tupel t = tupelEnumerator.Current as Tupel;
				if (t != null && t.Key.Equals(keyToRemove))
				{
					InnerList.Remove(t);
					return;
				}
			}
		}

		/// <summary>
		/// Returns a condition for the key.
		/// </summary>
		/// <param name="keyForCondition">Key for which the condition shall be received.</param>
		/// <returns>If a condition exists, the method returns the condition</returns>
		public ICondition Condition(string keyForCondition)
		{
			IEnumerator removeEnumerator = InnerList.GetEnumerator();
			while (removeEnumerator.MoveNext())
			{
				string value = removeEnumerator.Current.ToString();
				if (value.Equals(keyForCondition))
					return ((Tupel) removeEnumerator.Current).Member;
			}

			return null;
		}

		/// <summary>
		/// Merges a set with the current set
		/// </summary>
		/// <param name="setToMerge">Set to merge</param>
		public void Merge(Set setToMerge)
		{
			IEnumerator enumerator = setToMerge.GetEnumerator();
			while (enumerator.MoveNext())
			{
			    Tupel copy = enumerator.Current as Tupel;
			    if (copy != null) 
                    Add(copy.Key, copy.Member);
			}
		}

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified i.
        /// </summary>
        /// <value></value>
	    public string this[int i]
	    {
	        get
	        {
	            Tupel tupel = InnerList[i] as Tupel;
                if (tupel != null)
                    return tupel.TupelString();

	            throw new IndexOutOfRangeException("The indexer within the SET is out of range.");
	        }
	    }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public string GetTable(int i)
        {
            Tupel tupel = InnerList[i] as Tupel;
            if (tupel != null)
                return tupel.Key;

            throw new IndexOutOfRangeException("The indexer within the SET is out of range.");
        }

        /// <summary>
        /// Gets the tupel.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public Tupel GetTupel(string table)
        {
            foreach (Tupel tupel in InnerList)
                if (tupel.Key == table)
                    return tupel;

            return null;
        }
	}
}