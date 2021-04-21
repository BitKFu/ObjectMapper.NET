using System;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Util;
using AdFactum.Data.Linq;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This test has been created to check the caching after flat and deep loading of objects
    /// </summary>
    [TestFixture]
    public class FlatSelectDeepSelectTest : ObjectMapperTest
    {
 
        /// <summary>
        /// Tests the flat and deep loading.
        /// </summary>
        [Test]
        public void TestFlatAndDeepLoading ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create some objects
                 */
                var entry = new PhoneBookEntry(
                    new Friend("Toby", "M.", new DateTime(1972, 11, 10)), null, "0049 232112");

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(entry);
                OBM.Commit(mapper, nested);

                /*
                 * Try to get the nested person
                 */
                Assert.IsNotNull(mapper.GetNestedObject(typeof(PhoneBookEntry), "Person", entry.Id),
                                 "Could not load nested object");

                /*
                 * Try to load them flat
                 */
                var flatLoaded = (PhoneBookEntry)mapper.FlatLoad(typeof(PhoneBookEntry), entry.Id);
                Assert.IsNotNull(flatLoaded, "Object could not be loaded.");
                Assert.IsNull(flatLoaded.Person, "Person must be null, because we did a flat load.");

                var flatLoaded2 = (from row in mapper.Query<PhoneBookEntry>() where row.Id == entry.Id select row).Single();
                Assert.IsNotNull(flatLoaded2, "Object could not be loaded.");
                Assert.IsNull(flatLoaded2.Person, "Person must be null, because we did a flat load.");

                /*
                 * Try to load them full
                 */
                var fullLoaded = (PhoneBookEntry) mapper.Load(typeof (PhoneBookEntry), entry.Id);
                Assert.IsNotNull(fullLoaded, "Object could not be loaded.");
                Assert.IsNotNull(fullLoaded.Person, "Person must not be null, because we did a full load.");

                var fullLoaded2 = (from row in mapper.Query<PhoneBookEntry>() where row.Id == entry.Id select row).Level(HierarchyLevel.AllDependencies).Single();

                Assert.IsNotNull(fullLoaded2, "Object could not be loaded.");
                Assert.IsNotNull(fullLoaded2.Person, "Person must not be null, because we did a full load.");

            }
        }
    }
}
