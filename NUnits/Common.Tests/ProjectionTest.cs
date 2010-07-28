using System;
using System.Collections;
using System.Collections.Generic;
using AdFactum.Data;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ProjectionTest : ObjectMapperTest
    {

        /// <summary>
        /// This test tries to project only a part of the resultset to a new object
        /// </summary>
        [Test]
        public void TestMemberProjection()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                /*
                 * Insert some friends
                 */
                mapper.Save(new Friend("Karl", "Hans-Peter", new DateTime(1975, 10, 3)));
                mapper.Save(new Friend("Susi", "Salbei", new DateTime(1977, 5, 7)));
                mapper.Save(new Friend("Herbert", "Habicht", new DateTime(1979, 2, 4)));

                OBM.Commit(mapper, nested);

                /*
                 * SELECT and project to an other destination
                 */
                var names = new List<FriendName>(new ListAdapter<FriendName>(mapper.Select(typeof(FriendName),
                    new InCondition(typeof(Friend), "LastName", "Hans-Peter", "Salbei", "Habicht"))));
                ObjectDumper.Write(names);
                Assert.AreEqual(3, names.Count);

            }
        }

        /// <summary>
        /// Tests the advanced projection.
        /// </summary>
        [Test]
        public void TestAdvancedProjection()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                FullFeaturedCompany ffc1 = new FullFeaturedCompany("Global Player 1");
                ffc1.Employees.Add(new Employee("Tante", "Mathilde"));
                ffc1.Employees.Add(new Employee("Onkel", "Herbert"));
                ffc1.Employees.Add(new Employee("Papa", "Wutz"));

                FullFeaturedCompany ffc2 = new FullFeaturedCompany("Global Player 2");
                ffc2.Employees.Add(new Employee("Oma", "Hilde"));
                ffc2.Employees.Add(new Employee("Opa", "Kunze"));
                ffc2.Employees.Add(new Employee("Mama", "Po"));

                bool nested = OBM.BeginTransaction(mapper);

                mapper.Save(ffc1);
                mapper.Save(ffc2);

                OBM.Commit(mapper, nested);

                /*
                 * SELECT and project to an other destination
                 */
                List<FullFeaturedEmployee> names = new List<FullFeaturedEmployee>(
                    new ListAdapter<FullFeaturedEmployee>(
                        mapper.Select(typeof(FullFeaturedEmployee), 
                            new CollectionJoin(typeof(FullFeaturedCompany), "Employees", typeof(Employee)))));

                ObjectDumper.Write(names);
                Assert.AreEqual(6, names.Count);

                /*
                 * Count that advanced projection
                 */
                int counter = mapper.Count(typeof(FullFeaturedEmployee), new CollectionJoin(typeof(FullFeaturedCompany), "Employees", typeof(Employee)));
                Assert.AreEqual(6, counter);
            }
        }

        /// <summary>
        /// Tests the table replacement.
        /// </summary>
        [Test]
        [Category("ExcludeForSqlServerCE")]
        public void TestTableReplacement()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                /*
                 * Insert some friends
                 */
                mapper.Delete(typeof(Contact));
                mapper.Save(new Contact("Karl", "Hans-Peter"));
                mapper.Save(new Contact("Susi", "Salbei"));
                mapper.Save(new Contact("Herbert", "Habicht"));

                OBM.Commit(mapper, nested);

                /*
                 * Search using Table replacement
                 */
                IList friends = mapper.FlatSelect(typeof(Friend),
                                                  new TableReplacement(typeof(Friend), "SELECT * FROM #SR#CONTACTS"));

                // Load contacts to friends table
                Assert.AreEqual(3, friends.Count);
            }
        }

        /// <summary>
        /// Tests the sub select table replacement.
        /// </summary>
        [Test]
        [Category("ExcludeForSqlServerCE")]
        [Category("ExcludeForAccess")]
        public void TestSubSelectTableReplacement()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Build a small company employee, company contacts structure
                 */
                FullFeaturedCompany company = new FullFeaturedCompany("Milk farmer old John");
                company.Contacts.Add(new Contact("Hans", "Hecht"));
                company.Contacts.Add(new Contact("Susanne", "Hecht"));
                company.Contacts.Add(new Contact("Karl", "Hecht"));
                company.Contacts.Add(new Contact("Werner", "Hecht"));

                company.Employees.Add(new Employee("John", "Milk Farmer"));
                company.Employees.Add(new Employee("Lilly", "Milk Farmer"));

                mapper.BeginTransaction();
                mapper.Save(company);
                mapper.Commit();

                /*
                 * Select the company where a contact is called "Hans" as Firstname and the Employee is called "John" als FirstName
                 */
                ICondition contactCondition = new ConditionList(
                    new CollectionJoin(company, "Contacts", typeof(Contact)),
                    new AndCondition(typeof(Contact), "FirstName", "Hans"),
                    new TableReplacement(typeof(Contact), "SELECT * FROM #SR#CONTACTS"));
                SubSelect contactSubSelect = new SubSelect(typeof(FullFeaturedCompany), "Id", contactCondition);

                ICondition employeeCondition = new ConditionList(
                    new CollectionJoin(company, "Employees", typeof(Employee)),
                    new AndCondition(typeof(Employee), "FirstName", "John"),
                    new TableReplacement(typeof(Contact), "SELECT * FROM #SR#EMPLOYEE"));
                SubSelect employeeSubSelect = new SubSelect(typeof(FullFeaturedCompany), "Id", employeeCondition);

                ICondition companyCondition = new InCondition(typeof(FullFeaturedCompany), "Id",
                                                              new Union(contactSubSelect, employeeSubSelect));

                FullFeaturedCompany loaded =
                    (FullFeaturedCompany)mapper.Load(typeof(FullFeaturedCompany), companyCondition);
                Assert.IsNotNull(loaded, "Could not load company");
                Assert.AreEqual(company.Id, loaded.Id, "Company equals not expected company");
            }
        }
    }
}
