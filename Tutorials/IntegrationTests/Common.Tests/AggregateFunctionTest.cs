using System;
using System.Collections.Generic;
using AdFactum.Data;
using AdFactum.Data.Projection.Attributes;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// TimeEntryAggregation
    /// </summary>
    public class TimeEntryAggregation
    {
        /// <summary>
        /// Gets or sets the first date.
        /// </summary>
        /// <value>The first date.</value>
        [ProjectOntoProperty(typeof (TimeEntry), "StartDate")]
        [Min]
        public DateTime FirstDate { get; set; }
    }

    /// <summary>
    /// Groups the timeentries by project
    /// </summary>
    public class TimeEntryGrouping : TimeEntryAggregation
    {
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>The project.</value>
        [ProjectOntoProperty(typeof (TimeEntry), "ProjectId")]
        [GroupBy]
        public string Project { get; set; }
    }

    /// <summary>
    /// Test the ability to use aggregate functions
    /// </summary>
    [TestFixture]
    public class AggregateFunctionTest :  ObjectMapperTest
    {

        /// <summary>
        /// Tests the agregation functionality
        /// </summary>
        [Test]
        public void FirstEntryTest ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                InsertTimeEntriesForTest();

                var aggregate = mapper.Load(typeof(TimeEntryAggregation), null as ICondition) as TimeEntryAggregation;
                ClassicAssert.IsNotNull(aggregate);
                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), aggregate.FirstDate);

                var result = 
                    new List<TimeEntryAggregation>(
                        new ListAdapter<TimeEntryAggregation>(
                    mapper.FlatSelect(typeof(TimeEntryAggregation))));

                ClassicAssert.AreEqual(1, result.Count, "One timeentry expected.");
                
                TimeEntryAggregation firstEntry = result[0];
                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), firstEntry.FirstDate);
            }
        }

        /// <summary>
        /// Test the grouping functionality
        /// </summary>
        [Test]
        public void GroupingTest()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                InsertTimeEntriesForTest();

                /*
                 * Flat Select
                 */
                var result =
                    new List<TimeEntryGrouping>(
                        new ListAdapter<TimeEntryGrouping>(
                    mapper.FlatSelect(typeof(TimeEntryGrouping))));

                ClassicAssert.AreEqual(2, result.Count, "Two timeentries expected.");

                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), result[0].FirstDate);
                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), result[1].FirstDate);

                /*
                 * Paging  - first entry
                 */
                result = new List<TimeEntryGrouping>(
                        new ListAdapter<TimeEntryGrouping>(
                    mapper.FlatPaging(typeof(TimeEntryGrouping), null, 1,1)));

                ClassicAssert.AreEqual(1, result.Count, "One timeentry expected.");
                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), result[0].FirstDate);
            }
        }

        /// <summary>
        /// Tests the having clause
        /// </summary>
        [Test]
        public void HavingClauseTest()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                InsertTimeEntriesForTest();

                ICondition havingCondition = new AndCondition(typeof (TimeEntryGrouping), "FirstDate",
                                                              QueryOperator.GreaterEqual, new DateTime(2008, 01, 01));

                /*
                 * Flat Select with having condition
                 */
                var result =
                    new List<TimeEntryGrouping>(
                        new ListAdapter<TimeEntryGrouping>(
                    mapper.FlatSelect(typeof(TimeEntryGrouping), havingCondition)));

                ClassicAssert.AreEqual(2, result.Count, "Two timeentries expected.");

                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), result[0].FirstDate);
                ClassicAssert.AreEqual(new DateTime(2008, 01, 01), result[1].FirstDate);
            }
        }

        /// <summary>
        /// Inserts the time entries for test.
        /// </summary>
        private void InsertTimeEntriesForTest()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Delete(typeof(TimeEntry));
                mapper.Save(new TimeEntry(new DateTime(2008, 01, 10), new DateTime(2008, 01, 12), "AOM" ,"2 Days in Munich"));
                mapper.Save(new TimeEntry(new DateTime(2008, 01, 01), new DateTime(2008, 01, 01), "AOM", "New year"));
                mapper.Save(new TimeEntry(new DateTime(2008, 02, 14), new DateTime(2008, 02, 14), "AOM", "Valentine Day"));

                mapper.Save(new TimeEntry(new DateTime(2008, 01, 10), new DateTime(2008, 01, 12), "TP", "2 Days in Munich"));
                mapper.Save(new TimeEntry(new DateTime(2008, 01, 01), new DateTime(2008, 01, 01), "TP", "New year"));
                mapper.Save(new TimeEntry(new DateTime(2008, 02, 14), new DateTime(2008, 02, 14), "TP", "Valentine Day"));

                OBM.Commit(mapper, nested);
            }
        }
    }
}
