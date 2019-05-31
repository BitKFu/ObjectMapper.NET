using System;

namespace AdFactum.Data
{
    /// <summary>
    /// Base class for a value object 
    /// </summary>
    [Serializable]
    public  class GenericValueObject<T> : IGenericValueObject<T>
    {
        /// <summary>
        /// Indenticates if the object is new or loaded from database
        /// </summary>
        private bool isNew = true;

        /// <summary>
        /// Primary Key
        /// </summary>
        private object id;

        /// <summary>
        /// Internal used primary key
        /// </summary>
        private Guid internalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericValueObject&lt;T&gt;"/> class.
        /// </summary>
        public GenericValueObject ()
        {
            internalId = Guid.NewGuid();
            if (typeof(T) == typeof(Guid))
                id = internalId;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [PrimaryKey]
        public virtual T Id
        {
            get { return (T)id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is new.
        /// </summary>
        /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
        [Ignore]
        public bool IsNew
        {
            get { return isNew; }
            set { isNew = value; }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [Ignore]
        object IValueObject.Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets the internal id.
        /// </summary>
        /// <value>The internal id.</value>
        [Ignore]
        public Guid InternalId
        {
            get { return internalId; }
        }
    }
}
