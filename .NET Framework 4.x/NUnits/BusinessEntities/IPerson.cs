using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Interface that describes a person
    /// </summary>
    public interface IPerson : IValueObject
    {
        /// <summary>
        /// Gets or sets the unique value object id.
        /// </summary>
        /// <value>The unique value object id.</value>
        [PrimaryKey]
        new int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        [PropertyLength(50)]
        string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        [PropertyLength(50)]
        string LastName
        {
            get;
            set;
        }
    }
}
