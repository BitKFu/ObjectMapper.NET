using System.Collections;

namespace BusinessEntities.Collections
{
	/// <MarketplaceItem>
	/// MarketplaceItem description for MarketplaceItemCollection.
	/// </MarketplaceItem>
	public class MarketplaceItemCollection : CollectionBase
	{
		/// <MarketplaceItem>
		/// Initializes a new instance of the <see cref="MarketplaceItemCollection"/> class.
		/// </MarketplaceItem>
		public MarketplaceItemCollection()
		{
		}

		/// <MarketplaceItem>
		/// Initializes a new instance of the <see cref="MarketplaceItemCollection"/> class.
		/// </MarketplaceItem>
		/// <param name="copyCollection">The copy collection.</param>
		public MarketplaceItemCollection(IList copyCollection)
			: base()
		{
			InnerList.AddRange(copyCollection);
		}


		/// <MarketplaceItem>gets or sets the element at the specified index. </MarketplaceItem>
		public MarketplaceItem this[int index]
		{
			get { return (MarketplaceItem) List[index]; }
			set { List[index] = value; }
		}

		/// <MarketplaceItem>adds an item to the collection.</MarketplaceItem>
		/// <param name="value">The Contact to add to the collection. </param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(MarketplaceItem value)
		{
			return List.Add(value);
		}

		/// <MarketplaceItem>determines whether the collection contains a specific value. </MarketplaceItem>
		/// <param name="value">The Contact to locate in the collection. </param>
		/// <returns>true if the Contact is found in the collection; otherwise, false.</returns>
		public bool Contains(MarketplaceItem value)
		{
			// If value is not of type Contact, this will return false.
			return List.Contains(value);
		}

		/// <MarketplaceItem>determines the index of a specific item in the collection.</MarketplaceItem>
		/// <param name="value">The Contact to locate in the collection. </param>
		/// <returns>The index of value if found in the list; otherwise, -1.</returns>
		public int IndexOf(MarketplaceItem value)
		{
			return List.IndexOf(value);
		}

		/// <MarketplaceItem>inserts an item to the collection at the specified position.</MarketplaceItem>
		/// <param name="index">The zero-based index at which value should be inserted. </param>
		/// <param name="value">The Contact to insert into the collection. </param>
		public void Insert(int index, MarketplaceItem value)
		{
			List.Insert(index, value);
		}

		/// <MarketplaceItem>removes the first occurrence of a specific Contact from the collection.</MarketplaceItem>
		/// <param name="value">The Contact to remove from the collection. </param>
		public void Remove(MarketplaceItem value)
		{
			List.Remove(value);
		}
	}
}