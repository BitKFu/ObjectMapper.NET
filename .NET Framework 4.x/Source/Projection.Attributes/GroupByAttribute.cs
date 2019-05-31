using System;

namespace AdFactum.Data.Projection.Attributes
{
    /// <summary>
    /// Projection Grouping
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class GroupByAttribute : Attribute
    {

    }
}
