﻿using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Northwind.Entities
{
    /// <summary>
    /// Order Details
    /// </summary>
    [Table("ORDER_DETAILS")]
    public class OrderDetail : ValueObject
    {
        [PropertyName("OrderID")]
        [PropertyLength(5)]
        [ForeignKey(typeof(Order), "Id")]
        public int OrderID { get; set; }

        [PropertyName("OrderID")]
        public Order Order { get; set;}

        public int ProductID { get; set;}

        [PropertyName("ProductID")]
        public Product Product { get; set; }

        public double UnitPrice { get; set; }

        public int Quantity { get; set; }

        public double Discount { get; set; }
    }
}
