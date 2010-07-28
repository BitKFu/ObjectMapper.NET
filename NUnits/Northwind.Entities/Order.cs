using System;
using System.Collections.Generic;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Northwind.Entities
{
    [Table("Orders")]
    public class Order : AutoIncValueObject
    {
        [Ignore]
        public override int? Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }

        [PropertyName("OrderId")]
        [PrimaryKey]
        [PropertyLength(5)]
        public int? OrderID
        {
            get { return Id; }
            set { Id = value; }
        }

        [PropertyName("CustomerId")]
        public string CustomerID { get; set; }

        [PropertyName("CustomerId")]
        public Customer Customer { get; set; }

        [PropertyName("EmployeeId")]
        public Employee Employee { get; set; }

        [PropertyName("OrderDate")]
        public DateTime OrderDate { get; set; }

        //[PropertyName("RequiredDate")]
        //public DateTime RequiredDate { get; set; }

        [PropertyName("ShippedDate")]
        public DateTime ShippedDate { get; set; }

        //[PropertyName("ShipVia")]
        //public int ShipVia { get; set; }

        [PropertyName("Freight")]
        public double Freight { get; set; }

        [PropertyName("ShipName")]
        [PropertyLength(40)]
        public string ShipName { get; set; }

        //[PropertyName("ShipAddress")]
        //[PropertyLength(60)]
        //public string ShipAddress { get; set; }

        [PropertyName("ShipCity")]
        [PropertyLength(15)]
        public string ShipCity { get; set; }

        //[PropertyName("ShipRegion")]
        //[PropertyLength(15)]
        //public string ShipRegion { get; set; }

        //[PropertyName("ShipPostalCode")]
        //[PropertyLength(10)]
        //public string ShipPostalCode { get; set; }

        //[PropertyName("ShipCountry")]
        //[PropertyLength(15)]
        //public string ShipCountry { get; set; }

        /// <summary>
        /// Defines an One to many collection
        /// </summary>
        [OneToMany(typeof(OrderDetail), "OrderID")]
        public List<OrderDetail> Details { get; set; }
    }
}
