using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Company with employees
    /// </summary>
    [Table("CompanyEmp")]
    class Company_With_Employees : Company
    {
        private List<BackLinkedEmployee> employees = new List<BackLinkedEmployee>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Company_With_Employees"/> class.
        /// </summary>
        /// <param name="legalName">Name of the legal.</param>
        public Company_With_Employees(string legalName)
        :base(legalName)
        {
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company_With_Employees"/> class.
        /// </summary>
        public Company_With_Employees()
        {
            
        }

        /// <summary>
        /// Gets or sets the employees.
        /// </summary>
        /// <value>The employees.</value>
        public new List<BackLinkedEmployee> Employees
        {
            get { return employees; }
            set { employees = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new Company_With_Employees();
        }
    }
}
