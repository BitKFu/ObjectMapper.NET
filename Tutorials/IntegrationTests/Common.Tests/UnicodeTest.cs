using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
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
                 Assert.IsNotNull(loaded, "Could not load test entity");
                 Assert.AreEqual(ute.UnicodeChar, loaded.UnicodeChar, "Failed to compare unicode char");
                 Assert.AreEqual(ute.UnicodeString, loaded.UnicodeString, "Failed to compare unicode string");
                 Assert.AreEqual(ute.UnicodeMemo, loaded.UnicodeMemo, "Failed to compare unicode memo");
             }
        }
    }
}
