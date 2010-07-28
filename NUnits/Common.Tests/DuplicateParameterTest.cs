using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This testclass test the duplicate parameter values
    /// </summary>
    [TestFixture]
    public class DuplicateParameterTest : ObjectMapperTest
    {
        /// <summary>
        /// This method tries to store an object with a duplicate value and type
        /// in order to test parameter re-use.
        /// </summary>
        [Test]
        public void StoreContactWithDuplicatedValues ()
        {
            var c = new Contact {FirstName = "Stephan", LastName = "Stephan"};

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(c);
                OBM.Commit(mapper, nested);
            }
        }
    }
}
