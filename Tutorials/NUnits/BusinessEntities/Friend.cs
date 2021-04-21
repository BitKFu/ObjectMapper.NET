using System;
using System.ComponentModel;
using AdFactum.Data;
using AdFactum.Data.Projection.Attributes;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// List of all my Friend
    /// </summary>
    [Table("Friends")]
    public class Friend : Contact
    {
        private DateTime birthday;

        /// <summary>
        /// Initializes a new instance of the <see cref="Friend"/> class.
        /// </summary>
        public Friend ()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Friend"/> class.
        /// </summary>
        /// <param name="firstName">Name of the first.</param>
        /// <param name="lastName">Name of the last.</param>
        /// <param name="birthdayParam">The birthday param.</param>
        public Friend(string firstName, string lastName, DateTime birthdayParam)
        :base (firstName, lastName)
        {
            birthday = birthdayParam;
        }

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        /// <value>The birthday.</value>
        public DateTime Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new Friend();
        }
    }

    /// <summary>
    /// Class used for projections
    /// A class that is used for projects must not be a ValueObject!
    /// </summary>
    public class FriendName
    {
        /// <summary>
        /// Gets or sets the friend id.
        /// </summary>
        /// <value>The friend id.</value>
        [ProjectOntoProperty(typeof (Friend), "Id")]
        public int FriendId { get; set; }

        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        [ProjectOntoProperty(typeof (Friend), "FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        [ProjectOntoProperty(typeof (Friend), "LastName")]
        public string LastName { get; set; }
    }
}
