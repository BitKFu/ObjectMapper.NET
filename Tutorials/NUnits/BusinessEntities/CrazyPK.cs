using System;
using System.Collections.Generic;
using AdFactum.Data;
using AdFactum.Data.Util;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class uses the date time for primary key (only for testing purpose :)
    /// </summary>
    public class CrazyPK : GenericValueObject<DateTime>, ICreateObject
    {
        private CrazyChildPK crazyChild;
        private List<CrazyChildPK> crazyList = new List<CrazyChildPK>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CrazyPK"/> class.
        /// </summary>
        public CrazyPK()
        {
            Id = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the crazy child.
        /// </summary>
        /// <value>The crazy child.</value>
        [PropertyName("Child")]
        public CrazyChildPK CrazyChild
        {
            get { return crazyChild; }
            set { crazyChild = value; }
        }

        /// <summary>
        /// Gets or sets the crazy list.
        /// </summary>
        /// <value>The crazy list.</value>
        [PropertyName("List")]
        public List<CrazyChildPK> CrazyList
        {
            get { return crazyList; }
            set { crazyList = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new CrazyPK();
        }
    }
}
