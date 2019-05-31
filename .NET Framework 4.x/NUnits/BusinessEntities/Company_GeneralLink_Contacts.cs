using System.Collections.Generic;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    [Table("CompanyV4")]
    public class Company_GeneralLink_Contacts : Company
    {
        private List<Contact> contacts = new List<Contact>();

        /// <summary>
        /// Gets or sets the contacts.
        /// 
        /// Using the [GeneralLink] you can bind contacts and all derived objects
        /// </summary>
        /// <value>The contacts.</value>
        [GeneralLink(typeof(Contact))]      // Take the contact class as a base class for storing derived objects
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
            return new Company_GeneralLink_Contacts();
        }
    }
}
