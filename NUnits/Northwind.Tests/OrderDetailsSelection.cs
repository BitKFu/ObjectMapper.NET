using System.Linq;
using NUnit.Framework;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.Northwind.Entities;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    [TestFixture]
    public class OrderDetailsSelection : NorthwindBase
    {
        /// <summary>
        /// Mexican Orders
        /// </summary>
        [Test]
        public void SelectMexicanOrders ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var customers = mapper.Query<Customer>();

                IQueryable<Order> allOrders = from order in orders
                                join customer in customers on order.Customer equals customer 
                                where customer.Country == "Mexico"
                                select order;

                ObjectDumper.Write(allOrders);
            }
        }

        /// <summary>
        /// Mexican Orders with Customer Details
        /// </summary>
        [Test]
        public void SelectMexicanOrdersWithCustomerDetails ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var customers = mapper.Query<Customer>();

                var allOrders = from order in orders
                                join customer in customers on order.Customer equals customer
                                where customer.Country == "Mexico"
                                select new { customer.CompanyName, customer.ContactName, order.OrderDate, order.ShipName }
                ;

                ObjectDumper.Write(allOrders);
            }
        }

        /// <summary>
        /// Select company and employee for a special order
        /// </summary>
        [Test]
        public void SelectCompanyAndEmployeeForAnOrder ()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var customers = mapper.Query<Customer>();
                var employees = mapper.Query<Employee>();

               var dataForOrder1 = from order in orders
                                  join customer in customers on order.Customer equals customer
                                  join employee in employees on order.Employee equals employee
                                   where order.OrderID == 10334
                                  select new { order.OrderID, customer.CompanyName, employee.FirstName, employee.LastName, order.OrderDate, order.ShippedDate };

               ObjectDumper.Write(dataForOrder1);

               var  dataForOrder2 = from order in orders
                                   where order.OrderID == 10334 
                                   from customer in customers
                                   where order.Customer == customer 
                                   from employee in employees
                                   where order.Employee == employee
                                   select new { order.OrderID , customer.CompanyName, employee.FirstName, employee.LastName, order.OrderDate, order.ShippedDate };

                ObjectDumper.Write(dataForOrder2);

                Assert.AreEqual(dataForOrder1.Count(), dataForOrder2.Count());
            }
        }
    }
}
