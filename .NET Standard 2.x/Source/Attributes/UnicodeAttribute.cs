using System;

namespace AdFactum.Data
{
    /// <summary>
    /// The Unicode defines, that a attribute is created as an Unicode column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class UnicodeAttribute : Attribute
    {
    }
}
