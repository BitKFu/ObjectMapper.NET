using System;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class DefaultPrimaryKeyTest : ObjectMapperTest
    {
        /// <summary>
        /// Tests the selection.
        /// </summary>
        [Test]
        public void TestSelection ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                mapper.FlatSelect(typeof (DefaultPrimaryType));

                var defPrimaries = mapper.Query<DefaultPrimaryType>();
                foreach (var item in from primary in defPrimaries select primary)
                    Console.WriteLine(item.ToString());
            }
        }
    }
}
