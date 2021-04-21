using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AdFactum.Data;
using AdFactum.Data.Projection.Attributes;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// The full featured company re-presents a company that owns employees and contacts
    /// </summary>
    [Table("FFCompany")]
    public class FullFeaturedCompany : Company
    {
        private IList employees = new ArrayList();
        private List<Contact> contacts = new List<Contact>();
        private List<PhoneBookEntry> phoneBook = new List<PhoneBookEntry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class.
        /// </summary>
        public FullFeaturedCompany()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class.
        /// </summary>
        /// <param name="legalNameParam">The legal name param.</param>
        public FullFeaturedCompany(string legalNameParam)
            :base(legalNameParam)
        {
        }

        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PrimaryKey]
        public new int? Id
        {
            get { return base.Id;  }
            set { base.Id = value; }
        }

        /// <summary>
        /// Gets or sets the employees.
        /// </summary>
        /// <value>The employees.</value>
        [BindPropertyTo(typeof(Employee))]
        public new IList Employees
        {
            get { return employees; }
            set { employees = value; }
        }

        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>The contacts.</value>
        [BindPropertyTo(typeof(Contact))]
        public List<Contact> Contacts
        {
            get { return contacts; }
            set { contacts = value; }
        }

        public Employee Owner { get; set; }

        /// <summary>
        /// Gets or sets the phone book.
        /// </summary>
        /// <value>The phone book.</value>
        public List<PhoneBookEntry> PhoneBook
        {
            get { return phoneBook; }
            set { phoneBook = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new FullFeaturedCompany();
        }
       }

    /// <summary>
    /// Projects results of the company legalname and the employee names
    /// </summary>
    public class FullFeaturedEmployee 
    {
        private int companyId;
        private int employeeId;

        private string companyName;
        private string firstName;
        private string lastName;

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        /// <value>The name of the company.</value>
        [ProjectOntoProperty(typeof(FullFeaturedCompany), "LegalName")]
        public string CompanyName
        {
            get { return companyName; }
            set { companyName = value; }
        }

        [ProjectOntoProperty(typeof(Employee), "FirstName")]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [ProjectOntoProperty(typeof(Employee), "LastName")]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        [ProjectOntoProperty(typeof(FullFeaturedCompany), "Id")]
        public int CompanyId
        {
            get { return companyId; }
            set { companyId = value; }
        }

        [ProjectOntoProperty(typeof(Employee), "Id")]
        public int EmployeeId
        {
            get { return employeeId; }
            set { employeeId = value; }
        }
    }
}
