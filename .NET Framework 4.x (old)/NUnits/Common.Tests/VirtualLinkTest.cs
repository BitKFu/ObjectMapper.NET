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
    [TestFixture]
    public class VirtualLinkTest : ObjectMapperTest
    {
        /// <summary>
        /// Lookup the company name for employee entities 
        /// </summary>
        [Test]
        public void BackLinkTest ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                Company_With_Employees company = new Company_With_Employees("Fritz IX GmbH & Co Ok.");
                company.Employees.Add(new BackLinkedEmployee("Helge", "Findichfix"));
                company.Employees.Add(new BackLinkedEmployee("Anja", "Tutnichtgut"));
                company.Employees.Add(new BackLinkedEmployee("Pier", "Schuster"));

                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(company);
                OBM.Commit(mapper, nested);

                /*
                 * Now load all employees and check, if the company field has been set
                 */
                List<BackLinkedEmployee> employees =
                    new List<BackLinkedEmployee>(
                        new ListAdapter<BackLinkedEmployee>(mapper.Select(typeof(BackLinkedEmployee))));
                foreach (BackLinkedEmployee employee in employees)
                {
                    Assert.AreEqual(company.LegalName, employee.CompanyName, "Legal name could not be loaded.");
                    Console.WriteLine(
                        string.Concat(employee.FirstName, " ", employee.LastName, " ", employee.CompanyName));
                }
            }
        }
    
        /// <summary>
        /// Links to active products.
        /// </summary>
        [Test]
        [Category("ExcludeForSqlServerCE")]
        public void LinkTestWithGlobalParameters()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                List<DeliveryItem> deliveries = new List<DeliveryItem>();

                /*
                 * Create Test data
                 */
                Product archivedProduct01 = new Product("OBM", "ObjectMapper .NET 1.0", new DateTime(2006, 02, 01));
                Product archivedProduct02 = new Product("OBM", "ObjectMapper .NET 1.1", new DateTime(2006, 06, 01));
                Product activeProduct = new Product("OBM", "ObjectMapper .NET 1.2", DateTime.MinValue);

                DeliveryItem archivedDelivering01 = new DeliveryItem("OBM", 1, new DateTime(2006, 01, 01));
                DeliveryItem archivedDelivering02 = new DeliveryItem("OBM", 1, new DateTime(2006, 04, 01));
                DeliveryItem activeDelivering = new DeliveryItem("OBM", 1, new DateTime(2006, 08, 01));

                /*
                 * Save
                 */
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Save(archivedProduct01);
                mapper.Save(archivedProduct02);
                mapper.Save(activeProduct);

                mapper.Save(archivedDelivering01);
                mapper.Save(archivedDelivering02);
                mapper.Save(activeDelivering);

                OBM.Commit(mapper, nested);

                /* 
                 * Now find the name of the products for all deliveries depending off the delivering date
                 */
                deliveries.Add(archivedDelivering01);
                deliveries.Add(archivedDelivering02);
                deliveries.Add(activeDelivering);

                foreach (DeliveryItem item in deliveries)
                {
                    /*
                     * Get the next valid product date after the delivey data
                     */
                    ICondition validProducts = new ConditionList(
                        new AndCondition(typeof (Product), "ProductKey", QueryOperator.Equals, item.ProductKey),
                        new AndCondition(typeof (Product), "ValidUntil", QueryOperator.GreaterEqual, item.DeliveringDate)
                        );

                    List<Product> products = new List<Product>(
                        new ListAdapter<Product>(
                            mapper.FlatSelect(typeof(Product), validProducts,
                                              new OrderBy(typeof (Product), "ValidUntil", Ordering.Asc))));

                    DateTime searchDate = (products.Count > 0) ? products[0].ValidUntil : DateTime.MinValue;

                    /*
                     * Now load the Delivery and link in to the project, using the search Date, that fitts the best
                     * the delivery date
                     */
                    Hashtable globalJoins = new Hashtable();
                    globalJoins["@VALIDATION_DATE"] = searchDate;

                    DeliveryItem loaded = mapper.FlatLoad(typeof(DeliveryItem), item.Id, globalJoins) as DeliveryItem;
                    Assert.IsNotNull(loaded, "The loaded object must not be null.");
                    Assert.AreEqual(item.Id, loaded.Id, "The id of the loaded item must equal the expected one.");
                    Assert.AreEqual(loaded.ProductName01, loaded.ProductName02);
                    Assert.IsTrue(loaded.ProductName01.Length > 0, "The productname must not be empty.");

                    /*
                     * Output
                     */
                    Console.WriteLine(
                        string.Concat("Product name for delivery date ", loaded.DeliveringDate, " is ",
                                      loaded.ProductName01));
                }
            }
        }
        

    }
}
