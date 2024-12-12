using System;
using System.Collections.Generic;
using AdFactum.Data;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This class is used to test the with clause functionality
    /// </summary>
    [TestFixture]
    public class WithClauseTest : ObjectMapperTest
    {

        /// <summary>
        /// Withes the clause sub select test .
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForSqlServerCE")]
        public void WithClauseSubSelect()
        {
            /*
             * Insert some example data
             */
            Company c1 = new Company("Schnorrer AG");
            Activity a1 = new Activity(c1, DateTime.Today, "Von Kollegen schnorren");
            Activity a2 = new Activity(c1, DateTime.Today.AddDays(1), "Nochmal schnorren");
            Activity a3 = new Activity(c1, DateTime.Today.AddDays(10), "Alle nerven");

            Company c2 = new Company("Ich nicht AG");
            Activity a4 = new Activity(c2, DateTime.Today, "Alles wird gut");

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Delete(typeof(Activity));
                mapper.Delete(typeof(Company));

                mapper.Save(c1);
                OBM.Flush(mapper);

                mapper.Save(a1, HierarchyLevel.FlatObjectWithLinks);
                mapper.Save(a2, HierarchyLevel.FlatObjectWithLinks);
                mapper.Save(a3, HierarchyLevel.FlatObjectWithLinks);

                mapper.Save(c2);
                OBM.Flush(mapper);

                mapper.Save(a4, HierarchyLevel.FlatObjectWithLinks);

                OBM.Commit(mapper, nested);
            }

            /*
             * Do subselect
             */
            WithClause subClause = new WithClause("sub01", typeof(Activity), 
                new Join(typeof(Company), "Id", typeof(Activity), "Company"),
                new AndCondition(typeof(Company), "Id", c1));

            SubSelect activityId01 = new SubSelect(typeof(Activity), "Id", 
                new AndCondition(typeof(Activity), "ActivityDate", DateTime.Today),
                new TableReplacement(typeof(Activity), "sub01"));

            SubSelect activityId02 = new SubSelect(typeof(Activity), "Id",
                new AndCondition(typeof(Activity), "ActivityDate", DateTime.Today.AddDays(1)),
                new TableReplacement(typeof(Activity), "sub01"));

            ICondition selection = new ConditionList(
                subClause,
                new InCondition(typeof(Activity), "Id", new UnionAll(activityId01, activityId02)));

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Count the activities
                 */
                int result = mapper.Count(typeof(Activity), selection);
                ClassicAssert.AreEqual(2, result, "Only 1 result is expected");
            }

        }

        /// <summary>
        /// Withes the clause select.
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForSqlServerCE")]
        public void WithClauseSelect()
        {
            /*
             * Insert some example data
             */
            Company c1 = new Company("Schnorrer AG");
            Activity a1 = new Activity(c1, DateTime.Today, "Von Kollegen schnorren");
            Activity a2 = new Activity(c1, DateTime.Today.AddDays(1), "Nochmal schnorren");
            Activity a3 = new Activity(c1, DateTime.Today.AddDays(10), "Alle nerven");

            Company c2 = new Company("Ich nicht AG");
            Activity a4 = new Activity(c2, DateTime.Today, "Alles wird gut");

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Delete(typeof(Activity));
                mapper.DeleteRecursive(typeof(Company), HierarchyLevel.AllDependencies);

                mapper.Save(c1);
                OBM.Flush(mapper);

                mapper.Save(a1, HierarchyLevel.FlatObjectWithLinks);
                mapper.Save(a2, HierarchyLevel.FlatObjectWithLinks);
                mapper.Save(a3, HierarchyLevel.FlatObjectWithLinks);

                mapper.Save(c2);
                OBM.Flush(mapper);

                mapper.Save(a4, HierarchyLevel.FlatObjectWithLinks);

                OBM.Commit(mapper, nested);
            }

            /*
             * Select using the with clause , only company 1
             */
            ConditionList selection = new ConditionList(
                new WithClause("SUB01", typeof (Activity), new AndCondition(typeof (Activity), "Company", c1)),     // create with clause
                new TableReplacement(typeof(Activity), "SUB01"),                                                    // use it!

                new AndCondition(typeof(Activity), "ActivityDate", DateTime.Today)
            );

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Count the activities
                 */
                int result = mapper.Count(typeof (Activity), selection);
                ClassicAssert.AreEqual(1, result, "Only 1 result is expected");

                /*
                 * Load the activity
                 */
                Activity loaded = mapper.FlatLoad(typeof(Activity), selection) as Activity;
                ClassicAssert.IsNotNull(loaded, "Could not load activity");
                ClassicAssert.AreEqual(a1.Id, loaded.Id, "Found unexpected object");
            }
        }
    }
}
