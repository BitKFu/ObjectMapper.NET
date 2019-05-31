using System;
using System.Text;

namespace AdFactum.Data
{
    /// <summary>
    /// Auto increment value object
    /// </summary>
    public class AutoIncValueObject : IValueObject
    {
        private Guid internalId = Guid.NewGuid();            
        private bool isNew = true;
        private object id = null;

        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PrimaryKey]
        public virtual int? Id
        {
            get { return (int?) id;  }
            set { id = value;  }
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
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [Ignore]
        object IValueObject.Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
