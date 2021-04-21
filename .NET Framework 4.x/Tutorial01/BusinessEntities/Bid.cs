using System;
using AdFactum.Data;

namespace BusinessEntities
{
	/// <summary>
	/// This class contains all attributes used to identify a bid
	/// </summary>
	public class Bid : ValueObject
	{
		private MarketplaceItem bidOn;
		private DateTime timeStamp;
		private double maxBid;

		/// <summary>
		/// Gets or sets the bid on.
		/// </summary>
		/// <value>The bid on.</value>
		public MarketplaceItem BidOn
		{
			get { return bidOn; }
			set { bidOn = value; }
		}

		/// <summary>
		/// Gets or sets the time stamp.
		/// </summary>
		/// <value>The time stamp.</value>
		[PropertyName("Stamp")]
		public DateTime TimeStamp
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}

		/// <summary>
		/// Gets or sets the max bid.
		/// </summary>
		/// <value>The max bid.</value>
		public double MaxBid
		{
			get { return maxBid; }
			set { maxBid = value; }
		}
	}
}