using System;
using System.Collections;
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
    /// <summary>
    /// Linq Test
    /// </summary>
    [TestFixture]
    public class LinqTest : ObjectMapperTest
    {
        /// <summary>
        /// Insert Test Data
        /// </summary>
        private void InsertTestData ()
        {
            /*
             * Insert some test contacts
             */
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(typeof(Contact));
                mapper.Save(new Contact("Fritz", "Bauer"));
                mapper.Save(new Contact("Annemarie", "Aal"));
                mapper.Save(new Contact("Hans", "Habicht"));
                mapper.Save(new Contact("Olaf", "Schubert"));
                OBM.Commit(mapper, nested);
            }
        }

        /// <summary>
        /// Test simple linq conditions
        /// </summary>
        [Test]
        public void TestLinqSimpleConditions ()
        {
            InsertTestData();

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Select objects with WHERE
                 */
                var tableContact = mapper.Query<Contact>();
                var contacts = from contact in tableContact where contact.FirstName == "Fritz" select contact;
                ObjectDumper.Write(contacts);
                Assert.AreEqual(1, contacts.ToList().Count);

                /*
                 * Select objects with AND
                 */
                var contactsAnd = from contact in tableContact where contact.FirstName == "Fritz" && contact.LastName == "Bauer" select contact;
                ObjectDumper.Write(contactsAnd);
                Assert.AreEqual(1, contactsAnd.ToList().Count);

                /*
                 * Select objects with OR
                 */
                var contactsOr = from contact in tableContact where contact.FirstName == "Fritz" || contact.LastName == "Habicht" select contact;
                ObjectDumper.Write(contactsOr);
                Assert.AreEqual(2, contactsOr.ToList().Count);

                /*
                 * Select objects with OR OR OR
                 */
                var contactsOrOr = from contact in tableContact where contact.FirstName == "Fritz" || contact.LastName == "Habicht" || contact.LastName == "Schubert" select contact;
                ObjectDumper.Write(contactsOrOr);
                Assert.AreEqual(3, contactsOrOr.ToList().Count);
            }
        }

        /// <summary>
        /// Tests simple linq binding features
        /// </summary>
        [Test]
        public void TestLinqProjections ()
        {
            InsertTestData();

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Select objects
                 */
                var tableContact = mapper.Query<Contact>();
                var contacts = from contact in tableContact select contact;
                ObjectDumper.Write(contacts);

                /*
                 * Select member
                 */
                var names = from contact in tableContact select new { contact.FirstName };
                ObjectDumper.Write(names);

                /*
                 * Select anonymous types
                 */
                var fullnames = from contact in tableContact select new { contact.FirstName, contact.LastName };
                ObjectDumper.Write(fullnames);

                /*
                 * Select anonymous types with calculation
                 */
                var fullnames2 = from contact in tableContact select new { contact.FirstName, contact.LastName, City = "Bürstadt", Age = 5 * 4 };
                ObjectDumper.Write(fullnames2);
            }
        }
    }
}
