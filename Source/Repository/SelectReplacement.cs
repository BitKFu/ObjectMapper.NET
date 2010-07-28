using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdFactum.Data.Repository
{
    [Table("OMRE_ORM_REPLACEMENT")]
    [Serializable]
    public class SelectReplacement : MarkedValueObject
    {
        /// <summary> SqlId </summary>
        [PropertyName("ORM_SQLID"), Required, PropertyLength(64)]
        public string SqlId { get; set; }

        /// <summary> Override SQL </summary>
        [PropertyName("OVRD_SQL"), PropertyLength(int.MaxValue)]
        public string OverrideSql { get; set; }

        /// <summary> Override Hints </summary>
        [PropertyName("OVRD_HINTS"), PropertyLength(256)]
        public string OverrideHint { get; set; }
    }
}
