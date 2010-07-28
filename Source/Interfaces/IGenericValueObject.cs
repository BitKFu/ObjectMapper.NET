using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AdFactum.Data
{
    /// <summary>
    /// Interface for a generic value object 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericValueObject<T> : IValueObject
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [PrimaryKey]
        new T Id
        {
            [DebuggerStepThrough]
            get;
            set;
        }
    }
}
