using System;
using System.Diagnostics;

namespace AdFactum.Data
{
	/// <summary>
	/// Base class for a value object 
	/// </summary>
	[Serializable]
	public class ValueObject : IValueObject
	{
		/// <summary>
		/// Used for unique identifying a value object.
		/// </summary>
		private object id = Guid.NewGuid();

		/// <summary>
		/// Indenticates if the object is new or loaded from database
		/// </summary>
		private bool isNew = true;

        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PrimaryKey]
        public virtual Guid Id
	    {
            get { return (Guid) id;  }
            set { id = value; }
	    }

		/// <summary>
		/// Primary Key
		/// </summary>
		[Ignore]
        object IValueObject.Id
		{
            [DebuggerStepThrough]
            get { return id; }

            [DebuggerStepThrough]
            set { id = value; }
		}

	    /// <summary>
	    /// Gets the internal id.
	    /// </summary>
	    /// <value>The internal id.</value>
        [Ignore]
        public Guid InternalId
	    {
            get { return Id; }
	    }

	    /// <summary>
		/// Gets or sets a value indicating whether this instance is new.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		[Ignore]
        public bool IsNew
		{
            [DebuggerStepThrough]
            get { return isNew; }

            [DebuggerStepThrough]
            set { isNew = value; }
		}

	}
}