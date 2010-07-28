using System;
using AdFactum.Data.Repository;

namespace AdFactum.Data
{
    /// <summary>
    /// The attribute BindPropertyTo is used to bind single properties or a collection to a specifc data type. 
    /// This property is always required if you want to map a property with a not specified data type - 
    /// like a property that returns an interface, an abstract class or a untyped collection.
    /// 
    /// Using the [BindPropertyTo] for that cases offers you a better performance, because the ObjectMapper .NET 
    /// knows the target data type for the property and does not need to evaluate it dynamically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public class BindPropertyToAttribute : Attribute
    {
        private readonly Type bindingType;
        private readonly EntityRelation.OrmType? relationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindPropertyToAttribute"/> class.
        /// </summary>
        internal BindPropertyToAttribute ()
        {
	        
        }
	    
        /// <summary>
        /// Accessor for the Binding Type
        /// </summary>
        public Type BindingType
        {
            get { return bindingType; }
        }

        /// <summary>
        /// Returns the Relation Type
        /// </summary>
        public EntityRelation.OrmType? RelationType
        {
            get { return relationType; }
        }

        /// <summary>
        /// This attribute is used to bind collections to a special type
        /// </summary>
        /// <param name="bindingType"></param>
        public BindPropertyToAttribute(Type bindingType)
        {
            this.bindingType = bindingType;
        }

        /// <summary>
        /// This attribute is used to bind collections to a special type, using a specific relation type
        /// </summary>
        /// <param name="bindingType">Type of the binding.</param>
        /// <param name="relationType">Type of the relation.</param>
        public BindPropertyToAttribute(Type bindingType, EntityRelation.OrmType relationType)
        {
            this.bindingType = bindingType;
            this.relationType = relationType;
        }
    }
}