using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Activity Class
    /// </summary>
    [Table("ACTIVITIES")]
    public class Activity : ValueObject, ICreateObject
    {
        private DateTime activityDate;
        private string   title;
        private Company  company;

        private const int UNIQUEGROUP = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        public Activity()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="activityCompany">The activity company.</param>
        /// <param name="date">The date.</param>
        /// <param name="activityTitle">The activity title.</param>
        public Activity(Company activityCompany, DateTime date, string activityTitle)
        {
            company = activityCompany;
            activityDate = date;
            title = activityTitle;
        }

        /// <summary>
        /// Gets or sets the activity date.
        /// </summary>
        /// <value>The activity date.</value>
        public DateTime ActivityDate
        {
            get { return activityDate; }
            set { activityDate = value; }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [Unique(UNIQUEGROUP, 2)]
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>The company.</value>
        [Unique(UNIQUEGROUP, 1)]
        public Company Company
        {
            get { return company; }
            set { company = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new Activity();
        }
    }
}
