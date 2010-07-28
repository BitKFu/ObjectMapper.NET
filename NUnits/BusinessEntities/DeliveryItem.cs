using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This is a test class to check the virtual link functionality.
    /// 
    /// The delivery item will be bound to the product using the product key
    /// </summary>
    public class DeliveryItem : BaseVO, ICreateObject
    {
        private string productKey;
        private string productName01;
        private int amount;
        private DateTime deliveringDate;
        private string productName02;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryItem"/> class.
        /// </summary>
        public DeliveryItem()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryItem"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="amountParam">The amount param.</param>
        /// <param name="delivering">The delivering.</param>
        public DeliveryItem(string key, int amountParam, DateTime delivering)
        {
            productKey = key;
            amount = amountParam;
            deliveringDate = delivering;
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
        [VirtualLink(typeof(Product), "ProductName", "ProductKey", "ProductKey", "ValidUntil", "@VALIDATION_DATE")]
        public string ProductName01
        {
            get { return productName01; }
            set { productName01 = value; }
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        [VirtualLink("SELECT * FROM PRODUCT", typeof(Product), "ProductName", "ProductKey", "ProductKey", "ValidUntil", "@VALIDATION_DATE")]
        public string ProductName02
        {
            get { return productName02; }
            set { productName02 = value; }
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        /// <summary>
        /// Gets or sets the delivering date.
        /// </summary>
        /// <value>The delivering date.</value>
        public DateTime DeliveringDate
        {
            get { return deliveringDate; }
            set { deliveringDate = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new DeliveryItem();
        }
    }
}
