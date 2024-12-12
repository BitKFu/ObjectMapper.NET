using System;
using System.Collections.Generic;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This example shows how unique keys are working.
    /// </summary>
    [TestFixture]
    public class UniqueKeys : ObjectMapperTest
    {
        /// <summary>
        /// Tests the key groups.
        /// </summary>
        [Test]
        public void TestKeyGroups ()
        {
            Translation german = new Translation("Welcome", "Herzlich Willkommen!", "de-DE");    
            Translation english = new Translation("Welcome", "Happy welcome!", "en-GB");
            
            /*
             * Try a good case, save both translations
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(german);
                mapper.Save(english);
                OBM.Commit(mapper, nested);

                /*
                 * Create a second object with the same key and locale - now we must expect an exception
                 */
                Translation exception = new Translation("Welcome", "We expect an exception", "en-GB");
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(exception);

                try
                {
                    OBM.Commit(mapper, nested);
                    ClassicAssert.Fail("Our combined unique key did not fire an exception.");
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
