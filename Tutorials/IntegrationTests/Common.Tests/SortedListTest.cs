using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class SortedListTest : ObjectMapperTest 
    {
        /// <summary>
        /// Loads the save sorted list.
        /// 
        /// Remark: I know that my example is not a real world example, because you can't add a birthday twice.
        ///         But keep in mind, that this is only an example for using SortedList, not for implementing
        ///         a birthday list.
        /// </summary>
        [Test]
        public void LoadSaveSortedList ()
        {
            BirthdayList loadedList;

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                IPerson c1 = new Contact("Andrew", "Miller");
                IPerson c2 = new Contact("Peter", "Edenburgh");
                IPerson c3 = new Contact("Susan", "Miller");

                string keyC1 = new DateTime(1974, 03, 24).ToShortDateString();
                string keyC2 = new DateTime(1972, 02, 08).ToShortDateString();
                string keyC3 = new DateTime(1973, 06, 16).ToShortDateString();

                BirthdayList birthdays = new BirthdayList();
                birthdays.Contacts.Add(keyC1, c1);
                birthdays.Contacts.Add(keyC2, c2);
                birthdays.Contacts.Add(keyC3, c3);

                mapper.BeginTransaction();
                mapper.Save(birthdays);
                mapper.Commit();

                // Load list
                loadedList = mapper.Load(typeof(BirthdayList), birthdays.Id) as BirthdayList;
                Assert.IsNotNull(loadedList, "List could not be loaded.");
                Assert.AreEqual(birthdays.Contacts.Count, loadedList.Contacts.Count, "Not all contacts could be loaded.");

                foreach (KeyValuePair<string, IPerson> pair in loadedList.Contacts)
                    Console.WriteLine(string.Concat(pair.Key, " ", pair.Value.FirstName, " ", pair.Value.LastName));

                // Sort Contacts
                c1 = loadedList.Contacts[keyC1];
                c2 = loadedList.Contacts[keyC2];
                c3 = loadedList.Contacts[keyC3];

                loadedList.Contacts = new SortedList<string, IPerson>();
                loadedList.Contacts.Add(keyC1, c3);
                loadedList.Contacts.Add(keyC2, c2);
                loadedList.Contacts.Add(keyC3, c1);

                mapper.BeginTransaction();
                mapper.Save(loadedList);
                mapper.Commit();

                // Load list
                loadedList = mapper.Load(typeof(BirthdayList), loadedList.Id) as BirthdayList;
                Assert.IsNotNull(loadedList, "List could not be loaded.");
                Assert.AreEqual(birthdays.Contacts.Count, loadedList.Contacts.Count, "Not all contacts could be loaded.");

                foreach (KeyValuePair<string, IPerson> pair in loadedList.Contacts)
                    Console.WriteLine(string.Concat(pair.Key, " ", pair.Value.FirstName, " ", pair.Value.LastName));

                // Check if the sorted id's are correct
                Assert.AreEqual(c1.Id, loadedList.Contacts[keyC3].Id, "Wrong object Id is not found");
                Assert.AreEqual(c2.Id, loadedList.Contacts[keyC2].Id, "Wrong object Id is not found");
                Assert.AreEqual(c3.Id, loadedList.Contacts[keyC1].Id, "Wrong object Id is not found");

            }
        }
        
    }
}
