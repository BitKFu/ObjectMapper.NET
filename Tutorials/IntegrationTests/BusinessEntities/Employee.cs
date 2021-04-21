using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// One Employee
    /// </summary>
    public class Employee : BaseVO, IPerson, ICreateObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        public Employee ()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="firstNameParam">The first name param.</param>
        /// <param name="lastNameParam">The last name param.</param>
        public Employee(string firstNameParam, string lastNameParam)
        {
            FirstName = firstNameParam;
            LastName = lastNameParam;
        }

        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        [PropertyLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        [PropertyLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Company Id
        /// </summary>
        [ForeignKey(typeof(Company), "Id")]
        public int? CompanyId { get; set; }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public virtual IValueObject CreateNewObject()
        {
            return new Employee();
        }
    }
}
