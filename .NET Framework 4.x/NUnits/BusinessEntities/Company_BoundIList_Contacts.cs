using System.Collections;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Company that links their contacts using a bound IList
    /// </summary>
    [Table("CompanyV2")]
    public class Company_BoundIList_Contacts : Company
    {
        private IList contacts = new ArrayList();

        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>The contacts.</value>
        [BindPropertyTo(typeof(Contact))]
        public IList Contacts
        {
            get { return contacts; }
            set { contacts = value; }
        }

        public override IValueObject CreateNewObject()
        {
            return new Company_BoundIList_Contacts();
        }
    }
}
