using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.Northwind.Entities;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    [TestFixture]
    public class OneToManyTests : NorthwindBase
    {
        public OneToManyTests()
        {
            OBM.CurrentSqlTracer = new Core.ConsoleTracer();
        }

        /// <summary>
        /// Try to load the order details within the order object
        /// </summary>
        [Test]
        public void TestLoadOrderDetails()
        {
            using (var mapper = OBM.CreateMapper(Connection))
            {
                var order = mapper.FlatLoad(typeof (Order), "10248") as Order;
                Assert.IsNotNull(order);
                Assert.IsNull(order.Details, "Details must be null, because we only flat loaded it.");

                order = mapper.Load(typeof (Order), "10248", HierarchyLevel.AllDependencies) as Order;
                Assert.IsNotNull(order);
                Assert.IsNotNull(order.Details, "Details must not be null, because we only flat loaded it.");
            }    
        }
    }
}
