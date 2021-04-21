using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Northwind.Entities
{
    /// <summary>
    /// Categories Table
    /// </summary>
    [Table("Categories")]
    public class Category : AutoIncValueObject
    {
        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PropertyName("CategoryID")]
        [PrimaryKey]
        [PropertyLength(5)]
        public new int? Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        /// <value>The name of the category.</value>
        [PropertyLength(15)]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [PropertyLength(int.MaxValue)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>The picture.</value>
        public byte[] Picture { get; set; }

        /// <summary>
        /// Defines a list of products within the category
        /// </summary>
        [OneToMany(typeof(Product), "CategoryId")]
        public List<Product> Products { get; set; }
    }
}
