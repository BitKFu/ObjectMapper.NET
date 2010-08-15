using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Northwind.Entities
{
    [Table("Employees")]
    public class Employee : AutoIncValueObject
    {
        [PropertyName("EmployeeID")]
        [PrimaryKey]
        public new int? Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [PropertyLength(20)]
        public string LastName { get; set; }

        [PropertyLength(10)]
        public string FirstName { get; set; }

        [PropertyLength(30)]
        public string Title { get; set; }

        [PropertyLength(25)]
        public string TitleOfCourtesy { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime HireDate { get; set; }

        [PropertyLength(60)]
        public string Address { get; set; }

        [PropertyLength(15)]
        public string City { get; set; }

        [PropertyLength(15)]
        public string Region { get; set; }

        [PropertyLength(10)]
        public string PostalCode { get; set; }

        [PropertyLength(15)]
        public string Country { get; set; }

        [PropertyLength(24)]
        public string HomePhone { get; set; }

        [PropertyLength(4)]
        public string Extension { get; set; }

        public byte[] Photo { get; set; }

        [PropertyLength(int.MaxValue)]
        public string Notes { get; set; }

        public Employee ReportsTo { get; set; }

        [PropertyName("ReportsTo")]
        public int? ReportsToId { get; set; }
    }
}
