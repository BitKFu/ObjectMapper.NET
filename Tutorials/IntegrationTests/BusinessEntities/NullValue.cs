using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using AdFactum.Data;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Class to test nullables and null values for database
    /// </summary>
    public class NullValue : BaseVO, ICreateObject
    {
        private DateTime creationDate = DateTime.Now;

        private DateTime nullTime = DateTime.MinValue;
        private Guid nullGuid = Guid.Empty;
        private string nullString = null;
        
        private int? nullInt = null;
        private DateTime? nullTime2 = null;
        private Guid? nullGuid2 = null;
        private bool? nullBoolean = null;

        /// <summary>
        /// Gets or sets the null time.
        /// </summary>
        /// <value>The null time.</value>
        public DateTime NullTime
        {
            get { return nullTime; }
            set { nullTime = value; }
        }

        /// <summary>
        /// Gets or sets the null GUID.
        /// </summary>
        /// <value>The null GUID.</value>
        public Guid NullGuid
        {
            get { return nullGuid; }
            set { nullGuid = value; }
        }

        /// <summary>
        /// Gets or sets the null string.
        /// </summary>
        /// <value>The null string.</value>
        public string NullString
        {
            get { return nullString; }
            set { nullString = value; }
        }

        /// <summary>
        /// Gets or sets the null int.
        /// </summary>
        /// <value>The null int.</value>
        public int? NullInt
        {
            get { return nullInt; }
            set { nullInt = value; }
        }

        /// <summary>
        /// Gets or sets the null time2.
        /// </summary>
        /// <value>The null time2.</value>
        public DateTime? NullTime2
        {
            get { return nullTime2; }
            set { nullTime2 = value; }
        }

        /// <summary>
        /// Gets or sets the null guid2.
        /// </summary>
        /// <value>The null guid2.</value>
        public Guid? NullGuid2
        {
            get { return nullGuid2; }
            set { nullGuid2 = value; }
        }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>The creation date.</value>
        public DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

        /// <summary>
        /// Gets or sets the null boolean.
        /// </summary>
        /// <value>The null boolean.</value>
        public bool? NullBoolean
        {
            get { return nullBoolean; }
            set { nullBoolean = value; }
        }

        /// <summary>
        /// Asserts the check.
        /// </summary>
        /// <param name="mustEqual">The must equal.</param>
        public void AssertCheck (NullValue mustEqual)
        {
            ClassicAssert.AreEqual(NullString, mustEqual.NullString, "Null string differs");
            ClassicAssert.AreEqual(NullGuid, mustEqual.NullGuid, "Null Guid differs");
            ClassicAssert.AreEqual(NullTime.ToString(), mustEqual.NullTime.ToString(), "Null time differs");
            ClassicAssert.AreEqual(nullInt, mustEqual.NullInt, "Null Int differs");
            ClassicAssert.AreEqual(NullGuid2, mustEqual.NullGuid2, "Null Guid2 differs");
            ClassicAssert.AreEqual(NullTime2.ToString(), mustEqual.NullTime2.ToString(), "Null time2 differs");
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
