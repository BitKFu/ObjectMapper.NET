using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class ListBinding : ObjectMapperTest 
    {
        /// <summary>
        /// Tests the Ilist binding.
        /// </summary>
        [Test]
        public void TestIListBinding ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create data
                 */
                Company_IList_Contacts company01 = new Company_IList_Contacts();
                company01.LegalName = "c1";

                /*
                 * Create contacts
                 */
                company01.Contacts.Add(new Contact("Daniel", "Düsentrieb"));
                company01.Contacts.Add(new Contact("Herbert", "Hartwig"));

                /*
                 * Store data
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Because it's not bound to Contact we can store Friends too
                 */
                company01.Contacts.Add(new Friend("John", "Important", new DateTime(1980, 05, 22)));
                company01.Contacts.Add(new Friend("Peter", "GoAway", new DateTime(1978, 02, 13)));

                /*
                 * Store data
                 */
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load the company
                 */
                Company_IList_Contacts result;
                result = mapper.Load(typeof(Company_IList_Contacts), company01.Id) as Company_IList_Contacts;
                Assert.IsNotNull(result, "Company could not be loaded.");
                Assert.IsTrue(result.Contacts.Count == 4, "Not all contacts could be loaded.");
            }
        }

        /// <summary>
        /// Tests the Ilist binding.
        /// </summary>
        [Test]
        public void TestGeneralLinkBinding()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create data
                 */
                Company_GeneralLink_Contacts company01 = new Company_GeneralLink_Contacts();
                company01.LegalName = "c1";

                /*
                 * Create contacts
                 */
                company01.Contacts.Add(new Contact("Daniel", "Düsentrieb"));
                company01.Contacts.Add(new Contact("Herbert", "Hartwig"));

                /*
                 * Store data
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Because it's not bound to Contact we can store Friends too
                 */
                company01.Contacts.Add(new Friend("John", "Important", new DateTime(1980, 05, 22)));
                company01.Contacts.Add(new Friend("Peter", "GoAway", new DateTime(1978, 02, 13)));

                /*
                 * Store data
                 */
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load the company
                 */
                Company_GeneralLink_Contacts result;
                result = mapper.Load(typeof(Company_GeneralLink_Contacts), company01.Id) as Company_GeneralLink_Contacts;
                Assert.IsNotNull(result, "Company could not be loaded.");
                Assert.IsTrue(result.Contacts.Count == 4, "Not all contacts could be loaded.");
            }
        }

        /// <summary>
        /// Tests the Ilist binding.
        /// </summary>
        [Test]
        public void TestGenericListBinding()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create data
                 */
                Company_GenericList_Contacts company01 = new Company_GenericList_Contacts();
                company01.LegalName = "c1";

                /*
                 * Create contacts
                 */
                company01.Contacts.Add(new Contact("Daniel", "Düsentrieb"));
                company01.Contacts.Add(new Contact("Herbert", "Hartwig"));

                /*
                 * Store data
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load the company
                 */
                Company_GenericList_Contacts result;
                result = mapper.Load(typeof(Company_GenericList_Contacts), company01.Id) as Company_GenericList_Contacts;
                Assert.IsNotNull(result, "Company could not be loaded.");
                Assert.AreEqual(2, result.Contacts.Count, "Not all contacts could be loaded.");
            }
        }

        /// <summary>
        /// Tests the Ilist binding.
        /// </summary>
        [Test]
        public void TestBoundIListBinding()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create data
                 */
                Company_BoundIList_Contacts company01 = new Company_BoundIList_Contacts();
                company01.LegalName = "c1";

                /*
                 * Create contacts
                 */
                company01.Contacts.Add(new Contact("Daniel", "Düsentrieb"));
                company01.Contacts.Add(new Contact("Herbert", "Hartwig"));

                /*
                 * Store data
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company01);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load the company
                 */
                Company_BoundIList_Contacts result;
                result = mapper.Load(typeof(Company_BoundIList_Contacts), company01.Id) as Company_BoundIList_Contacts;
                Assert.IsNotNull(result, "Company could not be loaded.");
                Assert.AreEqual(2, result.Contacts.Count, "Not all contacts could be loaded.");
            }
        }


    }
}
