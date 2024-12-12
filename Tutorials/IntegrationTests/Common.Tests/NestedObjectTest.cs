using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class NestedObjectTest : ObjectMapperTest
    {
        /// <summary>
        /// Gets the nested object.
        /// </summary>
        [Test]
        public void GetNestedObject ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                PhoneBookEntry entry = new PhoneBookEntry(
                    new Contact("Daniel", "Düsentrieb"),
                    new Contact("Herbert", "Hartwig"), "04123-12asd");

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(entry);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load persons with getnestedobject
                 */
                IPerson result = mapper.GetNestedObject(typeof(PhoneBookEntry), "Person", entry.Id, HierarchyLevel.FlatObject) as IPerson;

                ClassicAssert.IsNotNull(result, "Persone of phone book entry 1 could not be loaded.");
                ClassicAssert.AreEqual(entry.Person.Id, result.Id, "Ids of result 1 aren't equal.");
            }   
        }

        /// <summary>
        /// Saves the nested objects split.
        /// </summary>
        [Test]
        public void TrySplittedSave()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                FullFeaturedCompany ffc = new FullFeaturedCompany("Splitting AG");

                Contact c1 = new Contact("Daniel", "Düsentrieb");
                Contact c2 = new Contact("Herbert", "Hartwig");
                Contact c3 = new Contact("Pisata", "Bernod");
                Contact c4 = new Contact("Mercur", "Monarch");

                ffc.Contacts.Add(c1);
                ffc.Contacts.Add(c2);
                ffc.Contacts.Add(c3);
                ffc.Contacts.Add(c4);

                ffc.PhoneBook.Add(new PhoneBookEntry(c1, c2, "04123-12asd"));
                ffc.PhoneBook.Add(new PhoneBookEntry(c3, c4, "041232-sasd"));

                /*
                 * Save PhoneBook Entries first
                 */
                bool nested = OBM.BeginTransaction(mapper);

                foreach (Contact c in ffc.Contacts)
                    mapper.FlatSave(c);

                foreach (PhoneBookEntry pbe in ffc.PhoneBook)
                    mapper.Save(pbe, HierarchyLevel.FlatObjectWithLinks);

                mapper.Save(ffc, HierarchyLevel.FlatObjectWithLinks);
                OBM.Commit(mapper, nested);

                /*
                 * Load stored entry
                 */
                ffc = mapper.Load(typeof (FullFeaturedCompany), ffc.Id) as FullFeaturedCompany;
                ClassicAssert.IsNotNull(ffc, "FullFeaturedCompany could not be loaded.");
            }
        }
    }
}
