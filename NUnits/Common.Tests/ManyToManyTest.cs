using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class ManyToManyTest : ObjectMapperTest
    {
        FullFeaturedCompany company = new FullFeaturedCompany("ManyToMany")
        {
            Employees = new List<Employee>()
                                      {
                                          new Employee("Sven", "Björndal"),
                                          new Employee("Marius", "Uhlen"),
                                          new Employee("Bonsai", "Bandito"),
                                          new Employee("Hermann", "Kuckuck"),
                                          new Employee("Peter", "Witzigmann"),
                                      },

            PhoneBook = new List<PhoneBookEntry>()
                            {
                                new PhoneBookEntry(null, null, "0172-82956789")        ,
                                new PhoneBookEntry(null, null, "0151-58786245")
                            }
        };

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company);
                OBM.Commit(mapper, nested);

                company = mapper.Load(typeof (FullFeaturedCompany), company.Id) as FullFeaturedCompany;
            }
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(company);
                OBM.Commit(mapper, nested);
            }
        }

        [Test]
        public void TestDualManyToManyLinkBridge()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                var companies = mapper.Query<FullFeaturedCompany>();
                var companiesEmployees = mapper.Query<LinkBridge<FullFeaturedCompany, Employee>>("FFCompany_Employees");
                var employees = mapper.Query<Employee>();
                var companiesPhonebook = mapper.Query<LinkBridge<FullFeaturedCompany, PhoneBookEntry>>("FFCompany_Phonebook");
                var phonebooks = mapper.Query<PhoneBookEntry>();

                // select the company using two linkBridges
                var result = (from company in companies
                              from companyEmployee in companiesEmployees
                              from employee in employees
                              from companyPhonebook in companiesPhonebook
                              from phonebook in phonebooks

                              where company == companyEmployee.Parent
                                    && companyEmployee.Client == employee
                                    && employee.LastName == "Witzigmann"
                                    && company == companyPhonebook.Parent
                                    && companyPhonebook.Client == phonebook
                                    && phonebook.PhoneNumber == "0151-58786245"

                              select company
                             ).Count();

                Assert.AreEqual(1, result);
            }
        }
    }
}
