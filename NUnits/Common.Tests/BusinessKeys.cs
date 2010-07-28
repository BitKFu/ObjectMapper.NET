using System;
using System.Collections.Generic;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Tries to create some business driven foreign keys.
    /// 
    /// Remark: Technical foreign keys will be created automatically without user interaction.
    /// </summary>
    [TestFixture]
    public class BusinessKeys : ObjectMapperTest
    {
        /// <summary>
        /// Countries the region test.
        /// </summary>
        [Test]
        public void CountryRegionTest()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Insert Valid data
                 */
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Save(new Country("DE", "Deutschland", "de-DE"));
                mapper.Save(new Country("DE", "Germany", "en-GB"));

                mapper.Save(new CountryRegion("DE", "BAY", "Bayern", "de-DE"));
                mapper.Save(new CountryRegion("DE", "BAY", "Bavaria", "en-GB"));

                mapper.Save(new CountryRegion("DE", "THU", "Thüringen", "de-DE"));
                mapper.Save(new CountryRegion("DE", "THU", "Thuringia", "en-GB"));

                OBM.Commit(mapper, nested);

                /*
                 * Insert Bad Data
                 */
                nested = OBM.BeginTransaction(mapper);

                /*
                 * This should fail, because the country does not exists.
                 */
                mapper.Save(new CountryRegion("EN", "SCH", "Schottland", "de-DE"));
                mapper.Save(new CountryRegion("EN", "SCH", "Scotland", "en-GB"));

                try
                {
                    OBM.Commit(mapper, nested);
                    Assert.Fail("This should fail, because the country does not exists.");
                }
                catch (SqlCoreException sqlCoreException)
                {
                    // That is what we expect
                    Console.WriteLine(sqlCoreException.Message);
                }
            }
        }
        
    }
}
