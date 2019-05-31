using System.Linq;
using NUnit.Framework;
using AdFactum.Data.Util;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    /// <summary>
    /// Tests the customer selection with linq
    /// </summary>
    [TestFixture]
    public class CustomerSelection : NorthwindBase
    {
        /// <summary>
        /// Select Mexicans
        /// </summary>
        [Test]
        public void SelectMexicans ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var mexicans = from customer in customers where customer.City.StartsWith("México") select customer;

                ObjectDumper.Write(mexicans);
                Assert.Greater(mexicans.Count(), 0, "At least 1 entry has been expected.");
            }
        }

        /// <summary>
        /// Select Mexicans
        /// Refers to (Where Simple 1): http://msdn2.microsoft.com/en-us/vcsharp/aa336760.aspx
        /// /// </summary>
        [Test]
        public void SelectWhereSimple1()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var mexicans = from customer in customers where
                                   customer.Country == "Mexico" || customer.City == "Berlin"
                               select customer;

                ObjectDumper.Write(mexicans);
                Assert.Greater(mexicans.Count(), 0, "At least 1 entry has been expected.");
            }
        }

        /// <summary>
        /// Select Region
        /// Refers to (Where Simple 2): http://msdn2.microsoft.com/en-us/vcsharp/aa336760.aspx
        /// </summary>
        [Test]
        public void SelectWhereSimple2()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var regions = from customer in customers
                               where customer.Region != null
                               select customer;

                ObjectDumper.Write(regions);
                Assert.Greater(regions.Count(), 0, "At least 1 entry has been expected.");
            }
        }

        /// <summary>
        /// Retrieves the customer fullnames
        /// Referes to (Anonymous Types 1) : http://msdn2.microsoft.com/en-us/vcsharp/aa336758.aspx
        /// </summary>
        [Test]
        public void SelectAnonymousTypes1 ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var regions = from customer in customers
                              select new { Company1 = customer.CompanyName };

                ObjectDumper.Write(regions);
                Assert.Greater(regions.Count(), 0, "At least 1 entry has been expected.");
            }
        }

        /// <summary>
        /// Selects the anonymous types1.
        /// </summary>
        [Test]
        public void SelectBasicType()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var regions = from customer in customers
                              select customer.CompanyName;

                ObjectDumper.Write(regions);
                Assert.Greater(regions.Count(), 0, "At least 1 entry has been expected.");
            }
        }


        /// <summary>
        /// Retrieves the first value 
        /// Refers to (First Simple) : http://msdn2.microsoft.com/en-us/vcsharp/aa336750.aspx
        /// </summary>
        [Test]
        public void SelectFirstSimple()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var firstCustomer = (from customer in customers select customer).First();
                
                Assert.IsNotNull(firstCustomer, "One Customer expected");
                ObjectDumper.Write(firstCustomer);
            }
        }

        /// <summary>
        /// Retrieves the first value 
        /// Refers to (First Simple) : http://msdn2.microsoft.com/en-us/vcsharp/aa336750.aspx
        /// </summary>
        [Test]
        public void SelectSingleSimple()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var firstCustomer = (from customer in customers select customer).Single();

                Assert.IsNotNull(firstCustomer, "One Customer expected");
                ObjectDumper.Write(firstCustomer);
            }
        }

        /// <summary>
        /// Retrieves the first 3 values
        /// Refers to (Take - Nested) : http://msdn2.microsoft.com/en-us/vcsharp/aa336757.aspx
        /// </summary>
        [Test]
        public void SelectTakeNested()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var first3Customer = (from customer in customers select customer).Take(3);

                ObjectDumper.Write(first3Customer);
                Assert.AreEqual(3, first3Customer.Count(), "3 entries expected");
            }
        }

        /// <summary>
        /// Skips the first 3 values
        /// Refers to (Skip - Nested) : http://msdn2.microsoft.com/en-us/vcsharp/aa336757.aspx
        /// </summary>
        [Test]
        public void SelectSkipNested()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var skip3Customers = (from customer in customers select customer).Skip(3);

                ObjectDumper.Write(skip3Customers);
                Assert.Greater(skip3Customers.Count(), 0, "At least 1 entry has been expected.");
            }
        }

        /// <summary>
        /// Skips 10 values and takes 5 values
        /// </summary>
        [Test]
        public void SelectPageNested()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var pageCustomers = (from customer in customers select customer).Skip(10).Take(5);

                ObjectDumper.Write(pageCustomers);
                Assert.AreEqual(5, pageCustomers.Count(), "5 entries expected");
            }
        }

        /// <summary>
        /// Count Test
        /// </summary>
        [Test]
        public void CountTest()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var customers = mapper.Query<Customer>();
                var mexicans = from customer in customers where customer.City.StartsWith("México") select customer;

                ObjectDumper.Write(mexicans.Count());
                ObjectDumper.Write(mexicans);
                Assert.Greater(mexicans.Count(),0, "At least 1 entry has been expected.");
            }
        }
    }
}
