using System;
using System.Collections.Generic;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// This class tests the paging functionality
    /// </summary>
    [TestFixture]
    public class PagingTest : ObjectMapperTest
    {
        /// <summary>
        /// Simples the paging.
        /// </summary>
        [Test]
        public void SimplePaging ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                InsertProductsForPaging();

                OrderBy order = new OrderBy(typeof(Product), "ProductName", Ordering.Asc);

                /*
                 * Flat Paging
                 */
                List<Product> site01 = new List<Product>(new ListAdapter<Product>(
                    mapper.FlatPaging(typeof(Product), null, order, 1, 10)));
                Assert.AreEqual(10, site01.Count, "10 records expected");

                List<Product> site02 = new List<Product>(new ListAdapter<Product>(
                    mapper.FlatPaging(typeof(Product), null, order, 10, 19)));
                Assert.AreEqual(10, site02.Count, "10 records expected");

                Assert.AreEqual(site01[9].Id, site02[0].Id, "The second list must begin with the last element of the first paging frame.");

                /*
                 * Flat Distinct Paging
                 */
                site01 = new List<Product>(new ListAdapter<Product>(
                    mapper.FlatDistinctPaging(typeof(Product), null, order, 1, 10)));
                Assert.AreEqual(10, site01.Count, "10 records expected");

                site02 = new List<Product>(new ListAdapter<Product>(
                    mapper.FlatDistinctPaging(typeof(Product), null, order, 10, 19)));
                Assert.AreEqual(10, site02.Count, "10 records expected");

                Assert.AreEqual(site01[9].Id , site02[0].Id, "The second list must begin with the last element of the first paging frame.");
            }
        }

        /// <summary>
        /// Inserts the products for paging.
        /// </summary>
        private void InsertProductsForPaging()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);

                mapper.Delete(typeof(Product));
                mapper.Save(new Product("1", "Chai", DateTime.MinValue));
                mapper.Save(new Product("2", "Chang", DateTime.MinValue));
                mapper.Save(new Product("3", "Aniseed Syrup", DateTime.MinValue));
                mapper.Save(new Product("4", "Chef Anton's Cajun Seasoning", DateTime.MinValue));
                mapper.Save(new Product("5", "Chef Anton's Gumbo Mix", DateTime.MinValue));
                mapper.Save(new Product("6", "Grandma's Boysenberry Spread", DateTime.MinValue));
                mapper.Save(new Product("7", "Uncle Bob's Organic Dried Pears", DateTime.MinValue));
                mapper.Save(new Product("8", "Northwoods Cranberry Sauce", DateTime.MinValue));
                mapper.Save(new Product("9", "Mishi Kobe Niku", DateTime.MinValue));
                mapper.Save(new Product("10","Ikura", DateTime.MinValue));
                mapper.Save(new Product("11","Queso Cabrales", DateTime.MinValue));
                mapper.Save(new Product("12","Queso Manchego La Pastora", DateTime.MinValue));
                mapper.Save(new Product("13","Konbu", DateTime.MinValue));
                mapper.Save(new Product("14","Tofu", DateTime.MinValue));
                mapper.Save(new Product("15","Genen Shouyu", DateTime.MinValue));
                mapper.Save(new Product("16","Pavlova", DateTime.MinValue));
                mapper.Save(new Product("17","Alice Mutton", DateTime.MinValue));
                mapper.Save(new Product("18","Carnarvon Tigers", DateTime.MinValue));
                mapper.Save(new Product("19","Teatime Chocolate Biscuits", DateTime.MinValue));
                mapper.Save(new Product("20","Sir Rodney's Marmalade", DateTime.MinValue));
            
                OBM.Commit(mapper, nested);
            }
        }


    }
}
