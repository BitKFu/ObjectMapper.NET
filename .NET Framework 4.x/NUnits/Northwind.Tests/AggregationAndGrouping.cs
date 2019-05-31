using System.Linq;
using AdFactum.Data.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.Northwind.Entities;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    /// <summary>
    /// This test belongs to the linq test series
    /// </summary>
    [TestFixture]
    [Category("NOT_IMPLEMENTED_RIGHT_NOW")]
    public class AggregationAndGrouping : NorthwindBase
    {
        /// <summary>
        /// Simple grouping test
        /// </summary>
        [Test]
        public void MultiGroup()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                IQueryable<Order> orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName, order.ShipCity}
                            into groupedShip
                                select new
                                           {
                                               Name = groupedShip.Key.ShipName,
                                               City = groupedShip.Key.ShipCity
                                           };

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples sum aggregation test.
        /// </summary>
        [Test]
        public void SimpleAverageAggregation()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               Freight = groupedShip.Average(order => order.Freight)
                                           };

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples sum aggregation test.
        /// </summary>
        [Test]
        public void SimpleCountAggregation()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               DistinctCities = groupedShip.Count(order => order.ShipCity)
                                           };

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples the count aggregation with having clause.
        /// </summary>
        [Test]
        public void SimpleCountAggregationWithHavingClause()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                where groupedShip.Count(order => order.ShipCity) > 10
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               DistinctCities = groupedShip.Count(order => order.ShipCity)
                                           };

                ObjectDumper.Write(ships);
            }
        }



        /// <summary>
        /// Simple grouping test
        /// </summary>
        [Test]
        public void SimpleGroup()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by order.ShipName
                            into groupedShip
                                select new {groupedShip.Key};

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples sum aggregation test.
        /// </summary>
        [Test]
        public void SimpleMaxAggregation()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               Freight = groupedShip.Max(order => order.Freight)
                                           };

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples sum aggregation test.
        /// </summary>
        [Test]
        public void SimpleMinAggregation()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               Freight = groupedShip.Min(order => order.Freight)
                                           };

                ObjectDumper.Write(ships);
            }
        }

        /// <summary>
        /// Simples sum aggregation test.
        /// </summary>
        [Test]
        public void SimpleSumAggregation()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var orders = mapper.Query<Order>();
                var ships = from order in orders
                            group order by new {order.ShipName}
                            into groupedShip
                                select new
                                           {
                                               groupedShip.Key.ShipName,
                                               Freight = groupedShip.Sum(order => order.Freight)
                                           };

                ObjectDumper.Write(ships);
            }
        }
    }
}