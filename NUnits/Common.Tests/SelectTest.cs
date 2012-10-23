using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class SelectTest : ObjectMapperTest
    {
        private Contact contact1 = new Contact("Bernd", "Beispiel");
        private Contact contact2 = new Contact("Luise", "Lustig");
        private Contact contact3 = new Contact("Peter", "Lustig");
        private Contact contact4 = new Contact("Bernd", "das Brot");

        [SetUp]
        public void Setup()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(contact1);
                mapper.Save(contact2);
                mapper.Save(contact3);
                mapper.Save(contact4);
                OBM.Commit(mapper, nested);
            }
        }

        [TearDown]
        public void Teardown()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(contact1);
                mapper.Delete(contact2);
                mapper.Delete(contact3);
                mapper.Delete(contact4);
                OBM.Commit(mapper, nested);
            }
        }

        [Test]
        public void Select_AndCondition_ReturnsValidResult()
        {
            // Arrange & Act
            IList actual = null;
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var condition = new AndCondition(typeof(Contact), "FirstName", "Bernd");

                actual = mapper.Select(typeof(Contact), condition);
            }

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
        }

        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForSqlServer")]
        [Category("ExcludeForSqlServerCE")]
        [Category("ExcludeForPostgres")]
        public void Select_AndConditionWithHint_ReturnsValidResult()
        {
            // Arrange & Act
            IList actual = null;
            var hintLogger = new HintLogger();
            OBM.CurrentSqlTracer = hintLogger;
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var condition = new AndCondition(typeof(Contact), "FirstName", "Bernd");
                condition.Add(new HintCondition("ORDERED"));

                actual = mapper.Select(typeof(Contact), condition);
            }

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(hintLogger.LastSqlStatement.Contains("ORDERED"));
        }

        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForSqlServer")]
        [Category("ExcludeForSqlServerCE")]
        [Category("ExcludeForPostgres")]
        public void Select_HintConditionWithAnd_ReturnsValidResult()
        {
            // Arrange & Act
            IList actual = null;
            var hintLogger = new HintLogger();
            OBM.CurrentSqlTracer = hintLogger;
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var condition = new HintCondition("ORDERED");
                condition.Add(new AndCondition(typeof(Contact), "FirstName", "Bernd"));

                actual = mapper.Select(typeof(Contact), condition);
            }

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(hintLogger.LastSqlStatement.Contains("ORDERED"));
        }

        private class HintLogger : ConsoleTracer
        {
            public string LastSqlStatement { get; set; }

            public override void SqlCommand(System.Data.IDbCommand original, string extended, int affactedRows, TimeSpan duration)
            {
                base.SqlCommand(original, extended, affactedRows, duration);
                LastSqlStatement = extended;
            }
        }
    }
}
