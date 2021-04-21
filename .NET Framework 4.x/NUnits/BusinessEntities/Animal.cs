using System;
using AdFactum.Data;
using AdFactum.Data.Util;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Animal class 
    /// </summary>
    public class Animal : AutoIncValueObject, ICreateObject 
    {
        private int legs;
        private string name;

        /// <summary>
        /// Gets or sets the legs.
        /// </summary>
        /// <value>The legs.</value>
        public int Legs
        {
            get { return legs; }
            set { legs = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
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
            return new Animal();
        }
    }
}
