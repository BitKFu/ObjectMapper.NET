using System;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
    /// <summary>
    /// The GeneralLink attribute tells the mapper that the property links to an object, which is not explicit the same, but a derived type from the property type.
    /// If a link is general it can point to a object whos class type is derived from the property type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class GeneralLinkAttribute : Attribute
    {
        private readonly Type primaryKeyType;
        private readonly Type baseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralLinkAttribute"/> class.
        /// </summary>
        public GeneralLinkAttribute()
        {
            baseType = typeof(ValueObject);
            primaryKeyType = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(baseType).PropertyType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralLinkAttribute"/> class.
        /// </summary>
        /// <param name="baseTypeParameter">The base type parameter.</param>
        public GeneralLinkAttribute(Type baseTypeParameter)
        {
            baseType = baseTypeParameter;
            primaryKeyType = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(baseType).PropertyType;
        }

        /// <summary>
        /// Gets the type of the primary key.
        /// </summary>
        /// <value>The type of the primary key.</value>
        public Type PrimaryKeyType
        {
            get { return primaryKeyType; }
        }

        /// <summary>
        /// Gets or sets the type of the base.
        /// </summary>
        /// <value>The type of the base.</value>
        public Type BaseType
        {
            get { return baseType; }
        }
    }
}