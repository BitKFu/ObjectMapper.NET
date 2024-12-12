using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{

    [TestFixture]
    public class ManyToManyTest : ObjectMapperTest
    {

        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForPostgres")]
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

                ClassicAssert.AreEqual(1, result);
            }
        }
    }
}
