using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This test class tests the unicode functionality of the ObjectMapper .NET
    /// </summary>
    [TestFixture]
    public class UnicodeTest : ObjectMapperTest
    {

        /// <summary>
        /// Loads the save unicode.
        /// </summary>
        [Test]
        public void LoadSaveUnicode ()
        {
             using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
             {
                 var ute = new UnicodeTestEntity();
                 ute.UnicodeChar = UnicodeTestEntity.constUnicodeChar;
                 ute.UnicodeString = UnicodeTestEntity.constUnicodeString;
                 ute.UnicodeMemo = UnicodeTestEntity.constUnicodeMemo;

                 bool nested = OBM.BeginTransaction(mapper);
                 mapper.Save(ute);
                 OBM.Commit(mapper, nested);

                 var loaded = mapper.Load(typeof(UnicodeTestEntity), ute.Id) as UnicodeTestEntity;
                 ClassicAssert.IsNotNull(loaded, "Could not load test entity");
                 ClassicAssert.AreEqual(ute.UnicodeChar, loaded.UnicodeChar, "Failed to compare unicode char");
                 ClassicAssert.AreEqual(ute.UnicodeString, loaded.UnicodeString, "Failed to compare unicode string");
                 ClassicAssert.AreEqual(ute.UnicodeMemo, loaded.UnicodeMemo, "Failed to compare unicode memo");
             }
        }
    }
}
