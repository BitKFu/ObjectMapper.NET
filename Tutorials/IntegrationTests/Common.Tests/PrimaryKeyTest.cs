using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class PrimaryKeyTest : ObjectMapperTest
    {
        /// <summary>
        /// Tests the integer primary keys.
        /// </summary>
        /// <returns></returns>
        [Test]
        public void TestIntegerPrimaryKeys()
        {
            Animal animal = new Animal();
            animal.Legs = 4;
            animal.Name = "Dog";

            /*
             * Try to store the values
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(animal);

                OBM.Flush(mapper);

                animal.Legs = 6;
                mapper.Save(animal);
                OBM.Commit(mapper, nested);
            }
        }

        /// <summary>
        /// Tests the crazy primary keys.
        /// </summary>
        [Test]
        public void TestCrazyPrimaryKeys()
        {
            /*
             * Create crazy pk objects
             */
            CrazyChildPK childPk = new CrazyChildPK();
            childPk.ChildName = "Crazy Bob";

            CrazyChildPK childA = new CrazyChildPK();
            childA.ChildName = "Yep that's me";

            CrazyChildPK childB = new CrazyChildPK();
            childB.ChildName = "Why not me too.";

            CrazyPK crazyPK = new CrazyPK();
            crazyPK.CrazyChild = childPk;

            crazyPK.CrazyList.Add(childA);
            crazyPK.CrazyList.Add(childB);

            /*
             * Store crazy PK objects
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(crazyPK);
                OBM.Commit(mapper, nested);

                /*
                 Load the crazy objects
                */
                CrazyPK loaded = (CrazyPK)mapper.Load(typeof(CrazyPK), crazyPK.Id);

                Assert.IsNotNull(loaded, "Crazy object could not be loaded.");
                Assert.IsNotNull(loaded.CrazyChild, "Crazy child could not be loaded.");
                Assert.IsNotNull(loaded.CrazyList, "Crazy list could not be loaded.");
                Assert.AreEqual(crazyPK.CrazyList.Count, loaded.CrazyList.Count, "Could not load all list objects.");
            }
        }
    }
}
 