using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Northwind.Entities
{
    /// <summary>
    /// Product Table
    /// </summary>
    [Table("Products")]
    public class Product : AutoIncValueObject
    {
        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PropertyName("ProductId")]
        [PrimaryKey]
        [PropertyLength(5)]
        public new int? Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        [PropertyLength(40)]
        public string ProductName   { get; set ; }

        /// <summary>
        /// Gets or sets the supplier id.
        /// </summary>
        /// <value>The supplier id.</value>
        public int SupplierId        { get; set ; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>The category id.</value>
        [ForeignKey(typeof(Category), "Id")]
        public int CategoryId        { get; set ; }

        /// <summary>
        /// Gets or sets the quantity per unit.
        /// </summary>
        /// <value>The quantity per unit.</value>
        [PropertyLength(20)]
        public string QuantityPerUnit    { get; set ; }

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        /// <value>The unit price.</value>
        public double UnitPrice     { get; set ; } 

        /// <summary>
        /// Gets or sets the units in stock.
        /// </summary>
        /// <value>The units in stock.</value>
        public int UnitsInStock      { get; set ; }

        /// <summary>
        /// Gets or sets the units in order.
        /// </summary>
        /// <value>The units in order.</value>
        public int UnitsOnOrder          { get; set ; }

        /// <summary>
        /// Gets or sets the re order level.
        /// </summary>
        /// <value>The re order level.</value>
        public int ReOrderLevel      { get; set ; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Product"/> is discontinued.
        /// </summary>
        /// <value><c>true</c> if discontinued; otherwise, <c>false</c>.</value>
        public bool Discontinued         { get; set ; }
    }
}
