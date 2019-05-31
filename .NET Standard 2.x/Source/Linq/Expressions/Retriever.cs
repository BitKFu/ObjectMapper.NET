using System;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// Retriever interface
    /// </summary>
    public interface IRetriever
    {
        /// <summary>
        /// Returns the value of a field or a property of the target object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        object GetValue(object target);

        /// <summary>
        /// Returns the target type
        /// </summary>
        Type Target { get; }

        /// <summary>
        /// Returns the source property or field
        /// </summary>
        MemberInfo Source { get;  }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        Type SourceType { get; }
    }

    /// <summary>
    /// The Field Tupel holds information about the source and the field of a member projection
    /// </summary>
    public class FieldTupel : IRetriever
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="sourceField">The source field.</param>
        public FieldTupel(Type targetType, FieldInfo sourceField)
        {
            Target = targetType;
            Source = sourceField;
        }

        /// <summary>
        /// Gets or sets the property target.
        /// </summary>
        public Type Target { get; set; }

        /// <summary>
        /// Gets or sets the field info.
        /// </summary>
        public FieldInfo Source { get; set; }

        /// <summary>
        /// Source implementation for the IRetriever interface
        /// </summary>
        MemberInfo IRetriever.Source { get { return Source; }}

        /// <summary>
        /// Returns a value
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public object GetValue(object target)
        {
            return Source.GetValue(target);
        }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type SourceType
        {
            get { return Source.FieldType; }
        }
    }

    /// <summary>
    /// The Property Tupel holds information about the source and the target property of a member projection
    /// </summary>
    public class PropertyTupel : IRetriever
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTupel"/> struct.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="sourceProperty">The source property.</param>
        public PropertyTupel(Type targetType, PropertyInfo sourceProperty)
        {
            Target = targetType;
            Source = sourceProperty;
        }

        /// <summary>
        /// Gets or sets the property target.
        /// </summary>
        /// <value>The target.</value>
        public Type Target { get; set; }

        /// <summary>
        /// Gets or sets the property source.
        /// </summary>
        /// <value>The source.</value>
        public PropertyInfo Source { get; set; }

        /// <summary>
        /// Source implementation for the IRetriever interface
        /// </summary>
        MemberInfo IRetriever.Source { get { return Source; } }

        /// <summary>
        /// Returns a value of the target object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public object GetValue(object target)
        {
            return Property.GetPropertyInstance(Source).GetValue(target);
        }

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type SourceType
        {
            get { return Source.PropertyType; }
        }

        /// <summary>
        /// Gets the type of the complex.
        /// </summary>
        /// <value>The type of the complex.</value>
        public Type CoveredType
        {
            get
            {
                if (!Target.IsGenericType)
                    return null;

                // Return Grouping Type
                if (Target.IsGroupingType())
                {
                    Type[] types = Target.GetGenericArguments();
                    return types[0];
                }

                return null;
            }
        }
    }
}