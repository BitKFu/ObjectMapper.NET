using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data;
using ObjectMapper.NUnits.Northwind.Entities;

namespace ObjectMapper.NUnits.Northwind
{
    public class CustomerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerInfo"/> class.
        /// </summary>
        public CustomerInfo()
        {
            
        }

        public CustomerInfo(string id, string name, string info)
        {
            Id = id;
            Name = name;
            Info = info;
        }

        public  string Id {get;set;}
        public  string Name {get;set;}
        public  string Info {get;set;}
    }


    /// <summary>
    /// Customer class within the northwind database
    /// </summary>
    [Table("Customers")]
    public class Customer : GenericValueObject<string>
    {
        public Customer()
        {
            
        }

        public Customer(string id, string name)
        {
            Id = id;
            CompanyName = name;

            IsNew = false;
        }

        [Ignore]
        public override sealed string Id
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

        [PropertyName("CustomerID")]
        [PrimaryKey]
        [PropertyLength(5)]
        public string CustomerID
        {
            get { return Id; }
            set { Id = value; }
        }
        
        [PropertyLength(40)]
        public string CompanyName { get; set;}

        [PropertyLength(30)]
        public string ContactName { get; set; }

        //[PropertyLength(30)]
        //public string ContactTitle { get; set; }

        //[PropertyLength(60)]
        //public string Address { get; set; }

        [PropertyLength(15)]
        public string City { get; set; }

        [PropertyLength(15)]
        public string Region { get; set; }

        //[PropertyLength(10)]
        //public string PostalCode { get; set; }

        [PropertyLength(15)]
        public string Country { get; set; }

        [PropertyLength(24)]
        public string Phone { get; set; }

        //[PropertyLength(24)]
        //public string Fax { get; set; }

        [OneToMany(typeof(Order), "CustomerID")]
        public List<Order> Orders { get; set; }
    }
}
