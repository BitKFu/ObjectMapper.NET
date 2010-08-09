using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Test the default primary key type for interfaces
    /// </summary>
    [Table("DefaultPk")]
    public class DefaultPrimaryType : ValueObject
    {
        private List<IIrgendwas> ignoreMe = new List<IIrgendwas>();
        private List<IIrgendwas> saveMe = new List<IIrgendwas>();

        /// <summary>
        /// Gets or sets the ignore me.
        /// </summary>
        /// <value>The ignore me.</value>
        [Ignore]
        public List<IIrgendwas> IgnoreMe
        {
            get { return ignoreMe; }
            set { ignoreMe = value; }
        }

        /// <summary>
        /// Gets or sets the save me.
        /// </summary>
        /// <value>The save me.</value>
        [GeneralLink()]
        public List<IIrgendwas> SaveMe
        {
            get { return saveMe; }
            set { saveMe = value; }
        }
    }
}
