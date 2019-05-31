using System;

namespace AdFactum.Data
{
    /// <summary>
    /// This attribute specifies the primary key
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class PrimaryKeyAttribute : Attribute
    {
    }
}