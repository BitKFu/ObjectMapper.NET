using System.Collections.Generic;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Company that links their contacts using a generic List
    /// </summary>
    [Table("CompanyV3")]
    class Company_GenericList_Contacts : Company
    {
        private List<Contact> contacts = new List<Contact>();

        /// <summary>
        /// Gets or sets the contacts.
        /// 
        /// Because of the generic List the property is already bound to the contact
        /// </summary>
        /// <value>The contacts.</value>
        public List<Contact> Contacts
        {
            get { return contacts; }
            set { contacts = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new Company_GenericList_Contacts();
        }


    }
}
