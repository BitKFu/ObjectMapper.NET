using System;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class OneToManyTest : ObjectMapperTest
    {
        [Test]
        public void CheckCreationOfEmployees()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                var loadedCompany = mapper.Load(typeof(Company), company.Id) as Company;
                Assert.IsNotNull(loadedCompany);
                Assert.AreEqual(fullFeaturedCompany.Employees.Count, loadedCompany.Employees.Count, "Count of Employees must equal.");
            }
        }

        [Test]
        public void ModifyAnEmployee()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                var loadedCompany = (Company) mapper.Load(typeof(Company), company.Id);
                loadedCompany.Employees.Find(emp => emp.LastName == "Uhlen").LastName = "Bubenhausen";

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(loadedCompany);
                OBM.Commit(mapper, nested);

                // Reload to check changes
                loadedCompany = (Company) mapper.Load(typeof(Company), company.Id);
                Assert.IsTrue(loadedCompany.Employees.Any(emp=>emp.LastName=="Bubenhausen"));
            }
        }

        [Test]
        public void TryToDeleteAnEmployee()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                var loadedCompany = (Company)mapper.Load(typeof(Company), company.Id);
                loadedCompany.Employees.Remove(loadedCompany.Employees.Find(emp => emp.LastName == "Kuckuck"));

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(loadedCompany);
                OBM.Commit(mapper, nested);

                // Reload to check changes
                loadedCompany = (Company)mapper.Load(typeof(Company), company.Id);
                Assert.IsFalse(loadedCompany.Employees.Any(emp => emp.LastName == "Kuckuck"));
            }
        }
    }
}
