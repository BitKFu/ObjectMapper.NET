using System;

namespace AdFactum.Data
{
    /// <summary>
    /// If this attribute is set, it's not allowed to store the property in database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class IgnoreAttribute : Attribute
    {
    }
}