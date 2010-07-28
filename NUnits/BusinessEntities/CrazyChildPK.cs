using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AdFactum.Data;
using AdFactum.Data.Util;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class uses a own value object implementation with a different primary key type and name
    /// </summary>
    public class CrazyChildPK : IValueObject, ICreateObject
    {
        /// <summary>
        /// InternalId Reference
        /// </summary>
        private Guid internalId = Guid.NewGuid();
        
        /// <summary>
        /// Used for unique identifying a value object.
        /// </summary>
        private object childName = Guid.NewGuid();

        /// <summary>
        /// Indenticates if the object is new or loaded from database
        /// </summary>
        private bool isNew = true;

        /// <summary>
        /// Gets or sets the unique value object childName.
        /// </summary>
        /// <value>The unique value object childName.</value>
        [PrimaryKey]
        [PropertyName("Name")]
        public string ChildName
        {
            get { return (string)childName; }
            set { childName = value; }
        }

        /// <summary>
        /// Primary Key
        /// </summary>
        [Ignore]
        object IValueObject.Id
        {
            [DebuggerStepThrough]
            get { return childName; }

            [DebuggerStepThrough]
            set { childName = value; }
        }

        /// <summary>
        /// Gets the internal childName.
        /// </summary>
        /// <value>The internal childName.</value>
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
            [DebuggerStepThrough]
            get { return isNew; }

            [DebuggerStepThrough]
            set { isNew = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new CrazyChildPK();
        }
    }
}
