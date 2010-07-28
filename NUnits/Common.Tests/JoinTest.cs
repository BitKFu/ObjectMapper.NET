using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Linq;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Test the join feature
    /// </summary>
    [TestFixture]
    public class JoinTest : ObjectMapperTest
    {

        /// <summary>
        /// Collections the join objects.
        /// </summary>
        [Test]
        public void CollectionJoinObjects()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var emp = new Employee("All", "Mine");
                var c1 = new FullFeaturedCompany("Heavy stuff");
                c1.Employees.Add(emp);
                var c2 = new FullFeaturedCompany("Light stuff");
                c2.Employees.Add(emp);

                var emp2 = new Employee("Thats", "not mine");
                var c3 = new FullFeaturedCompany("Other stuff");
                c3.Employees.Add(emp2);

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(typeof (FullFeaturedCompany));
                mapper.Save(emp); // Store this object first, because it's used twice
                mapper.Flush();

                mapper.Save(c1);
                mapper.Save(c2);
                mapper.Save(c3);
                OBM.Commit(mapper, nested);

                /*
                 * Join company c1 object with owner object
                 */
                var loaded = (FullFeaturedCompany) mapper.FlatLoad(typeof (FullFeaturedCompany), new CollectionJoin(c1, "Employees", emp));
                Assert.AreEqual(c1.Id, loaded.Id, "Could not load company");

                var linqLoaded = (from ffCompany in mapper.Query<FullFeaturedCompany>()
                                  from employee in mapper.Query<LinkBridge<FullFeaturedCompany, Employee>>("FFCOMPANY_EMPLOYEES")
                                  where ffCompany == employee.Parent && ffCompany == c1
                                  select ffCompany).Single();
                Assert.AreEqual(c1.Id, linqLoaded.Id, "Could not load company");


                /*
                 * Join typeof(Company) with owner object
                 */
                IList selection = mapper.FlatSelect(typeof (FullFeaturedCompany), new CollectionJoin(typeof (FullFeaturedCompany), "Employees", emp));
                Assert.AreEqual(2, selection.Count, "Could not find the 2 expected companies.");

                /*
                 * Join typeof(Company) with typeof(Employee)
                 */
                selection = mapper.FlatSelect(typeof (FullFeaturedCompany),
                                              new CollectionJoin(typeof (FullFeaturedCompany), "Employees",
                                                                 typeof (Employee)));
                Assert.AreEqual(3, selection.Count, "Could not find the 3 expected companies.");
            }
        }

        /// <summary>
        /// Joins the objects.
        /// </summary>
        [Test]
        public void JoinObjects()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var emp = new Employee("All", "Mine");
                var c1 = new FullFeaturedCompany("Heavy stuff");
                c1.Owner = emp;
                var c2 = new FullFeaturedCompany("Light stuff");
                c2.Owner = emp;

                var emp2 = new Employee("Thats", "not mine");
                var c3 = new FullFeaturedCompany("Other stuff");
                c3.Owner = emp2;

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(typeof (FullFeaturedCompany));
                mapper.Save(emp); // Store this first, because it's used twice
                mapper.Flush();

                mapper.Save(c1);
                mapper.Save(c2);
                mapper.Save(c3);
                OBM.Commit(mapper, nested);

                /*
                 * Join company c1 object with owner object
                 */
                var loaded = (FullFeaturedCompany) mapper.FlatLoad(typeof (FullFeaturedCompany),
                                                                                   new Join(c1, "Owner", emp));
                Assert.AreEqual(c1.Id, loaded.Id, "Could not load company");

                /*
                 * Join typeof(Company) with owner object
                 */
                var selection = mapper.FlatSelect(typeof (FullFeaturedCompany),
                                                    new Join(typeof (FullFeaturedCompany), "Owner", emp));
                Assert.AreEqual(2, selection.Count, "Could not find the 2 expected companies.");

                /*
                 * Join typeof(Company) with typeof(Employee)
                 */
                selection = mapper.FlatSelect(typeof (FullFeaturedCompany),
                                              new Join(typeof (FullFeaturedCompany), "Owner", typeof (Employee)));
                Assert.AreEqual(3, selection.Count, "Could not find the 3 expected companies.");
            }
        }
    }
}