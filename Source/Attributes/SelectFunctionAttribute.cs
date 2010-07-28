using System;

namespace AdFactum.Data
{
    /// <summary>
    /// Select Functions will be called, when selecting a property in database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class SelectFunctionAttribute : DatabaseFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFunctionAttribute"/> class.
        /// </summary>
        /// <param name="dbFunction">The db function.</param>
        public SelectFunctionAttribute(string dbFunction) : base(dbFunction)
        {
        }
    }
}
