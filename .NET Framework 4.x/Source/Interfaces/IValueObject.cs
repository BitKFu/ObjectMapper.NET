using System;
using System.Diagnostics;

namespace AdFactum.Data
{
	/// <summary>
	/// Base interface that must be inhertied from all business entities
	/// </summary>
	public interface IValueObject
	{
		/// <summary>
		/// Gets or sets the unique value object id.
		/// </summary>
		/// <value>The unique value object id.</value>
		object Id
		{
			[DebuggerStepThrough]
			get; 
			set;
		}

        /// <summary>
        /// Gets the internal id.
        /// </summary>
        /// <value>The internal id.</value>
        [Ignore]
        Guid InternalId
        { 
            get;
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is new.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		[Ignore]
		bool IsNew
		{
			[DebuggerStepThrough]
			get;
			set;
		}

	}
}