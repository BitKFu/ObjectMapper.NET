using System;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
    /// <summary>
    /// Summary description for BackLinkAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class BackLinkAttribute : VirtualLinkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackLinkAttribute"/> class.
        /// </summary>
        internal BackLinkAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackLinkAttribute"/> class.
        /// </summary>
        /// <param name="parentClass">The _parent class.</param>
        /// <param name="resultProperty">The _result property.</param>
        /// <param name="parentProperty">The _parent property.</param>
        /// <param name="currentClass">The _current class.</param>
        public BackLinkAttribute(
            Type parentClass,
            string resultProperty,
            string parentProperty,
            Type currentClass
            )
            : base(parentClass, resultProperty, parentProperty,
                   ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(currentClass).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackLinkAttribute"/> class.
        /// </summary>
        /// <param name="parentClass">The _parent class.</param>
        /// <param name="resultProperty">The _result property.</param>
        /// <param name="parentProperty">The _parent property.</param>
        /// <param name="currentClass">The _current class.</param>
        /// <param name="joinFieldForGlobalParameter">The _join field for global parameter.</param>
        /// <param name="globalParameterName">Name of the _global parameter.</param>
        public BackLinkAttribute(
            Type parentClass,
            string resultProperty,
            string parentProperty,
            Type currentClass,
            string joinFieldForGlobalParameter,
            string globalParameterName
            )
            : base(parentClass, resultProperty, parentProperty,
                   ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(currentClass).Name,
                   joinFieldForGlobalParameter, globalParameterName)
        {
        }
    }
}