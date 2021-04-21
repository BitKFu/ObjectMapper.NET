using System;
using System.Collections.Generic;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Company base class
    /// </summary>
    public class Company : BaseVO, ICreateObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class.
        /// </summary>
        public Company ()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class.
        /// </summary>
        /// <param name="legalNameParam">The legal name param.</param>
        public Company(string legalNameParam)
        {
            LegalName = legalNameParam;
        }

        /// <summary>
        /// Gets or sets the name of the legal.
        /// </summary>
        /// <value>The name of the legal.</value>
        [PropertyLength(100)]
        public string LegalName { get; set; }

        /// <summary>
        /// Gets or sets the Employee List
        /// </summary>
        [OneToMany(typeof(Employee), "CompanyId")]
        public List<Employee> Employees { get; set; }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public virtual IValueObject CreateNewObject()
        {
            return new Company();
        }
    }
}
