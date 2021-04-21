using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data;
using AdFactum.Data.Linq;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This class test the new InCondition functionality
    /// </summary>
    [TestFixture]
    public class InConditionTest : ObjectMapperTest
    {
        private const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED_RIGHT_NOW";

        /// <summary>
        /// Ins the condition by value.
        /// </summary>
        [Test]
        public void InConditionByValue ()
        {
            /*
             * Insert some test contacts
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(new Contact("Fritz", "Bauer"));
                mapper.Save(new Contact("Annemarie", "Aal"));
                mapper.Save(new Contact("Hans", "Habicht"));
                mapper.Save(new Contact("Olaf", "Schubert"));
                OBM.Commit(mapper, nested);

                /*
                 * Search "Annemarie" and "Olaf" using the InCondition
                 */
                var condition = new InCondition(typeof (Contact), "FirstName", "Annemarie", "Olaf");
                var hechtFamily = mapper.Select(typeof(Contact), condition);

                /*
                 * We selected two contacts
                 */
                Assert.AreEqual(2, hechtFamily.Count, "We expected two objects.");
            }
        }

        /// <summary>
        /// Ins the condition by sub select.
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        public void InConditionBySubSelect()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Build a small company employee, company contacts structure
                 */
                var company = new FullFeaturedCompany("Milk farmer old John");
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
                    new CollectionJoin(typeof (FullFeaturedCompany), "Contacts", typeof (Contact)),
                    new AndCondition(typeof (Contact), "FirstName", "Hans"));
                var contactSubSelect = new SubSelect(typeof (FullFeaturedCompany), "Id", contactCondition);

                ICondition employeeCondition = new ConditionList(
                    new CollectionJoin(typeof (FullFeaturedCompany), "Employees", typeof (Employee)),
                    new AndCondition(typeof (Employee), "FirstName", "John"));
                var employeeSubSelect = new SubSelect(typeof (FullFeaturedCompany), "Id", employeeCondition);

                ICondition companyCondition = new InCondition(typeof (FullFeaturedCompany), "Id",
                                                              new Union(contactSubSelect, employeeSubSelect));

                var loaded =
                    (FullFeaturedCompany)mapper.Load(typeof(FullFeaturedCompany), companyCondition);
                Assert.IsNotNull(loaded, "Could not load company");
                Assert.AreEqual(company.Id, loaded.Id, "Company equals not expected company");
            }
        }


        /// <summary>
        /// Ins the condition by sub select.
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        [Category(NOT_IMPLEMENTED)]
        public void InConditionBySubSelectUsingLinq()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Build a small company employee, company contacts structure
                 */
                var company = new FullFeaturedCompany("Milk farmer old John");
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
                 * Now using linq
                 */
                var fullFeaturedCompanies = mapper.Query<FullFeaturedCompany>();
                var contactBridge = mapper.Query<LinkBridge<FullFeaturedCompany, Contact>>("FFCOMPANY_CONTACTS");
                var employeeBridge = mapper.Query<LinkBridge<FullFeaturedCompany, Employee>>("FFCOMPANY_EMPLOYEES");
                var employees = mapper.Query<Employee>();
                var contacts = mapper.Query<Contact>();

                var hans = from bridge in contactBridge
                           from contact in contacts
                           from fcompany in fullFeaturedCompanies

                           where bridge.Client == contact
                                 && fcompany == bridge.Parent
                                 && contact.FirstName == "Hans"

                           select fcompany;


                var john = from bridge in employeeBridge
                           join fcompany in fullFeaturedCompanies on bridge.Parent equals fcompany
                           join employee in employees on bridge.Client equals employee

                           where employee.FirstName == "John"

                           select fcompany;

                var unionCompany = (from fcompany in fullFeaturedCompanies
                                    where hans.Union(john).Contains(fcompany)
                                    select fcompany).First();

                Assert.IsNotNull(unionCompany, "Could not load company");
                Assert.AreEqual(company.Id, unionCompany.Id, "Company equals not expected company");
                ObjectDumper.Write(unionCompany);
            }
        }
    }
}
