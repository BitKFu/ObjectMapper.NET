using System;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Test for database functions
    /// </summary>
    public class DatabaseFunction : BaseVO, ICreateObject
    {
        /// <summary>
        /// Gets or sets the last read.
        /// </summary>
        /// <value>The last read.</value>
        [SelectFunction("GETDATE()")]
        public DateTime LastRead { get; set; }

        /// <summary>
        /// Gets or sets the creation.
        /// </summary>
        /// <value>The creation.</value>
        [InsertFunction("GETDATE()")]
        public DateTime Creation { get; set; }

        /// <summary>
        /// Gets or sets the last updated.
        /// </summary>
        /// <value>The last updated.</value>
        [UpdateFunction("GETDATE()")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new DatabaseFunction();
        }
    }
}
