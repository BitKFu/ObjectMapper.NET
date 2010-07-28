using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class NullValueTest : ObjectMapperTest
    {
        /// <summary>
        /// Tests the null values.
        /// </summary>
        [Test]
        public void TestNullValues()
        {
            /*
             * Try to store the null values
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                NullValue nullValue = new NullValue();
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(nullValue);
                OBM.Commit(mapper, nested);

                NullValue loaded = mapper.Load(typeof(NullValue), nullValue.Id) as NullValue;
                nullValue.AssertCheck(loaded);

                DateTime now = DateTime.Now;
                DateTime assertTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

                /*
                 * Store values 
                 */
                nullValue.NullGuid = Guid.NewGuid();
                nullValue.NullString = "Hello";
                nullValue.NullTime = assertTime;
                nullValue.NullInt = 5;
                nullValue.NullGuid2 = Guid.NewGuid();
                nullValue.NullTime2 = assertTime;

                /*
                 * Try to store some values
                 */
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(nullValue);
                OBM.Commit(mapper, nested);

                loaded = mapper.Load(typeof(NullValue), nullValue.Id) as NullValue;
                nullValue.AssertCheck(loaded);

                /*
                 * Reset values 
                 */
                nullValue.NullGuid = Guid.Empty;
                nullValue.NullString = null;
                nullValue.NullTime = DateTime.MinValue;
                nullValue.NullInt = null;
                nullValue.NullGuid2 = null;
                nullValue.NullTime2 = null;

                /*
                 * Try to store the NULL values again
                 */
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(nullValue);
                OBM.Commit(mapper, nested);

                loaded = mapper.Load(typeof(NullValue), nullValue.Id) as NullValue;
                nullValue.AssertCheck(loaded);
            }
        }
    }
}
