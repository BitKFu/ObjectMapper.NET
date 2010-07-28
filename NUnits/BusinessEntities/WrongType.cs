using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class creates the wrong object type and causes the WrongTypeException
    /// </summary>
    public class WrongType : BaseVO, ICreateObject
    {
        private int number;

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        [PropertyName("IntNumber")]
        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new NullValue();
        }
    }
}
