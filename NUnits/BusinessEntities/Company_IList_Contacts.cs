using System.Collections;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Company that links their contacts using IList
    /// </summary>
    [Table("CompanyV1")]
    public class Company_IList_Contacts : Company
    {
        private IList contacts = new ArrayList();

        /// <summary>
        /// Gets or sets the contacts.
        /// 
        /// The [GeneralLink] Attribute is dispensable
        /// </summary>
        /// <value>The contacts.</value>
        [GeneralLink(typeof(Contact))]
        public IList Contacts
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
            return new Company_IList_Contacts();
        }
    }
}
