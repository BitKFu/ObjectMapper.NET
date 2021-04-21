using System;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    [Table ("PhoneEntries")]
    public class PhoneBookEntry : BaseVO, ICreateObject
    {
        private string phoneNumber;
        private IPerson person;
        private IPerson partner;    

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneBookEntry"/> class.
        /// </summary>
        public PhoneBookEntry()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneBookEntry"/> class.
        /// </summary>
        /// <param name="personParam">The person param.</param>
        /// <param name="partnerParam">The partner param.</param>
        /// <param name="phoneParam">The phone param.</param>
        public PhoneBookEntry(IPerson personParam, IPerson partnerParam, string phoneParam)
        {
            person = personParam;
            partner = partnerParam;
            phoneNumber = phoneParam;
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        [PropertyLength(50)]
        [PropertyName("Phone_Number")]
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = value; }
        }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        [GeneralLink]
        public IPerson Person
        {
            get { return person; }
            set { person = value; }
        }

        /// <summary>
        /// Gets or sets the partner.
        /// </summary>
        /// <value>The partner.</value>
        [GeneralLink]
        public IPerson Partner
        {
            get { return partner; }
            set { partner = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new PhoneBookEntry();
        }
    }
}
