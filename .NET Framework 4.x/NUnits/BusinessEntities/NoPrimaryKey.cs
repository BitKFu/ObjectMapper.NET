using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class overwrites the Id property, but does not tag it with the [PrimaryKey] attribute.
    /// </summary>
    public class NoPrimaryKey : ValueObject
    {
        private Guid copyOfId;
        private Buying buying;

        /// <summary>
        /// Overwrite the Id for what ever reason.
        /// </summary>
        /// <value>The unique value object id.</value>
        public new Guid Id
        {
            get { return base.Id; }
            set { base.Id = copyOfId = value; }
        }

        /// <summary>
        /// Gets or sets the copy of id.
        /// </summary>
        /// <value>The copy of id.</value>
        public Guid CopyOfId
        {
            get { return copyOfId; }
            set { copyOfId = value; }
        }

        /// <summary>
        /// Gets or sets the buying.
        /// </summary>
        /// <value>The buying.</value>
        public Buying Buying
        {
            get { return buying; }
            set { buying = value; }
        }
    }
}
