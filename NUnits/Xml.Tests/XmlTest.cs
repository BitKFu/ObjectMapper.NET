using System.IO;
using AdFactum.Data;
using AdFactum.Data.Util;
using AdFactum.Data.Xml;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Xml.Entities;

namespace ObjectMapper.NUnits.Xml.Tests
{
    [TestFixture]
    public class XmlTest
    {
        /// <summary>
        /// Simples the XML binding.
        /// </summary>
        [Test]
        public void SimpleXmlBinding ()
        {
            /*
             * Insert Valid data
             */
            using (AdFactum.Data.ObjectMapper mapper = XmlMapper)
            {
                mapper.BeginTransaction();

                mapper.Save(new Country("DE", "Deutschland", "de-DE"));
                mapper.Save(new Country("DE", "Germany", "en-GB"));

                mapper.Save(new CountryRegion("DE", "BAY", "Bayern", "de-DE"));
                mapper.Save(new CountryRegion("DE", "BAY", "Bavaria", "en-GB"));

                mapper.Save(new CountryRegion("DE", "THU", "Thüringen", "de-DE"));
                mapper.Save(new CountryRegion("DE", "THU", "Thuringia", "en-GB"));

                mapper.Commit();
            }
        }

        /// <summary>
        /// Creates the XML persister.
        /// </summary>
        /// <value>The XML mapper.</value>
        /// <returns></returns>
        public static AdFactum.Data.ObjectMapper XmlMapper
        {
            get
            {
                string file = Directory.GetCurrentDirectory() + "\\XmlTest.xml";
                File.Delete(file);
                var persister = new XmlPersister("XmlTest", file);
                var mapper = new AdFactum.Data.ObjectMapper(new UniversalFactory(), persister, Transactions.Manual);
                return mapper;
            }
        }

        /// <summary>
        /// XMLs the phone book entry.
        /// </summary>
        [Test]
        public void XmlLinkTest ()
        {
            var parent = new XmlLinkParent();
            XmlLinkParent loaded;

            /*
             * Initialize Parent Child List
             */
            parent.ChildList.Add(parent.Child1);
            parent.ChildList.Add(parent.Child2);

            parent.ChildDictionary.Add("A", parent.Child1);
            parent.ChildDictionary.Add("B", parent.Child2);

            /*
             * Insert Valid data
             */
            using (AdFactum.Data.ObjectMapper mapper = XmlMapper)
            {
                mapper.BeginTransaction();
                mapper.Save(parent);
                mapper.Commit();
             
                loaded = (XmlLinkParent) mapper.Load(typeof (XmlLinkParent), parent.Id);
            }

            ObjectDumper.Write(loaded);

            Assert.IsNotNull(loaded, "Loaded is null");
            Assert.AreEqual(parent.Id, loaded.Id, "ID does not equal");
            Assert.AreEqual(parent.Child1.Id, loaded.Child1.Id, "Child 1 does not equal");
            Assert.AreEqual(parent.Child2.Id, loaded.Child2.Id, "Child 2 does not equal");
            Assert.IsTrue(loaded.ChildList.Contains(loaded.Child1));
            Assert.IsTrue(loaded.ChildList.Contains(loaded.Child2));
            Assert.IsTrue(loaded.ChildDictionary.ContainsValue(loaded.Child1));
            Assert.IsTrue(loaded.ChildDictionary.ContainsValue(loaded.Child2));
            Assert.IsTrue(loaded.ChildDictionary.ContainsKey("A"));
            Assert.IsTrue(loaded.ChildDictionary.ContainsKey("B"));
        }
    }
}