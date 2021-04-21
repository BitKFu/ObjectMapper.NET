using AdFactum.Data;
using BusinessEntities.Collections;

namespace BusinessEntities
{
	/// <summary>
	/// This class describes a user that can sell items and place bids.
	/// </summary>
	[Table ("USERS")]
	public class User : ValueObject
	{
		private string name;
		private string logon;
		private string md5PasswordKey;

		private MarketplaceItemCollection sellings;
		private BidCollection bids;

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
		/// Gets or sets the logon.
		/// </summary>
		/// <value>The logon.</value>
		[PropertyLength(32)]
		[Required]
		[Unique]
		public string Logon
		{
			get { return logon; }
			set { logon = value; }
		}

		/// <summary>
		/// Gets or sets the MD5 password key.
		/// </summary>
		/// <value>The MD5 password key.</value>
		[PropertyLength(32)]
		public string Md5PasswordKey
		{
			get { return md5PasswordKey; }
			set { md5PasswordKey = value; }
		}

		/// <summary>
		/// Gets or sets the sellings.
		/// </summary>
		/// <value>The sellings.</value>
		[BindPropertyTo(typeof (MarketplaceItem))]
		public MarketplaceItemCollection Sellings
		{
			get { return sellings; }
			set { sellings = value; }
		}

		/// <summary>
		/// Gets or sets the bids.
		/// </summary>
		/// <value>The bids.</value>
		[BindPropertyTo(typeof (Bid))]
		public BidCollection Bids
		{
			get { return bids; }
			set { bids = value; }
		}


	}
}