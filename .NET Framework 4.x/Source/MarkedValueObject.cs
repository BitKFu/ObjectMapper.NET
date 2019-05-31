using System;
using System.Diagnostics;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
	/// This base class is used for implementing a markable value object. 
	/// A markable value object is a value object that has a last update field.
	/// </summary>
	[Serializable]
    public class MarkedValueObject : ValueObject, IMarkedValueObject
	{
		/// <summary>
		/// Defines the last update time
		/// </summary>
		private DateTime lastUpdate = DateTime.MinValue;

		/// <summary>
		/// Returns the latest update function to identify bad update
		/// </summary>
		[PropertyName(DBConst.LastUpdateField)]
		public DateTime LastUpdate
		{
            [DebuggerStepThrough]
            get { return lastUpdate; }

            [DebuggerStepThrough]
            set { lastUpdate = value; }
		}
	}
}