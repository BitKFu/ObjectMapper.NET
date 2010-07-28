using System;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Internal;
using System.Text;

namespace AdFactum.Data.Fields
{
    ///<summary>
    /// Class that defines a link in a list (e.x. a link in a ArrayList or Hashtable)
    ///</summary>
    [Serializable]
    public class OneToManyLink : SpecializedLink
    {
        public Field JoinField { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        /// <param name="pProperty">Defines the property id</param>
		public OneToManyLink(FieldDescription fdesc, object pProperty)
            :base(fdesc, pProperty)
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        public OneToManyLink(FieldDescription fdesc, Field joinField)
            : base(fdesc)
        {
            JoinField = joinField;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        public OneToManyLink(FieldDescription fdesc)
            :base(fdesc)
		{
		}

        /// <summary>
        /// Creates a clone
        /// </summary>
        /// <param name="copy"></param>
        public OneToManyLink(OneToManyLink copy)
            :base(copy)
        {
            JoinField = new Field(copy.JoinField);
        }

        /// <summary>
        /// Update the Join Field
        /// </summary>
        /// <param name="value"></param>
        public void UpdateParentReferenceId(object value)
        {
            if (JoinField != null)
                JoinField.Value = value;
        }

        /// <summary>
        /// Copys a link by creating a new object with the copy constructor
        /// </summary>
        public override Object Clone()
        {
            return new OneToManyLink(this);
        }
    }
}
