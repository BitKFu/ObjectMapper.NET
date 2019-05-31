using System;

namespace AdFactum.Data
{
    /// <summary>
    /// Insert Functions will be called, when updating a property in database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class InsertFunctionAttribute : DatabaseFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertFunctionAttribute"/> class.
        /// </summary>
        /// <param name="dbFunction">The db function.</param>
        public InsertFunctionAttribute(string dbFunction) : base(dbFunction)
        {
        }
    }
}