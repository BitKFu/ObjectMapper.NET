using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This is a test class to check the virtual link functionality.
    /// </summary>
    public class Product : BaseVO, ICreateObject
    {
        private string   productKey;
        private string   productName;
        private DateTime validUntil;        // an empty valid until date, means that the product is active

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        public Product()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="validUntilParameter">The valid until parameter.</param>
        public Product(string key, string name, DateTime validUntilParameter)
        {
            productKey = key;
            productName = name;
            validUntil = validUntilParameter;
        }
        
        /// <summary>
        /// Gets or sets the product key.
        /// </summary>
        /// <value>The product key.</value>
        [PropertyName("Key")]
        [PropertyLength(5)]
        public string ProductKey
        {
            get { return productKey; }
            set { productKey = value; }
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        [PropertyName("Name")]
        [PropertyLength(50)]
        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        /// <summary>
        /// Gets or sets the valid until.
        /// </summary>
        /// <value>The valid until.</value>
        public DateTime ValidUntil
        {
            get { return validUntil; }
            set { validUntil = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new Product();
        }
    }
}
