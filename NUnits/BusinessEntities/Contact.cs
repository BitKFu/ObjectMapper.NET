using System;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Contact
    /// </summary>
    [Table("Contacts")]
    public class Contact : BaseVO, IPerson, ICreateObject
    {
        private string firstName;
        private string lastName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        public Contact ()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="firstNameParam">The first name param.</param>
        /// <param name="lastNameParam">The last name param.</param>
        public Contact(string firstNameParam, string lastNameParam)
        {
            firstName = firstNameParam;
            lastName = lastNameParam;
        }

        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        [PropertyLength(50)]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        [PropertyLength(50)]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public virtual IValueObject CreateNewObject()
        {
            return new Contact();
        }
    }
}
