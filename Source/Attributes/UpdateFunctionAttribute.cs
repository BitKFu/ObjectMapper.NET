using System;

namespace AdFactum.Data
{
    /// <summary>
    /// Update Functions will be called, when updating a property in database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class UpdateFunctionAttribute : DatabaseFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFunctionAttribute"/> class.
        /// </summary>
        /// <param name="dbFunction">The db function.</param>
        public UpdateFunctionAttribute(string dbFunction) : base(dbFunction)
        {
        }
    }
}
