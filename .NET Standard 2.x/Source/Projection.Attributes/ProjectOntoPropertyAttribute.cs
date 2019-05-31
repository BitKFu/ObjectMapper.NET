using System;

namespace AdFactum.Data.Projection.Attributes
{
    /// <summary>
    /// This property is used to allow projections bind property to a value object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class ProjectOntoPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectOntoPropertyAttribute"/> class.
        /// </summary>
        /// <param name="projectedTypeParameter">The projected type parameter.</param>
        /// <param name="projectedPropertyName">Name of the projected property.</param>
        public ProjectOntoPropertyAttribute (Type projectedTypeParameter, string projectedPropertyName)
        {
            ProjectedType = projectedTypeParameter;
            ProjectedProperty = projectedPropertyName;
        }

        /// <summary> This is the type of the value object which shall be projected </summary>
        public Type ProjectedType { get; private set;}

        /// <summary> This is the name of the value object property which shall be projected </summary>
        public string ProjectedProperty { get; private set;}
    }
}