using System;
using System.IO;
using AdFactum.Data;

namespace BusinessEntities
{
	/// <summary>
	/// Marketplace Item
	/// </summary>
	[Table("Item")]
	public class MarketplaceItem : ValueObject, ICreateObject
	{
		private string name;
		private double minimumBid;
		private string description;

		private bool isQuickBuyEnabled;
		private double quickBuyPrice;

		private User soldTo;
		private TimeSpan offerDuration;
		private DateTime startDate;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[PropertyLength(50)]
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets the minimum bid.
		/// </summary>
		/// <value>The minimum bid.</value>
		[PropertyName("MINIMUM_BID")]
		public double MinimumBid
		{
			get { return minimumBid; }
			set { minimumBid = value; }
		}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[PropertyLength(int.MaxValue)]
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is quick buy enabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is quick buy enabled; otherwise, <c>false</c>.
		/// </value>
		[PropertyName("QUICK_BUY")]
		public bool IsQuickBuyEnabled
		{
			get { return isQuickBuyEnabled; }
			set { isQuickBuyEnabled = value; }
		}

		/// <summary>
		/// Gets or sets the quick buy price.
		/// </summary>
		/// <value>The quick buy price.</value>
		[PropertyName("QUICK_BUY_PRICE")]
		public double QuickBuyPrice
		{
			get { return quickBuyPrice; }
			set { quickBuyPrice = value; }
		}

		/// <summary>
		/// Gets or sets the sold to.
		/// </summary>
		/// <value>The sold to.</value>
		[PropertyName("SOLD_TO")]
		public User SoldTo
		{
			get { return soldTo; }
			set { soldTo = value; }
		}

		/// <summary>
		/// Gets or sets the offer duration.
		/// </summary>
		/// <value>The offer duration.</value>
		[Required]
		[PropertyName("DURATION")]
		public TimeSpan OfferDuration
		{
			get { return offerDuration; }
			set { offerDuration = value; }
		}

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		/// <value>The start date.</value>
		[Required]
		[PropertyName("START_DATE")]
		public DateTime StartDate
		{
			get { return startDate; }
			set { startDate = value; }
		}

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
	    public IValueObject CreateNewObject()
	    {
	        return new MarketplaceItem();
	    }
	}
}