using System;
using System.Text;
using AdFactum.Data;

namespace AdFactum.Data
{
    ///<summary>
    /// Used to create a new Value Object out of an existing object instance
    ///</summary>
    public interface ICreateObject
    {
        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        IValueObject CreateNewObject();
    }
}
