using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Test for the database functions
    /// </summary>
    [TestFixture]
    public class DbFunctionTest : ObjectMapperTest
    {

        /// <summary>
        /// Tests the functions.
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForOracle")]
        public void TestSqlServerFunctions()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * First create
                 */
                var function = new DatabaseFunction();
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(function);
                OBM.Commit(mapper, nested);

                /*
                 * Read and every function field must be filled
                 */
                var read = mapper.FlatLoad(typeof(DatabaseFunction), function.Id) as DatabaseFunction;

                // Wait a second to force an update
                System.Threading.Thread.Sleep(1000);

                /*
                 * Second update
                 */
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(read);
                OBM.Commit(mapper, nested);

                /*
                 * Read and every function field must be filled
                 */
                read = (DatabaseFunction) mapper.FlatLoad(typeof(DatabaseFunction), function.Id);

                Assert.IsTrue(read.LastRead > DateTime.MinValue, "Read Function did not work.");
                Assert.IsTrue(read.LastUpdated > DateTime.MinValue, "Update Function did not work.");
                Assert.IsTrue(read.Creation > DateTime.MinValue, "Create Function did not work.");
            }
        }

    }
}
