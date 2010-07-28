using System.Collections;

namespace BusinessEntities.Collections
{
	/// <Bid>
	/// Bid description for BidCollection.
	/// </Bid>
	public class BidCollection : CollectionBase
	{
		/// <Bid>
		/// Initializes a new instance of the <see cref="BidCollection"/> class.
		/// </Bid>
		public BidCollection()
		{
		}

		/// <Bid>
		/// Initializes a new instance of the <see cref="BidCollection"/> class.
		/// </Bid>
		/// <param name="copyCollection">The copy collection.</param>
		public BidCollection(IList copyCollection)
			: base()
		{
			InnerList.AddRange(copyCollection);
		}


		/// <Bid>gets or sets the element at the specified index. </Bid>
		public Bid this[int index]
		{
			get { return (Bid) List[index]; }
			set { List[index] = value; }
		}

		/// <Bid>adds an item to the collection.</Bid>
		/// <param name="value">The Contact to add to the collection. </param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(Bid value)
		{
			return List.Add(value);
		}

		/// <Bid>determines whether the collection contains a specific value. </Bid>
		/// <param name="value">The Contact to locate in the collection. </param>
		/// <returns>true if the Contact is found in the collection; otherwise, false.</returns>
		public bool Contains(Bid value)
		{
			// If value is not of type Contact, this will return false.
			return List.Contains(value);
		}

		/// <Bid>determines the index of a specific item in the collection.</Bid>
		/// <param name="value">The Contact to locate in the collection. </param>
		/// <returns>The index of value if found in the list; otherwise, -1.</returns>
		public int IndexOf(Bid value)
		{
			return List.IndexOf(value);
		}

		/// <Bid>inserts an item to the collection at the specified position.</Bid>
		/// <param name="index">The zero-based index at which value should be inserted. </param>
		/// <param name="value">The Contact to insert into the collection. </param>
		public void Insert(int index, Bid value)
		{
			List.Insert(index, value);
		}

		/// <Bid>removes the first occurrence of a specific Contact from the collection.</Bid>
		/// <param name="value">The Contact to remove from the collection. </param>
		public void Remove(Bid value)
		{
			List.Remove(value);
		}
	}
}