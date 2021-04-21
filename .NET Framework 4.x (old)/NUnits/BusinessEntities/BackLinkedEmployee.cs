using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// The BackLinked Employee contains a back link to the company class
    /// </summary>
    [Serializable]
    public class BackLinkedEmployee : Employee
    {
        string companyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackLinkedEmployee"/> class.
        /// </summary>
        public BackLinkedEmployee()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackLinkedEmployee"/> class.
        /// </summary>
        /// <param name="firstName">Name of the first.</param>
        /// <param name="lastName">Name of the last.</param>
        public BackLinkedEmployee(string firstName, string lastName)
            :base(firstName, lastName)
        {
        }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        /// <value>The name of the company.</value>
        [BackLink(typeof(Company_With_Employees), "LegalName", "Employees", typeof(BackLinkedEmployee))]
        public string CompanyName
        {
            get { return companyName; }
            set { companyName = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new BackLinkedEmployee();
        }
    }
}
