using System;
using System.Collections.Generic;
using System.Linq;
using AdFactum.Data.Linq;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.Northwind.Entities;
using AdFactum.Data.Exceptions;
using NUnit.Framework.Legacy;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    [TestFixture]
    public class NorthwindExecutionTests : NorthwindBase
    {
        private const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED_RIGHT_NOW";

        public class ResultClass<T1, T2, T3, T4>
        {
            public T1 A1 { get; private set; }
            public T2 B1 { get; private set; }
            public T3 C1 { get; private set; }
            public T4 D1 { get; set; }

            public ResultClass(T1 a, T2 b, T3 c)
            {
                A1 = a;
                B1 = b;
                C1 = c;
            }

            public ResultClass(T1 a, T2 b, T3 c, T4 d)
            {
                A1 = a;
                B1 = b;
                C1 = c;
                D1 = d;
            }
        }

        public class Northwind
        {
            private AdFactum.Data.ObjectMapper mapper;

            /// <summary>
            /// Initializes a new instance of the <see cref="Northwind"/> class.
            /// </summary>
            /// <param name="mapper">The mapper.</param>
            public Northwind(AdFactum.Data.ObjectMapper mapper)
            {
                this.mapper = mapper;
            }

            /// <summary>
            /// Gets the customers.
            /// </summary>
            /// <value>The customers.</value>
            public IQueryable<Customer> Customers
            {
                get
                {
                    return Mapper.Query<Customer>();
                }
            }

            public IQueryable<Product> Products
            {
                get
                {
                    return Mapper.Query<Product>();
                }
            }

            public IQueryable<Order> Orders
            {
                get
                {
                    return Mapper.Query<Order>();
                }
            }

            public IQueryable<OrderDetail> OrderDetails
            {
                get
                {
                    return Mapper.Query<OrderDetail>();
                }
            }

            public IQueryable<Employee> Employees
            {
                get
                {
                    return Mapper.Query<Employee>();
                }
            }

            public AdFactum.Data.ObjectMapper Mapper
            {
                get { return mapper; }
            }
        }

        private Northwind db;

        [SetUp]
        public void Setup()
        {
#if INTERNAL_DEBUG
            AdFactum.Data.Linq.Expressions.Alias.ResetAliasCounter();
#endif
        }

        [Test]
        public void Test1()
        {
            var dt = new DateTime(1997, 1, 1);
            var query = from order in db.Orders
                        where order.OrderDate < dt
                        select order;   
         
            ObjectDumper.Write(query);
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="NorthwindExecutionTests"/> class.
        /// </summary>
        public NorthwindExecutionTests()
        {
            OBM.CurrentSqlTracer = new Core.ConsoleTracer();
            db = new Northwind(OBM.CreateMapper(Connection));
        }


        [Test]
        public void TestCompoundQueries()
        {
            var childs = from employee in db.Employees
                         from order in GetOrders(1997)
                            where order.Employee == employee 
                            select order;

            ObjectDumper.Write(childs);
        }

        private IQueryable<Order> GetOrders(int yearValue)
        {
            return db.Orders.Where(o => o.CustomerID == "ANTON" && o.ShippedDate.Year == yearValue && o.ShippedDate.Year == yearValue);
        }

        [Test]
        public void TestSqlParameterNames()
        {
            ResultClass<string, int, int, int> parameter = new ResultClass<string, int, int, int>("ANTON", 1997, 0, 0);

            var result = from order in db.Orders
                         where order.CustomerID == parameter.A1 && order.ShippedDate.Year == parameter.B1
                         select order;

            ClassicAssert.AreEqual(5, result.Count());
            ObjectDumper.Write(result);
        }

        [Test]
        public void TestSqlId()
        { 
            var customer = db.Customers.SqlId("TestSqlId").Where(r => r.CustomerID == "ANATR").First();
            ClassicAssert.IsNotNull(customer);
        }

        [Test]
        public void TestSqlIdAndHint()
        { 
            var customer = db.Customers.SqlId("TestSqlId").Hint("FIRST_ROWS").Where(r => r.CustomerID == "ANATR").First();
            ClassicAssert.IsNotNull(customer);
        }

        [Test]
        public void TestSqlIdAndHintPreCompiled()
        {
            const string sqlId = "TestCount";

            var fn = QueryCompiler.Compile(
                    (string id) => db.Customers.SqlId(sqlId).Where(r => r.CustomerID == id).Count());

            ExpressionOverride.Replacements.Insert(sqlId, new SelectReplacement() { SqlId = sqlId, OverrideHint = "FIRST_ROWS"});
            ClassicAssert.AreEqual(1, fn(db.Mapper, "ANATR"));

            var tableName = db.Mapper.Persister.TypeMapper.Quote("Customers");
            var column = db.Mapper.Persister.TypeMapper.Quote("CustomerID");

            ExpressionOverride.Replacements.Insert(sqlId, new SelectReplacement() { SqlId = sqlId, OverrideSql = "SELECT count(*) from " + tableName + " where "+column+"='ANATR';" });
            ClassicAssert.AreEqual(1, fn(db.Mapper, "ANATR"));

            string storedSqlCommand = ((CompiledQuery) fn.Target).StoredSqlCommand;
            Console.WriteLine("OriginalSql: " + storedSqlCommand);
        }

        [Test]
        public void TestUnaryBooleanCompare()
        {
            // Search all Discontinued Products
            var products = from product in db.Products where product.Discontinued || product.Discontinued == true select product;
            ClassicAssert.AreEqual(8, products.Count());

            // Search all Active products
            products = from product in db.Products where !product.Discontinued || product.Discontinued == false select product;
            ClassicAssert.AreEqual(69, products.Count());
        }

        [Test]
        public void TestUnionResultWithSelector()
        {
            List<bool> twoCustomers = db.Customers.Select(r => r.CustomerID == "ANATR").Union(db.Customers.Select(r => r.CustomerID == "ANTON")).ToList();
            ClassicAssert.AreEqual(2, twoCustomers.Count);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupJoinSimpleExample()
        {
            var q = (from c in db.Customers
                     join o in db.Orders on c.CustomerID equals o.CustomerID into g
                     select new { c.CompanyName, Orders = g }).ToList();

            var q1 = (from c in db.Customers
                     select new
                     {
                         c.CompanyName,
                         Orders = (IEnumerable<Order>) (from o in db.Orders
                                                 where c.CustomerID == o.CustomerID
                                                 select o)
                     }).ToList();

            ClassicAssert.AreEqual(q.Count, q1.Count, "Both selects must equal");
            ClassicAssert.IsTrue(q[0].Orders.Count() > 0, "Orders haven't been loaded.");
            ClassicAssert.AreEqual(q[0].Orders.Count(), q1[0].Orders.Count(), "The order count must equal");

            var groupedNames1 = q.GroupBy(x => x.CompanyName, x => x.CompanyName);
            var groupedNames2 = q1.GroupBy(x => x.CompanyName, x => x.CompanyName);

            ClassicAssert.AreEqual(groupedNames1.Count(), groupedNames2.Count());
            ClassicAssert.AreEqual(groupedNames1.Count(), q.Count, "Result of q have not been grouped.");
            ClassicAssert.AreEqual(groupedNames2.Count(), q1.Count, "Result of q1 have not been grouped.");

            foreach (var group in q)
            {
                Console.WriteLine("Customer: {0}", group.CompanyName);
                foreach (var order in group.Orders)
                    Console.WriteLine("  - {0}", order.OrderID);
            }
        }

        /// <summary>
        /// Tests the union with new result object.
        /// </summary>
        [Test]
        public void TestUnionWithNewResultObject()
        {
            var customer = from cus in db.Customers
                           select new CustomerInfo(){Id = cus.CustomerID, Name = cus.CompanyName, Info = "c1"};
          
            var customer1 = from cus1 in db.Customers where cus1.City == "Berlin"
                            from order1 in db.Orders where order1.Customer == cus1
                           select new CustomerInfo(){Id = cus1.CustomerID, Name = cus1.CompanyName, Info = "c2"};

            var customer2 = from cus2 in db.Customers
                            where cus2.City == "México D.F."
                            from order2 in db.Orders where order2.Customer == cus2
                            from details in db.OrderDetails where details.Order == order2
                            select new CustomerInfo(){Id = cus2.CustomerID, Name = cus2.CompanyName, Info = "c3"};

            var result = customer.Union(customer1).Union(customer2);
            result = result.Where(cus => cus.Id == "ANATR" || cus.Id == "ALFKI" || cus.Id == "TORTU");

            var list = result.ToList();
            var lookup = list.ToLookup(x => x.Info, x => x);
            ClassicAssert.AreEqual(3, lookup.Count);
        }

        /// <summary>
        /// Tests the union with different result sets.
        /// </summary>
        [Test]
        public void TestUnionWithDifferentResultSets()
        {
            var customer = from cus in db.Customers select cus;
            
            var customer1 = from cus2 in db.Customers where cus2.City == "Mexico"
                            from order1 in db.Orders where order1.Customer == cus2
                            select cus2;

            var customer2 = from cus1 in db.Customers where cus1.City == "Mexico"
                            from order2 in db.Orders where order2.Customer == cus1
                            from details in db.OrderDetails where details.Order == order2
                            select cus1;

            var twoCustomers = customer.Union(customer1).Union(customer2).Where(r => r.CustomerID == "ANATR" || r.CustomerID == "ANTON").ToList();
            ClassicAssert.AreEqual(2, twoCustomers.Count);
        }

        [Test]
        public void TestUnionResult()
        {
            var twoCustomers = db.Customers.Where(r => r.CustomerID == "ANATR").Concat(db.Customers.Where(r => r.CustomerID == "ANTON")).ToList();
            ClassicAssert.AreEqual(2, twoCustomers.Count);
        }

        [Test]
        public void TestParameterQuery()
        {
            ClassicAssert.IsNotNull(TestParameterQuerySub("ANATR"));
        }

        private Customer TestParameterQuerySub(string customerId)
        {
            return db.Customers.Where(c => c.CustomerID == customerId).SingleOrDefault();
        }

        [Test]
        public void TestNonCompiledQuery()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.Where(c => c.CustomerID == id));
            
            var items = fn(db.Mapper, "ALFKI").ToList();
            ClassicAssert.AreEqual(1, items.Count);

            items = fn(db.Mapper, "ALFKI").ToList();
            ClassicAssert.AreEqual(1, items.Count);
        }

        [Test]
        public void TestCompiledQueryTwoParameter()
        {
            var fn = QueryCompiler.Compile((string id1, string id2) =>
                db.Customers.Where(c => c.CustomerID == id1)
                .Concat(
                db.Customers.Where(c => c.CustomerID == id2)
                ).ToList());

            var items = fn(db.Mapper, "ALFKI", "ALFKI");
            ClassicAssert.AreEqual(2, items.Count);

            items = fn(db.Mapper, "ALFKI", "WHITC");
            ClassicAssert.AreEqual(2, items.Count);
        }

        [Test]
        public void TestCompiledQueryDuplicatedParameter()
        {
            var fn = QueryCompiler.Compile((string id) =>
                db.Customers.Where(c => c.CustomerID == id)
                .Concat(
                db.Customers.Where(c => c.CustomerID == id)
                ).ToList());

            var items = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(2, items.Count);

            items = fn(db.Mapper, "WHITC");
            ClassicAssert.AreEqual(2, items.Count);
        }

        [Test]
        public void TestCompiledQuery()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.Where(c => c.CustomerID == id).ToList());
            var items = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, items.Count);

            items = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, items.Count);
        }

        /// <summary>
        /// Returns a IQueryable object for the customer
        /// </summary>
        /// <param name="db1">The DB1.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static IQueryable<Customer> Customers(Northwind db1, string id)
        {
            return db1.Customers.Where(c => c.CustomerID == id && c.CustomerID == id);
        }

        /// <summary>
        /// Tests the compiled query from method source.
        /// </summary>
        [Test]
        public void TestCompiledQueryFromMethodSource()
        {
            /*
             * That's a special case! Be careful!
             * 
             * Because the method call to Customers is not compilable with abstract variables,
             * we have to insert a default value like string.Empty and name the parameter like the
             * parameter in the submethod Customers is named.
             * 
             * Otherwise the paramter mapping in the SQL will fail.
             */
            var fn = QueryCompiler.Compile((string id) => Customers(db, string.Empty /* id */).ToList());
            var items = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, items.Count);

            items = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, items.Count);
        }

        [Test]
        public void TestCompiledQuerySingleton()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.SingleOrDefault(c => c.CustomerID == id));
            Customer cust = fn(db.Mapper, "ALFKI");
            ClassicAssert.IsNotNull(cust);

            cust = fn(db.Mapper, "ALFKI");
            ClassicAssert.IsNotNull(cust);
        }

        [Test] public void TestCompiledContainsQuery()
        {
            var fn = QueryCompiler.Compile((List<string> ids) => db.Customers.Where(e => ids.Contains(e.CustomerID)).ToList());
            List<Customer> cust = fn(db.Mapper, new List<string>() { "ALFKI", "WOLZA" });
            ClassicAssert.AreEqual(2, cust.Count());

            var fn1 = QueryCompiler.Compile((List<string> ids) => db.Customers.Where(e => ids.Contains(e.CustomerID)));
            List<Customer> cust1 = fn1(db.Mapper, new List<string>() { "ALFKI", "WOLZA" }).ToList();
            ClassicAssert.AreEqual(2, cust1.Count());
        }

        [Test] public void TestCompiledQueryCount()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.Count(c => c.CustomerID == id));
            int n = fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, n);

            fn(db.Mapper, "ALFKI");
            ClassicAssert.AreEqual(1, n);
        }

        //[Test] public void TestCompiledQueryIsolated()
        //{
        //    var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id));
        //    var items = fn(db.Mapper, this.db, "ALFKI").ToList();
        //}

        //[Test] public void TestCompiledQueryIsolatedWithHeirarchy()
        //{
        //    var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id).Select(c => n.Orders.Where(o => o.CustomerID == c.CustomerID)));
        //    var items = fn(db.Mapper, this.db, "ALFKI").ToList();
        //}

        [Test] public void TestWhere()
        {
            var list = db.Customers.Where(c => c.City == "London").ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Test] public void TestWhereTrue()
        {
            var list = db.Customers.Where(c => true).ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestCompareEntityEqual()
        {
            Customer alfki = new Customer { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c == alfki).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual("ALFKI", list[0].CustomerID);
        }

        [Test] public void TestCompareEntityNotEqual()
        {
            Customer alfki = new Customer { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c != alfki).ToList();
            ClassicAssert.AreEqual(90, list.Count);
        }

        [Test] public void TestCompareConstructedEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City } == new { x = "London" }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Test] public void TestCompareConstructedMultiValueEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Test] public void TestCompareConstructedMultiValueNotEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }).ToList();
            ClassicAssert.AreEqual(84, list.Count);
        }

        [Test] public void TestSelectScalar()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => c.City).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual("London", list[0]);
            ClassicAssert.IsTrue(list.All(x => x == "London"));
        }

        [Test] public void TestSelectAnonymousOne()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual("London", list[0].City);
            ClassicAssert.IsTrue(list.All(x => x.City == "London"));
        }

        [Test] public void TestSelectAnonymousTwo()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c.Phone }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual("London", list[0].City);
            ClassicAssert.IsTrue(list.All(x => x.City == "London"));
            ClassicAssert.IsTrue(list.All(x => x.Phone != null));
        }

        [Test] public void TestSelectCustomerTable()
        {
            var list = db.Customers.ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestSelectAnonymousWithObject()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { city1=c.City, c }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual("London", list[0].city1);
            ClassicAssert.IsTrue(list.All(x => x.city1 == "London"));
            ClassicAssert.IsTrue(list.All(x => x.c.City == x.city1));
        }

        [Test] public void TestAssociationHorror()
        {
            var companyNames = (
                from od in db.OrderDetails
                from order in db.Orders
                where od.Product.ProductName == "Lakkalikööri"
                      && od.Order == order
                select order.Customer.CompanyName).ToList();
        }

        [Test] public void TestSelectAnonymousLiteral()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { X = 10 }).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.IsTrue(list.All(x => x.X == 10));
        }

        [Test] public void TestSelectConstantInt()
        {
            var list = db.Customers.Select(c => 10).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(list.All(x => x == 10));
        }

        [Test] public void TestSelectConstantNullString()
        {
            var list = db.Customers.Select(c => (string)null).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(list.All(x => x == null));
        }

        [Test] public void TestSelectLocal()
        {
            int x = 10;
            var list = db.Customers.Select(c => x).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(list.All(y => y == 10));
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestSelectNestedCollection()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "ALFKI"
                           select db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID)
                       ).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Count());
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestSelectNestedCollectionInAnonymousType()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "ALFKI"
                           select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList() }
                       ).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Foos.Count);
        }

        [Test] public void TestJoinCustomerOrders()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "ALFKI"
                           join o in db.Orders on c.CustomerID equals o.CustomerID
                           select new { c.ContactName, o.OrderID }
                       ).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Test] public void TestJoinMultiKey()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "ALFKI"
                           join o in db.Orders on new { a = c.CustomerID, b = c.CustomerID } equals new { a = o.CustomerID, b = o.CustomerID }
                           select new { c, o }
                       ).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Category("ExcludeForOracle")]  // the outcoming SQL is correct, but oracle does not like it.
        [Test] public void TestJoinIntoCustomersOrdersCount()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "ALFKI"
                           join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                           select new { cust = c, ords = ords.Count() }
                       ).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].ords);
        }

        [Test] public void TestJoinIntoDefaultIfEmpty()
        {
            var list = (
                           from c in db.Customers
                           where c.CustomerID == "PARIS"
                           join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                           from o in ords.DefaultIfEmpty()
                           select new { c, o }
                       ).ToList(); 

            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(null, list[0].o);
        }

        [Test] public void TestMultipleJoinsWithJoinConditionsInWhere()
        {
            // this should reduce to inner joins
            //ClassicAssert.Fail("FAILES");
            var list = (
                           from c in db.Customers
                           from o in db.Orders
                           from d in db.OrderDetails
                           where o.CustomerID == c.CustomerID && o.OrderID == d.OrderID
                           where c.CustomerID == "ALFKI"
                           select d
                       ).ToList();

            ClassicAssert.AreEqual(12, list.Count);
        }

        [Test] public void TestMultipleJoinsWithMissingJoinCondition()
        {
            //ClassicAssert.Fail("FAILES");
            // this should force a naked cross join
            var list = (
                           from c in db.Customers
                           from o in db.Orders
                           from d in db.OrderDetails
                           where o.CustomerID == c.CustomerID /*&& o.OrderID == d.OrderID*/
                           where c.CustomerID == "ALFKI"
                           select d
                       ).ToList();

            ClassicAssert.AreEqual(12930, list.Count);
        }

        [Test] public void TestOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestOrderByOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.Phone).OrderBy(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestOrderByThenBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestOrderByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestOrderByDescendingThenBy()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestOrderByDescendingThenByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestOrderByJoin()
        {
            var list = (
                           from c in db.Customers.OrderBy(c => c.CustomerID)
                           join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                           select new { c.CustomerID, o.OrderID }
                       ).ToList();

            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestOrderBySelectMany()
        {
            var list = (
                           from c in db.Customers.OrderBy(c => c.CustomerID)
                           from o in db.Orders.OrderBy(o => o.OrderID)
                           where c.CustomerID == o.CustomerID
                           select new { c.CustomerID, o.OrderID }
                       ).ToList();
            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID).ToList();
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestGroupBy()
        {
            var list = db.Customers.GroupBy(c => c.City).ToList();
            ClassicAssert.AreEqual(69, list.Count);
        }

        [Test] public void TestGroupByOne()
        {
            var list = db.Customers.Where(c => c.City == "London").GroupBy(c => c.City).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Count());
        }

        [Test] public void TestGroupBySelectMany()
        {
            var list = db.Customers.GroupBy(c => c.City).SelectMany(g => g).ToList();
            ClassicAssert.AreEqual(91, list.Count);

            var list2 = db.Customers.Where(c => c.City == "London").GroupBy(c => c.City).SelectMany(g => g).ToList();
            ClassicAssert.AreEqual(6, list2.Count);
        }

        [Test] public void TestGroupBySum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0]);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByMax()
        {
            List<int?> list = (from o in db.Orders group o by o.CustomerID into g select g.Max(o => o.OrderID)).ToList();

            ClassicAssert.AreEqual(89, list.Count);
            ClassicAssert.IsTrue(new List<int?>() { 11011, 11064 }.Contains(list[0]));
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByMaxIntoNew()
        {
            var list = (from o in db.Orders group o by o.CustomerID into g select new { CustomerId = g.Key, MaxOrderId = g.Max(o => o.OrderID) }).ToList();

            ClassicAssert.AreEqual(89, list.Count);
            ClassicAssert.IsTrue(new List<int?>() { 11011, 11064 }.Contains(list[0].MaxOrderId));
        }

        [Test]
        public void TestGroupByCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Count()).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0]);
        }

        [Test] public void TestGroupByLongCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.LongCount()).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6L, list[0]);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupBySumMinMaxAvg()
        {
            var list = 
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
                    new
                        {
                            Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }).ToList();

            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Sum);
        }

        [Test]
        public void TestCreateVOWithConstructor()
        {
            var customer = db.Customers.Select(c => new Customer(c.CustomerID, c.CompanyName)).First();
            ClassicAssert.IsNotNull(customer);
            ClassicAssert.IsNotNull(customer.Id);
            ClassicAssert.IsNotNull(customer.CompanyName);
            ClassicAssert.IsNull(customer.City);
        }

        [Test]
        public void TestCreateVOWithMembers()
        {
            var customer = db.Customers.Select(c => 
                new Customer{CustomerID = c.CustomerID, CompanyName = c.CompanyName}).First();

            ClassicAssert.IsNotNull(customer);
            ClassicAssert.IsNotNull(customer.Id);
            ClassicAssert.IsNotNull(customer.CompanyName);
            ClassicAssert.IsNull(customer.City);
        }

        [Test]
        public void TestCreateVOWithComplexMembers()
        {
            var customer = db.Orders.Select(c =>
                new
                    {
                        CustomerID = c.Customer.CustomerID,
                        FirstName = c.Employee.FirstName,
                    }).First();

            ClassicAssert.IsNotNull(customer);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupBySumMinMaxAvgNewObject1()
        {

            var list =
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
                    new ResultClass<int?, int?, int?, double?>(
                        g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        g.Min(o => o.OrderID),
                        g.Max(o => o.OrderID),
                        g.Average(o => o.OrderID))
                    ).ToList();

            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].A1);
            ClassicAssert.IsNotNull(list[0].D1);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupBySumMinMaxAvgNewObject2()
        {

            var list =
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
                    new ResultClass<int?,int?,int?,double?>(
                        g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        g.Min(o => o.OrderID),
                        g.Max(o => o.OrderID)) {D1 = g.Average(o => o.OrderID)}
                    ).ToList();

            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].A1);
            ClassicAssert.IsNotNull(list[0].D1);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithResultSelector()
        {
            var list = 
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, (k, g) =>
                                                                                         new
                                                                                             {
                                                                                                 Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                                                                                                 Min = g.Min(o => o.OrderID),
                                                                                                 Max = g.Max(o => o.OrderID),
                                                                                                 Avg = g.Average(o => o.OrderID)
                                                                                             }).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Sum);           
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithElementSelectorSum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => g.Sum()).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0]);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Count());
            ClassicAssert.AreEqual(6, list[0].Sum());
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithElementSelectorSumMax()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => new { Sum = g.Sum(), Max = g.Max() }).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0].Sum);
            ClassicAssert.AreEqual(1, list[0].Max);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithAnonymousElement()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => new { X = (o.CustomerID == "ALFKI" ? 1 : 1) }).Select(g => g.Sum(x => x.X)).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(6, list[0]);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestGroupByWithTwoPartKey()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).ToList();
            ClassicAssert.AreEqual(1, list.Count);
            var grp = list[0].ToList();
            var sorted = grp.OrderBy(o => o.OrderID);
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(grp, sorted));
        }

        [Test] public void TestOrderByGroupBySelectMany()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g).ToList();
            ClassicAssert.AreEqual(6, list.Count);
            var sorted = list.OrderBy(o => o.OrderID).ToList();
            ClassicAssert.IsTrue(Enumerable.SequenceEqual(list, sorted));
        }

        [Test] public void TestSumWithNoArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => (o.CustomerID == "ALFKI" ? 1 : 1)).Sum();
            ClassicAssert.AreEqual(6, sum);
        }

        [Test] public void TestSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            ClassicAssert.AreEqual(6, sum);
        }

        [Test] public void TestCountWithNoPredicate()
        {
            var cnt = db.Orders.Count();
            ClassicAssert.AreEqual(830, cnt);
        }

        [Test] public void TestCountWithPredicate()
        {
            var cnt = db.Orders.Count(o => o.CustomerID == "ALFKI");
            ClassicAssert.AreEqual(6, cnt);
        }

        [Test] public void TestDistinctNoDupes()
        {
            var list = db.Customers.Distinct().ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestDistinctScalar()
        {
            var list = db.Customers.Select(c => c.City).Distinct().ToList();
            ClassicAssert.AreEqual(69, list.Count);
        }

        [Test] public void TestOrderByDistinct()
        {
            var list = db.Customers.Where(c => c.City.StartsWith("P")).OrderBy(c => c.City).Select(c => c.City).Distinct().ToList();
            var sorted = list.OrderBy(x => x).ToList();
            ClassicAssert.AreEqual(list[0], sorted[0]);
            ClassicAssert.AreEqual(list[list.Count - 1], sorted[list.Count - 1]);
        }

        [Test] public void TestDistinctOrderBy()
        {
            var list = db.Customers.Where(c => c.City.StartsWith("P")).Select(c => c.City).Distinct().OrderBy(c => c).ToList();
            var sorted = list.OrderBy(x => x).ToList();
            ClassicAssert.AreEqual(list[0], sorted[0]);
            ClassicAssert.AreEqual(list[list.Count - 1], sorted[list.Count - 1]);
        }

        [Test] public void TestDistinctGroupBy()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().GroupBy(o => o.CustomerID).ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestGroupByDistinct()
        {
            // distinct after group-by should not do anything
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Distinct().ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestDistinctCount()
        {
            var cnt = db.Customers.Distinct().Count();
            ClassicAssert.AreEqual(91, cnt);
        }

        [Test] public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            var cnt = db.Customers.Select(c => c.City).Distinct().Count();
            ClassicAssert.AreEqual(69, cnt);
        }

        [Test] public void TestCartesianProduct()
        {
            var product = from cus1 in db.Customers
                          from cus2 in db.Customers
                          select new { name1 = cus1.CompanyName, name2 = cus2.CompanyName };

            int count = product.Count();
            ClassicAssert.AreEqual(8281, count);
        }

        [Test] public void TestComplexCartesianProduct()
        {
            var product = from cus1 in db.Customers
                          from cus2 in db.Customers
                          select new { cus1, cus2 };

            var cartesian = product.ToList();
            ClassicAssert.AreNotEqual(cartesian[1].cus1.CustomerID, cartesian[1].cus2.CustomerID);
            ClassicAssert.AreEqual(8281, cartesian.Count);
        }

        [Test] public void TestSelectSelectDistinctCount()
        {
            var cnt = db.Customers.Select(c => c.City).Select(c => c).Distinct().Count();
            ClassicAssert.AreEqual(69, cnt);
        }

        [Test] public void TestDistinctCountPredicate()
        {
            var cnt = db.Customers.Select(c => new {c.City, c.Country}).Distinct().Count(c => c.City == "London");
            ClassicAssert.AreEqual(1, cnt);
        }

        [Test] public void TestDistinctSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            ClassicAssert.AreEqual(6, sum);
        }

        [Test] public void TestSelectDistinctSum()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => o.OrderID).Distinct().Sum();
            ClassicAssert.AreEqual(64835, sum);
        }

        [Test] public void TestTake()
        {
            var list = db.Orders.Take(5).ToList();
            ClassicAssert.AreEqual(5, list.Count);
        }

        [Test] public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Distinct().Take(5).ToList();
            ClassicAssert.AreEqual(5, list.Count);
        }

        [Test] public void TestDistinctTakeCount()
        {
            var cnt = db.Orders.Distinct().OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Count();
            ClassicAssert.AreEqual(5, cnt);
        }

        [Test] public void TestTakeDistinctCount()
        {
            var cnt = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().Count();
            ClassicAssert.AreEqual(1, cnt);
        }

        [Test] public void TestFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First();
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("ROMEY", first.CustomerID);
        }

        [Test] public void TestFirstPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First(c => c.City == "London");
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("EASTC", first.CustomerID);
        }

        [Test] public void TestWhereFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").First();
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("EASTC", first.CustomerID);
        }

        [Test] public void TestFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault();
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("ROMEY", first.CustomerID);
        }

        [Test] public void TestFirstOrDefaultPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London");
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("EASTC", first.CustomerID);
        }

        [Test] public void TestWhereFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault();
            ClassicAssert.AreNotEqual(null, first);
            ClassicAssert.AreEqual("EASTC", first.CustomerID);
        }

        [Test] public void TestFirstOrDefaultPredicateNoMatch()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "SpongeBob");
            ClassicAssert.AreEqual(null, first);
        }

        [Test] public void TestReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.AreEqual("WOLZA", list[0].CustomerID);
            ClassicAssert.AreEqual("ROMEY", list[90].CustomerID);
        }

        [Test] public void TestReverseReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Reverse().ToList();
            ClassicAssert.AreEqual(91, list.Count);
            ClassicAssert.AreEqual("ROMEY", list[0].CustomerID);
            ClassicAssert.AreEqual("WOLZA", list[90].CustomerID);
        }

        [Test] public void TestReverseWhereReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Reverse().ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual("EASTC", list[0].CustomerID);
            ClassicAssert.AreEqual("BSBEV", list[5].CustomerID);
        }

        [Test] public void TestReverseTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Take(5).Reverse().ToList();
            ClassicAssert.AreEqual(5, list.Count);
            ClassicAssert.AreEqual("CHOPS", list[0].CustomerID);
            ClassicAssert.AreEqual("WOLZA", list[4].CustomerID);
        }

        [Test] public void TestReverseWhereTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Take(5).Reverse().ToList();
            ClassicAssert.AreEqual(5, list.Count);
            ClassicAssert.AreEqual("CONSH", list[0].CustomerID);
            ClassicAssert.AreEqual("BSBEV", list[4].CustomerID);
        }

        [Test] public void TestLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last();
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("WOLZA", last.CustomerID);
        }

        [Test] public void TestLastPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last(c => c.City == "London");
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("BSBEV", last.CustomerID);
        }

        [Test] public void TestWhereLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last();
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("BSBEV", last.CustomerID);
        }

        [Test] public void TestLastOrDefault()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault();
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("WOLZA", last.CustomerID);
        }

        [Test] public void TestLastOrDefaultPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London");
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("BSBEV", last.CustomerID);
        }

        [Test] public void TestWhereLastOrDefault()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault();
            ClassicAssert.AreNotEqual(null, last);
            ClassicAssert.AreEqual("BSBEV", last.CustomerID);
        }

        [Test] public void TestLastOrDefaultNoMatches()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "SpongeBob");
            ClassicAssert.AreEqual(null, last);
        }

        [Test] public void TestSingleFails()
        {
            var single = db.Customers.Single();

            try
            {
                db.Customers.Single(c => c.CustomerID == "SpongeBob");
                ClassicAssert.Fail("Need to throw an expcetion");
            }
            catch(NoDataFoundException)
            {}
        }

        [Test] public void TestSinglePredicate()
        {
            var single = db.Customers.Single(c => c.CustomerID == "ALFKI");
            ClassicAssert.AreNotEqual(null, single);
            ClassicAssert.AreEqual("ALFKI", single.CustomerID);
        }

        [Test] public void TestWhereSingle()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").Single();
            ClassicAssert.AreNotEqual(null, single);
            ClassicAssert.AreEqual("ALFKI", single.CustomerID);
        }

        [Test] public void TestSingleOrDefaultFails()
        {
            var single = db.Customers.SingleOrDefault();
        }

        [Test] public void TestSingleOrDefaultPredicate()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI");
            ClassicAssert.AreNotEqual(null, single);
            ClassicAssert.AreEqual("ALFKI", single.CustomerID);
        }

        [Test] public void TestWhereSingleOrDefault()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault();
            ClassicAssert.AreNotEqual(null, single);
            ClassicAssert.AreEqual("ALFKI", single.CustomerID);
        }

        [Test] public void TestSingleOrDefaultNoMatches()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "SpongeBob");
            ClassicAssert.AreEqual(null, single);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestAnyTopLevel()
        {
            var any = db.Customers.Any();
            ClassicAssert.IsTrue(any);
        }

        //[Test] public void TestAnyWithSubquery()
        //{
        //    var list = db.Customers.Where(c => c.Orders.Any(o => o.CustomerID == "ALFKI")).ToList();
        //    ClassicAssert.AreEqual(1, list.Count);
        //}

        [Category("ExcludeForAccess")]
        [Test]
        public void TestAnyWithSubqueryNoPredicate()
        {
            // customers with at least one order
            var list = db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any()).ToList();
            ClassicAssert.AreEqual(89, list.Count);
        }

        [Test] public void TestAnyWithLocalCollection()
        {
            // get customers for any one of these IDs
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Any(id => c.CustomerID == id)).ToList();
            ClassicAssert.AreEqual(2, list.Count);
        }

        //[Test] public void TestAllWithSubquery()
        //{
        //    var list = db.Customers.Where(c => c.Orders.All(o => o.CustomerID == "ALFKI")).ToList();
        //    // includes customers w/ no orders
        //    ClassicAssert.AreEqual(3, list.Count);
        //}

        [Test] public void TestAllWithLocalCollection()
        {
            // get all customers with a name that contains both 'm' and 'd'  (don't use vowels since these often depend on collation)
            string[] patterns = new[] { "m", "d" };

            var list = db.Customers.Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();
            var local = db.Customers.AsEnumerable().Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();

            ClassicAssert.AreEqual(local.Count, list.Count);
        }

        [Category("ExcludeForAccess")]
        [Test] public void TestAllTopLevel()
        {
            // all customers have name length > 0?
            var all = db.Customers.All(c => c.ContactName.Length > 0);
            ClassicAssert.IsTrue(all);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestAllTopLevelNoMatches()
        {
            // all customers have name with 'a'
            var all = db.Customers.All(c => c.ContactName.Contains("a"));
            ClassicAssert.IsFalse(all);
        }

        [Test] public void TestContainsWithSubquery()
        {
            // this is the long-way to determine all customers that have at least one order
            var list = db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID)).ToList();
            ClassicAssert.AreEqual(89, list.Count);
        }

        [Test]
        public void TestNotContainsWithSubquery()
        {
            // this is the long-way to determine all customers that have at least one order
            var list = db.Customers.Where(c => !db.Orders.Where(o=>o.CustomerID==c.CustomerID).Select(o => o.CustomerID).Contains(c.CustomerID)).Where(c=>c.City=="Mexico").ToList();
            ClassicAssert.AreEqual(0, list.Count);
        }

        [Test]
        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Contains(c.CustomerID)).ToList();
            ClassicAssert.AreEqual(2, list.Count);
        }

        [Test] public void TestContainsTopLevel()
        {
            var contains = db.Customers.Select(c => c.CustomerID).Contains("ALFKI");
            ClassicAssert.IsTrue(contains);
        }

        [Test] public void TestSkipTake()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Skip(5).Take(10).ToList();
            ClassicAssert.AreEqual(10, list.Count);
            ClassicAssert.AreEqual("BLAUS", list[0].CustomerID);
            ClassicAssert.AreEqual("COMMI", list[9].CustomerID);
        }

        [Test] public void TestDistinctSkipTake()
        {
            var list = db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).Skip(5).Take(10).ToList();
            ClassicAssert.AreEqual(10, list.Count);
            var hs = new HashSet<string>(list);
            ClassicAssert.AreEqual(10, hs.Count);
        }

        [Test] public void TestCoalesce()
        {
            var list = db.Customers.Select(c => new { City2 = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                .Where(x => (x.City2 ?? "NoCity") == "NoCity").ToList();
            ClassicAssert.AreEqual(6, list.Count);
            ClassicAssert.AreEqual(null, list[0].City2);
        }

        [Test] public void TestCoalesce2()
        {
            var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                .Where(x => (x.City ?? x.Country ?? "NoCityOrCountry") == "NoCityOrCountry").ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual(null, list[0].City);
            ClassicAssert.AreEqual(null, list[0].Country);
        }

        // framework function tests

        [Test] public void TestStringLength()
        {
            var list = db.Customers.Where(c => c.City.Length == 7).ToList();
            ClassicAssert.AreEqual(9, list.Count);
        }

        [Test] public void TestStringStartsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith("M")).ToList();
            ClassicAssert.AreEqual(12, list.Count);    
        }

        [Test] public void TestStringStartsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)).ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestStringEndsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith("s")).ToList();
            ClassicAssert.AreEqual(9, list.Count);
        }

        [Test] public void TestStringEndsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)).ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestStringContainsLiteral()
        {
            var local = db.Customers.ToList().Where(c => c.ContactName.ToLower().Contains("nd")).Select(c => c.ContactName).ToList();
            var list = db.Customers.Where(c => c.ContactName.Contains("nd")).Select(c => c.ContactName).ToList();
            ClassicAssert.AreEqual(local.Count, list.Count);
        }

        [Test] public void TestStringContainsColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.Contains(c.ContactName)).ToList();
            ClassicAssert.AreEqual(91, list.Count);
        }

        [Test] public void TestStringConcatInSelect()
        {
            var name = db.Customers.Where(c => c.ContactName == "Maria Anders").Select(c => c.CompanyName + " " + c.ContactName).Single();
            ClassicAssert.AreEqual("Alfreds Futterkiste Maria Anders", name);
        }

        [Test] public void TestStringConcatImplicit2Args()
        {
            var list = db.Customers.Where(c => c.ContactName + "X" == "Maria AndersX").ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestStringConcatExplicit2Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X") == "Maria AndersX").ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestStringConcatExplicit3Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "Maria AndersXGermany").ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestStringConcatExplicitNArgs()
        {
            var list = db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "Maria AndersXGermany").ToList();
            ClassicAssert.AreEqual(1, list.Count);
        }

        [Test] public void TestStringIsNullOrEmpty()
        {
            var selection = db.Customers.Select(c => c.City == "London" ? null : c.CustomerID);
            var list = selection.Where(x => string.IsNullOrEmpty(x)).ToList();
            ClassicAssert.AreEqual(6, list.Count);
        }

        [Test] public void TestStringToUpper()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "abc" : "abc").ToUpper());
            ClassicAssert.AreEqual("ABC", str);
        }

        [Test] public void TestStringToLower()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "ABC" : "ABC").ToLower());
            ClassicAssert.AreEqual("abc", str);
        }

        [Test] public void TestStringSubstring()
        {
            var list = db.Customers.Where(c => c.City.Substring(0, 4) == "Seat").ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual("Seattle", list[0].City);
        }

        [Test] public void TestStringSubstringNoLength()
        {
            var list = db.Customers.Where(c => c.City.Substring(4) == "tle").ToList();
            ClassicAssert.AreEqual(1, list.Count);
            ClassicAssert.AreEqual("Seattle", list[0].City);
        }

        [Test] public void TestStringIndexOf()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("ar"));
            ClassicAssert.AreEqual(1, n);
        }

        [Test] public void TestStringIndexOfChar()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf('r'));
            ClassicAssert.AreEqual(2, n);
        }

        [Test] public void TestStringIndexOfWithStart()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("a", 3));
            ClassicAssert.AreEqual(4, n);
        }

        [Test] public void TestStringTrim()
        {
            var notrim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " "));
            var trim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " ").Trim());
            ClassicAssert.AreNotEqual(notrim, trim);
            ClassicAssert.AreEqual(notrim.Trim(), trim);
        }

        [Test] public void TestDateTimeConstructYMD()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4));
            ClassicAssert.AreEqual(1997, dt.Year);
            ClassicAssert.AreEqual(7, dt.Month);
            ClassicAssert.AreEqual(4, dt.Day);
            ClassicAssert.AreEqual(0, dt.Hour);
            ClassicAssert.AreEqual(0, dt.Minute);
            ClassicAssert.AreEqual(0, dt.Second);
        }

        [Test] public void TestDateTimeConstructYMDHMS()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6));
            ClassicAssert.AreEqual(1997, dt.Year);
            ClassicAssert.AreEqual(7, dt.Month);
            ClassicAssert.AreEqual(4, dt.Day);
            ClassicAssert.AreEqual(3, dt.Hour);
            ClassicAssert.AreEqual(5, dt.Minute);
            ClassicAssert.AreEqual(6, dt.Second);
        }

        [Test] public void TestDateTimeDay()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Day);
            ClassicAssert.AreEqual(25, v);
        }

        [Test] public void TestDateTimeMonth()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Month);
            ClassicAssert.AreEqual(8, v);
        }

        [Test] public void TestDateTimeYear()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Year);
            ClassicAssert.AreEqual(1997, v);
        }


        /// <summary>
        /// Tests the date time now.
        /// </summary>
        [Test] public void TestDateTimeNow()
        {
            var v = db.Orders.Where(o => o.OrderDate == DateTime.Now);
            ClassicAssert.AreEqual(0, v.Count());
        }

        /// <summary>
        /// Tests the date time now.
        /// </summary>
        [Test]
        public void TestDateTimeToday()
        {
            var v = db.Orders.Where(o => o.OrderDate == DateTime.Today);
            ClassicAssert.AreEqual(0, v.Count());
        }

        [Test] public void TestDateTimeHour()
        {
            var hour = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Hour);
            ClassicAssert.AreEqual(3, hour);
        }

        [Test] public void TestDateTimeMinute()
        {
            var minute = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Minute);
            ClassicAssert.AreEqual(5, minute);
        }

        [Test] public void TestDateTimeSecond()
        {
            var second = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Second);
            ClassicAssert.AreEqual(6, second);
        }

        [Test] public void TestDateTimeDayOfWeek()
        {
            var dow = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.DayOfWeek);
            ClassicAssert.AreEqual(DayOfWeek.Monday, dow);
        }

        [Test] public void TestDateTimeAddYears()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddYears(2).Year == 1999);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestDateTimeAddMonths()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMonths(2).Month == 10);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestDateTimeAddDays()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddDays(2).Day == 27);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestDateTimeAddHours()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddHours(3).Hour == 3);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestDateTimeAddMinutes()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMinutes(5).Minute == 5);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestDateTimeAddSeconds()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddSeconds(6).Second == 6);
            ClassicAssert.AreNotEqual(null, od);
        }

        [Test] public void TestMathAbs()
        {
            var neg1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? -1 : 0));
            var pos1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? 1 : 0));
            ClassicAssert.AreEqual(Math.Abs(-1), neg1);
            ClassicAssert.AreEqual(Math.Abs(1), pos1);
        }

        [Test] public void TestMathAtan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 5.0 : 5.0));
            ClassicAssert.AreEqual(Math.Atan(0.0), zero, 0.0001);
            ClassicAssert.AreEqual(Math.Atan(5.0), one, 0.0001);
        }

        [Test] public void TestMathCos()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            ClassicAssert.AreEqual(Math.Cos(0.0), zero, 0.0001);
            ClassicAssert.AreEqual(Math.Cos(Math.PI), pi, 0.0001);
        }

        [Test] public void TestMathSin()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            var pi2 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin(((c.CustomerID == "ALFKI") ? Math.PI : Math.PI)/2.0));
            ClassicAssert.AreEqual(Math.Sin(0.0), zero);
            ClassicAssert.AreEqual(Math.Sin(Math.PI), pi, 0.0001);
            ClassicAssert.AreEqual(Math.Sin(Math.PI/2.0), pi2, 0.0001);
        }

        [Test] public void TestMathTan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            ClassicAssert.AreEqual(Math.Tan(0.0), zero, 0.0001);
            ClassicAssert.AreEqual(Math.Tan(Math.PI), pi, 0.0001);
        }

        [Test] public void TestMathExp()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 2.0 : 2.0));
            ClassicAssert.AreEqual(Math.Exp(0.0), zero, 0.0001);
            ClassicAssert.AreEqual(Math.Exp(1.0), one, 0.0001);
            ClassicAssert.AreEqual(Math.Exp(2.0), two, 0.0001);
        }

        [Test] public void TestMathLog()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var e = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? Math.E : Math.E));
            ClassicAssert.AreEqual(Math.Log(1.0), one, 0.0001);
            ClassicAssert.AreEqual(Math.Log(Math.E), e, 0.0001);
        }

        [Test] public void TestMathSqrt()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 4.0 : 4.0));
            var nine = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 9.0 : 9.0));
            ClassicAssert.AreEqual(1.0, one);
            ClassicAssert.AreEqual(2.0, four);
            ClassicAssert.AreEqual(3.0, nine);
        }

        [Test] public void TestMathPow()
        {
            // 2^n
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 2.0));
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 3.0));
            ClassicAssert.AreEqual(1.0, zero);
            ClassicAssert.AreEqual(2.0, one);
            ClassicAssert.AreEqual(4.0, two);
            ClassicAssert.AreEqual(8.0, three);
        }

        [Test] public void TestMathRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            ClassicAssert.AreEqual(3.0, four);
            ClassicAssert.AreEqual(4.0, six);
        }

        [Test] public void TestMathFloor()
        {
            // The difference between floor and truncate is how negatives are handled.  Floor drops the decimals and moves the
            // value to the more negative, so Floor(-3.4) is -4.0 and Floor(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.4 : 3.4)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.6 : 3.6)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? -3.4 : -3.4)));
            ClassicAssert.AreEqual(Math.Floor(3.4), four);
            ClassicAssert.AreEqual(Math.Floor(3.6), six);
            ClassicAssert.AreEqual(Math.Floor(-3.4), nfour);
        }

        [Test] public void TestDecimalFloor()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.6m : 3.6m)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? -3.4m : -3.4m)));
            ClassicAssert.AreEqual(decimal.Floor(3.4m), four);
            ClassicAssert.AreEqual(decimal.Floor(3.6m), six);
            ClassicAssert.AreEqual(decimal.Floor(-3.4m), nfour);
        }

        [Test] public void TestMathTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4 : -3.4));
            ClassicAssert.AreEqual(Math.Truncate(3.4), four);
            ClassicAssert.AreEqual(Math.Truncate(3.6), six);
            ClassicAssert.AreEqual(Math.Truncate(-3.4), neg4);
        }

        [Test] public void TestStringCompareTo()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Berlin"));
            ClassicAssert.AreEqual(-1, lt);
            ClassicAssert.AreEqual(1, gt);
            ClassicAssert.AreEqual(0, eq);
        }

        [Test] public void TestStringCompareToLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") < 0);
            ClassicAssert.AreNotEqual(null, cmpLT);
            ClassicAssert.AreEqual(null, cmpEQ);
        }

        [Test] public void TestStringCompareToLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") <= 0);
            ClassicAssert.AreNotEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareToGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") > 0);
            ClassicAssert.AreNotEqual(null, cmpLT);
            ClassicAssert.AreEqual(null, cmpEQ);
        }

        [Test] public void TestStringCompareToGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") >= 0);
            ClassicAssert.AreEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreNotEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareToEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") == 0);
            ClassicAssert.AreEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareToNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") != 0);
            ClassicAssert.AreNotEqual(null, cmpLE);
            ClassicAssert.AreEqual(null, cmpEQ);
            ClassicAssert.AreNotEqual(null, cmpGT);
        }

        [Test] public void TestStringCompare()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Berlin"));
            ClassicAssert.AreEqual(-1, lt);
            ClassicAssert.AreEqual(1, gt);
            ClassicAssert.AreEqual(0, eq);
        }

        [Test] public void TestStringCompareLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") < 0);
            ClassicAssert.AreNotEqual(null, cmpLT);
            ClassicAssert.AreEqual(null, cmpEQ);
        }

        [Test] public void TestStringCompareLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") <= 0);
            ClassicAssert.AreNotEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") > 0);
            ClassicAssert.AreNotEqual(null, cmpLT);
            ClassicAssert.AreEqual(null, cmpEQ);
        }

        [Test] public void TestStringCompareGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") >= 0);
            ClassicAssert.AreEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreNotEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") == 0);
            ClassicAssert.AreEqual(null, cmpLE);
            ClassicAssert.AreNotEqual(null, cmpEQ);
            ClassicAssert.AreEqual(null, cmpGT);
        }

        [Test] public void TestStringCompareNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") != 0);
            ClassicAssert.AreNotEqual(null, cmpLE);
            ClassicAssert.AreEqual(null, cmpEQ);
            ClassicAssert.AreNotEqual(null, cmpGT);
        }

        [Test] public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(10));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(9));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(11));
            ClassicAssert.AreEqual(0, eq);
            ClassicAssert.AreEqual(1, gt);
            ClassicAssert.AreEqual(-1, lt);
        }

        [Test] public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 10m));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 9m));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 11m));
            ClassicAssert.AreEqual(0, eq);
            ClassicAssert.AreEqual(1, gt);
            ClassicAssert.AreEqual(-1, lt);
        }

        [Test] public void TestDecimalAdd()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Add((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            ClassicAssert.AreEqual(3m, onetwo);
        }

        [Test] public void TestDecimalSubtract()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Subtract((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            ClassicAssert.AreEqual(-1m, onetwo);
        }

        [Test] public void TestDecimalMultiply()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Multiply((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            ClassicAssert.AreEqual(2m, onetwo);
        }

        [Test] public void TestDecimalDivide()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Divide((c.CustomerID == "ALFKI" ? 1.0m : 1.0m), 2.0m));
            ClassicAssert.AreEqual(0.5m, onetwo);
        }

        [Test] public void TestDecimalNegate()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Negate((c.CustomerID == "ALFKI" ? 1m : 1m)));
            ClassicAssert.AreEqual(-1m, one);
        }

        [Test] public void TestDecimalRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.5m : 3.5m)));
            ClassicAssert.AreEqual(3.0m, four);
            ClassicAssert.AreEqual(4.0m, six);
        }

        [Test] public void TestDecimalTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Truncate((c.CustomerID == "ALFKI") ? 3.4m : 3.4m));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6m : 3.6m));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4m : -3.4m));
            ClassicAssert.AreEqual(decimal.Truncate(3.4m), four);
            ClassicAssert.AreEqual(decimal.Truncate(3.6m), six);
            ClassicAssert.AreEqual(decimal.Truncate(-3.4m), neg4);
        }

        [Test] public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1.0m : 3.0m) < 2.0m);
            ClassicAssert.AreNotEqual(null, alfki);
        }

        [Test] public void TestIntLessThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) < 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) < 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntLessThanOrEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) <= 2);
            var alfki2 = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 3) <= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) <= 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreNotEqual(null, alfki2);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntGreaterThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) > 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntGreaterThanOrEqual()
        {
            var alfki = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) >= 2);
            var alfki2 = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 2) >= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreNotEqual(null, alfki2);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntNotEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 2);
            ClassicAssert.AreNotEqual(null, alfki);
            ClassicAssert.AreEqual(null, alfkiN);
        }

        [Test] public void TestIntAdd()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) + 2);
            ClassicAssert.AreEqual(3, three);
        }

        [Test] public void TestIntSubtract()
        {
            var negone = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) - 2);
            ClassicAssert.AreEqual(-1, negone);
        }

        [Test] public void TestIntMultiply()
        {
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 2 : 2) * 3);
            ClassicAssert.AreEqual(6, six);
        }

        [Test] public void TestIntDivide()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 3 : 3) / 2);
            ClassicAssert.AreEqual(1, one);
        }

        [Test] public void TestIntModulo()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 7 : 7) % 4);
            ClassicAssert.AreEqual(3, three);
        }

        [Test] public void TestIntLeftShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) << 3);
            ClassicAssert.AreEqual(8, eight);
        }

        [Test] public void TestIntRightShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 32 : 32) >> 2);
            ClassicAssert.AreEqual(8, eight);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestIntBitwiseAnd()
        {
            var band = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 6 : 6) & 3);
            ClassicAssert.AreEqual(2, band);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestIntBitwiseOr()
        {
            var eleven = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 10 : 10) | 3);
            ClassicAssert.AreEqual(11, eleven);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestIntBitwiseExclusiveOr()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) ^ 1);
            ClassicAssert.AreEqual(0, zero);
        }

        [Category("ExcludeForAccess")]
        [Test]
        public void TestIntBitwiseNot()
        {
            var bneg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ~((c.CustomerID == "ALFKI") ? -1 : -1));
            ClassicAssert.AreEqual(~-1, bneg);
        }

        [Test] public void TestIntNegate()
        {
            var neg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => -((c.CustomerID == "ALFKI") ? 1 : 1));
            ClassicAssert.AreEqual(-1, neg);
        }

        [Test] public void TestAnd()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" && c.City.StartsWith("A")).Select(c => c.City).ToList();
            ClassicAssert.AreEqual(2, custs.Count);
            ClassicAssert.IsTrue(custs.All(c => c.StartsWith("A")));
        }

        [Test] public void TestOr()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" || c.City.StartsWith("A")).Select(c => c.City).ToList();
            ClassicAssert.AreEqual(14, custs.Count);
        }

        [Test] public void TestNot()
        {
            var custs = db.Customers.Where(c => !(c.Country == "USA")).Select(c => c.Country).ToList();
            ClassicAssert.AreEqual(78, custs.Count);
        }

        [Test] public void TestEqualLiteralNull()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x == null);
            //ILinqCondition select = q as ILinqCondition;
            //ClassicAssert.IsTrue(select.SelectSql.Contains("IS NULL"));
            var n = q.Count();
            ClassicAssert.AreEqual(1, n);
        }

        [Test] public void TestEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null == x);
            //ILinqCondition select = q as ILinqCondition;
            //ClassicAssert.IsTrue(select.SelectSql.Contains("IS NULL"));
            var n = q.Count();
            ClassicAssert.AreEqual(1, n);
        }

        [Test] public void TestNotEqualLiteralNull()
        {
            var q = db.Customers.Select(c => new {  counter = c.CustomerID == "ALFKI" ? null : c.CustomerID})
                .Where(x => x != null);

            //ILinqCondition select = q as ILinqCondition;
            //var sql = select.SelectSql;
            //Console.WriteLine(sql);
            //ClassicAssert.IsTrue(sql.Contains("IS NOT NULL"));
            var n = q.Count();
            ClassicAssert.AreEqual(90, n);
        }

        [Test] public void TestNotEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null != x);
            //ILinqCondition select = q as ILinqCondition;
            //var sql = select.SelectSql;
            //Console.WriteLine(sql);
            //ClassicAssert.IsTrue(sql.Contains("IS NOT NULL"));
            var n = q.Count();
            ClassicAssert.AreEqual(90, n);
        }

        [Test] public void TestConditionalResultsArePredicates()
        {
            bool value = db.Orders.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? string.Compare(c.CustomerID, "POTATO") < 0 : string.Compare(c.CustomerID, "POTATO") > 0));
            ClassicAssert.IsTrue(value);
        }

        [Test] public void TestSelectManyJoined()
        {
            var cods = 
                (from c in db.Customers
                 from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                 select new { c.ContactName, o.OrderDate }).ToList();
            ClassicAssert.AreEqual(830, cods.Count);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test]
        public void TestSelectManyJoinedDefaultIfEmpty()
        {
            var cods = (
                           from c in db.Customers
                           from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                           select new { c.ContactName, o.OrderDate }
                       ).ToList();
            ClassicAssert.AreEqual(832, cods.Count);
        }

        [Test] public void TestSelectWhereAssociation()
        {
            var ords = (
                           from o in db.Orders
                           where o.Customer.City == "Seattle"
                           select o
                       ).ToList();
            ClassicAssert.AreEqual(14, ords.Count);
        }

        [Test] public void TestSelectWhereAssociationTwice()
        {
            var n = db.Orders.Where(c => c.CustomerID == "WHITC").Count();
            var ords = (
                           from o in db.Orders
                           where o.Customer.Country == "USA" && o.Customer.City == "Seattle"
                           select o
                       ).ToList();
            ClassicAssert.AreEqual(n, ords.Count);
        }

        [Test] public void TestSelectAssociation()
        {
            var custs = (
                            from o in db.Orders
                            where o.CustomerID == "ALFKI"
                            select o.Customer
                        ).ToList();
            ClassicAssert.AreEqual(6, custs.Count);
            ClassicAssert.IsTrue(custs.All(c => c.CustomerID == "ALFKI"));
        }

        [Test]
        public void TestSelectAssociatitveMemberAccess()
        {
            var custs = (
                            from o in db.Orders
                            where o.Customer.CustomerID == "ALFKI"
                            select o.Customer.CustomerID
                        ).ToList();
            ClassicAssert.AreEqual(6, custs.Count);
            ClassicAssert.IsTrue(custs.All(c => c == "ALFKI"));
        }

        [Test]
        public void TestSelectAssociationDuplicated()
        {
            var custs = (
                            from o in db.Orders
                            join c in db.Customers on o.CustomerID equals c.CustomerID
                            where o.CustomerID == "ALFKI"
                            select o.Customer
                        ).ToList();
            ClassicAssert.AreEqual(6, custs.Count);
            ClassicAssert.IsTrue(custs.All(c => c.CustomerID == "ALFKI"));
        }

        [Test] public void TestSelectAssociations()
        {
            var doubleCusts = (
                                  from o in db.Orders
                                  where o.CustomerID == "ALFKI"
                                  select new { A = o.Customer, B = o.Customer }
                              ).Level(1).ToList();

            ClassicAssert.AreEqual(6, doubleCusts.Count);
            ClassicAssert.IsTrue(doubleCusts.All(c => c.A.CustomerID == "ALFKI" && c.B.CustomerID == "ALFKI"));
        }

        [Test] public void TestSelectAssociationsWhereAssociations()
        {
            var stuff = (
                            from o in db.Orders
                            where o.Customer.Country == "USA"
                            && o.Customer.City != "Seattle"
                            select new { A = o.Customer, B = o.Customer }
                        ).ToList();
            ClassicAssert.AreEqual(108, stuff.Count);
        }

        [Category(NOT_IMPLEMENTED)]
        [Test] public void TestGroupByCountAggregation()
        {
            var ships = (from order in db.Orders
                         group order by new { order.CustomerID }
                             into groupedShip
                             select new
                             {
                                 groupedShip.Key.CustomerID,
                                 DistinctCities = groupedShip.Count(order => order.CustomerID)
                             }).ToList();

            foreach (var ship in ships)
            {
                Console.WriteLine(ship.CustomerID + " " + ship.DistinctCities);
            }
        }

        [Test] public void TestCustomersIncludeOrders()
        {
            var custs = db.Customers.Where(c => c.CustomerID == "ALFKI").Level(HierarchyLevel.Dependend1stLvl).ToList();
            ClassicAssert.AreEqual(1, custs.Count);
            ClassicAssert.AreNotEqual(null, custs[0].Orders);
            ClassicAssert.AreEqual(6, custs[0].Orders.Count);
        }

        [Test] public void TestCustomersIncludeOrdersAndDetails()
        {
            var custs = db.Customers.Where(c => c.CustomerID == "ALFKI").Level(HierarchyLevel.Dependend2ndLvl).ToList();
            ClassicAssert.AreEqual(1, custs.Count);
            ClassicAssert.AreNotEqual(null, custs[0].Orders);
            ClassicAssert.AreEqual(6, custs[0].Orders.Count);
            ClassicAssert.IsTrue(custs[0].Orders.Any(o => o.OrderID == 10643));
            ClassicAssert.AreNotEqual(null, custs[0].Orders.Single(o => o.OrderID == 10643).Details);
            ClassicAssert.AreEqual(3, custs[0].Orders.Single(o => o.OrderID == 10643).Details.Count);
        }

        [Test]
        public void TestOuterJoinSimpleExample()
        {
            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into g
                    from o in g.DefaultIfEmpty()
                    select new { Name = c.ContactName, OrderNumber = o == null ? "(no orders)" : o.OrderID.ToString() };

            foreach (var i in q)
            {
                Console.WriteLine("Customer: {0}  Order Number: {1}",
                    i.Name.PadRight(11, ' '), i.OrderNumber);
            }

            Console.ReadLine();
        }

        [Test]
        public void TestThreeTableJoin()
        {
            var q = (from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID
                    join d in db.OrderDetails on o.OrderID equals d.OrderID
                    select d).ToList();

            foreach (var i in q)
            {
                Console.WriteLine("OrderID: {0}  ProductID: {1}",
                    i.OrderID, i.ProductID);
            }
        }

        /// <summary>
        /// Simples the count aggregation with having clause.
        /// </summary>
        [Test]
        [Category(NOT_IMPLEMENTED)]
        public void TestSimpleCountAggregationWithHavingClause()
        {
            var ships = from order in db.Orders
                        group order by new { order.ShipName }
                            into groupedShip
                            where groupedShip.Count(order => order.ShipCity) > 10
                            select new
                            {
                                groupedShip.Key.ShipName,
                                DistinctCities = groupedShip.Count(order => order.ShipCity)
                            };

            ObjectDumper.Write(ships);
        }

        [Test]
        public void TestDuplicateJoinRemovement()
        {
            var duplicatedJoins = (from order in db.Orders

                                   from employee in db.Employees where order.Employee == employee
                                   from employee1 in db.Employees where order.Employee == employee1

                                   select order);

            ObjectDumper.Write(duplicatedJoins);
        }
    }


}