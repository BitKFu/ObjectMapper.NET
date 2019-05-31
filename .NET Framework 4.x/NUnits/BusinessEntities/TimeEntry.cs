using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Time entry class
    /// </summary>
    public class TimeEntry : ValueObject
    {
        private DateTime startDate;
        private DateTime endDate;
        private string description;
        private string projectId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeEntry"/> class.
        /// </summary>
        public TimeEntry()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeEntry"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="project">The project.</param>
        /// <param name="desc">The desc.</param>
        public TimeEntry(DateTime start, DateTime end, string project, string desc)
        {
            startDate = start;
            endDate = end;
            description = desc;
            projectId = project;
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>The project id.</value>
        public string ProjectId
        {
            get { return projectId;  }
            set { projectId = value; }
        }
    }
}
