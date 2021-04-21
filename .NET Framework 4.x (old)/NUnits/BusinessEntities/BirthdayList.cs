using System;
using System.Collections.Generic;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This is an example class to show a sorted list connection
    /// </summary>
    public class BirthdayList : BaseVO, ICreateObject
    {
        private SortedList<string, IPerson> contacts = new SortedList<string, IPerson>();

        private string name = "Geburtstagsliste";

        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>The contacts.</value>
        [GeneralLink(typeof(IPerson))]
        public SortedList<string, IPerson> Contacts
        {
            get { return contacts; }
            set { contacts = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [PropertyLength(64)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new BirthdayList();
        }
    }
}
