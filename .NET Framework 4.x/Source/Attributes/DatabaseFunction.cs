using System;

namespace AdFactum.Data
{
    ///<summary>
    /// This attribute is a base class for using functions on properties
    ///</summary>
    [Serializable]
    public abstract class DatabaseFunction : Attribute
    {
        private readonly string function;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFunction"/> class.
        /// </summary>
        protected DatabaseFunction(string dbFunction)
        {
            function = dbFunction;
        }

        /// <summary>
        /// Gets or sets the function.
        /// </summary>
        /// <value>The function.</value>
        public string Function
        {
            get { return function; }
        }
    }
}