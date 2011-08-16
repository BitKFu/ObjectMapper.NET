using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Util;
using System.Linq;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Generic Setter Delegate
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value"></param>
    public delegate void GenericSetter (object target, object value);
    
    /// <summary>
    /// Generic Getter Delegate
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public delegate object GenericGetter (object target);
    
    /// <summary>
    /// Class that contains the static methods to access the dynamic properties
    /// </summary>
    public class DynamicMethodContainer
    {
        
    }
        
    /// <summary>
	/// Property class to define logical properites of an c# property
	/// </summary>
    public class Property : PropertyDescriptor
	{
		/// <summary>
		/// Used to store the property instance classes
		/// </summary>
        private static readonly Cache<PropertyInfo, Property> PropertyInstances = new Cache<PropertyInfo, Property>("Property");

        /// <summary> Used to store the properties contained by a concrete type </summary>
        private static readonly Cache<Type, Dictionary<PropertyInfo, Property>> TypeProperties = new Cache<Type, Dictionary<PropertyInfo, Property>>("Object-Properties");

        private PropertyMetaInfo metaInfo;
	    
	    /// <summary>
	    /// Setter Delegate
	    /// </summary>
        private GenericSetter setterDelegate;

	    /// <summary>
        /// Getter Generic 
	    /// </summary>
        private GenericGetter getterDelegate;
    
	    /// <summary>
	    /// Property Info
	    /// </summary>
	    private readonly PropertyInfo propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="pmi">The pmi.</param>
        internal Property(
            PropertyMetaInfo pmi
        ) : base(pmi.PropertyName, null)
        {
            metaInfo = pmi;
        }
	    
        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="pd">The property descriptor.</param>
	    public Property(PropertyDescriptor pd)
	        :this(pd.ComponentType.GetPropertyInfo(pd.Name))
	    {
	    }
	    
        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="propertyInfoParameter">The property info parameter.</param>
        public Property(PropertyInfo propertyInfoParameter)
            : base(propertyInfoParameter.Name, null)
        {
            /*
             * Copy the property Info
             */
            propertyInfo = propertyInfoParameter;

            /*
             * Create the Meta Info
             */
            metaInfo = new PropertyMetaInfo(propertyInfo);
            
            /*
             * Now try to cache the method infos
             */
            CreateSetMethod(propertyInfo.GetSetMethod());
            CreateGetMethod(propertyInfo.GetGetMethod());
        }

        /// <summary>
        /// Creates a property out of a parameter info object
        /// </summary>
        /// <param name="parameter"></param>
        public Property(ParameterInfo parameter) 
            : base(parameter.Name, null)
        {
            metaInfo = new PropertyMetaInfo(parameter.Name, parameter.ParameterType);
            PropertyTypeOverride = parameter.ParameterType;
        }

        /// <summary>
        /// Gets a value indicating whether [contains unique default group].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [contains unique default group]; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsUniqueDefaultGroup
        {
            get
            {
                if (MetaInfo.UniqueKeyGroups == null)
                    return false;
                
                foreach (KeyGroup group in MetaInfo.UniqueKeyGroups)
                    if (group.Number == 0)
                        return true;
                
                return false;
            }
        }

	    /// <summary>
	    /// Creates a dynamic setter for the property
	    /// </summary>
	    private void CreateSetMethod (MethodInfo setMethod)
	    {
            if (setMethod == null)
                return;
            
            var arguments = new Type[2];
            arguments[0] = arguments[1] = typeof (object);

            string fullPropertyName = string.Concat(propertyInfo.DeclaringType.FullName.Replace(".", "_"), "_", propertyInfo.Name);
            Type propertyContainer = propertyInfo.DeclaringType.IsInterface ? typeof(DynamicMethodContainer) : propertyInfo.DeclaringType;

            var setter = new DynamicMethod(
                String.Concat("_Set", fullPropertyName, "_"),
                typeof(void), arguments, propertyContainer);
            ILGenerator generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);
            
            if (propertyInfo.PropertyType.IsClass)
	            generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            
            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);
            setterDelegate = (GenericSetter) setter.CreateDelegate(typeof (GenericSetter));
	    }
	    
	    /// <summary>
	    /// Creates a dynamic getter for the property
	    /// </summary>
	    /// <param name="getMethod"></param>
	    /// <returns></returns>
	    private void CreateGetMethod (MethodInfo getMethod)
	    {
	        if (getMethod == null) 
	            return;
	        
	        var arguments = new Type[1];
            arguments[0] = typeof (object);

            string fullPropertyName = string.Concat(propertyInfo.DeclaringType.FullName.Replace(".", "_"), "_", propertyInfo.Name);

            Type propertyContainer = propertyInfo.DeclaringType.IsInterface ? typeof(DynamicMethodContainer) : propertyInfo.DeclaringType;

	        var getter = new DynamicMethod(
                String.Concat("_Get", fullPropertyName, "_"),
                typeof(object), arguments, propertyContainer);
            ILGenerator generator = getter.GetILGenerator();
            generator.DeclareLocal(typeof (object));
	        generator.Emit(OpCodes.Ldarg_0);
	        generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
	        generator.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (!propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            generator.Emit(OpCodes.Ret);
            getterDelegate = (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
        }

	    /// <summary>
	    /// Delegate for the Get Method
	    /// </summary>
        public override object GetValue(object target)
	    {
            return getterDelegate(target);
	    }

	    /// <summary>
	    /// Delegate for the Set Method
	    /// </summary>
        public override void SetValue(object target, object value)
	    {
            if (setterDelegate == null)
                throw new MissingSetterException(propertyInfo.DeclaringType, propertyInfo.Name);

            setterDelegate(target, value);
            OnValueChanged(target, EventArgs.Empty);
	    }

		/// <summary>
		/// Returns an instance of the custom class Property.
		/// </summary>
		/// <param name="memberInfo">Property info object for which the custom property shall be resolved.</param>
		/// <returns>The custom class Property, if the property contains one. If not the method returns NULL</returns>
		public static Property GetPropertyInstance(PropertyInfo memberInfo)
		{
		    /*
             * First check if the value has already been cached
             */
            Property result = PropertyInstances.Get(memberInfo);
            if (result != null)
                return result;

            lock (PropertyInstances)
            {
                /*
                 * Second check
                 */
                result = PropertyInstances.Get(memberInfo);
                if (result != null)
                    return result;

                result = new Property(memberInfo);
                PropertyInstances.Insert(memberInfo, result);

                return result;
            }
        }

        /// <summary>
        /// Gets the property instances.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Dictionary<PropertyInfo, Property> GetPropertyInstances(Type type)
        {
            /*
             * First check if the value has already been cached
             */
            Dictionary<PropertyInfo, Property> result = TypeProperties.Get(type);
            if (result != null)
                return result;

            lock (TypeProperties)
            {
                /*
                 * Second check
                 */
                result = TypeProperties.Get(type);
                if (result != null)
                    return result;
                
                result = new Dictionary<PropertyInfo, Property>();

                var names = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] propertiesInfo = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo property in propertiesInfo)
                {
                    PropertyInfo previousInfo;
                    if (names.TryGetValue(property.Name, out previousInfo))
                    {
                        if (property.DeclaringType != previousInfo.DeclaringType)
                        {
                            if (property.DeclaringType.IsAssignableFrom(previousInfo.DeclaringType))
                                continue;

                            result.Remove(previousInfo);
                        }

                        names.Remove(property.Name);
                    }
                    
                    names.Add(property.Name, property);
                    result.Add(property, GetPropertyInstance(property));
                }

                TypeProperties.Insert(type, result);
            }

            return result;
        }


        ///<summary>
        ///When overridden in a derived class, returns whether resetting an object changes its value.
        ///</summary>
        ///
        ///<returns>
        ///true if resetting the component changes its value; otherwise, false.
        ///</returns>
        ///
        ///<param name="component">The component to test for reset capability. </param>
        public override bool CanResetValue(object component)
        {
            return (setterDelegate != null) && (MetaInfo.DefaultValue != null);
        }

        ///<summary>
        ///When overridden in a derived class, resets the value for this property of the component to the default value.
        ///</summary>
        ///
        ///<param name="component">The component with the property value that is to be reset to the default value. </param>
        public override void ResetValue(object component)
        {
            SetValue(component, MetaInfo.DefaultValue);
        }

        ///<summary>
	    ///When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
	    ///</summary>
	    ///
	    ///<returns>
	    ///true if the property should be persisted; otherwise, false.
	    ///</returns>
	    ///
	    ///<param name="component">The component with the property to be examined for persistence. </param>
	    public override bool ShouldSerializeValue(object component)
	    {
            return true;
	    }

	    ///<summary>
	    ///When overridden in a derived class, gets a value indicating whether this property is read-only.
	    ///</summary>
	    ///
	    ///<returns>
	    ///true if the property is read-only; otherwise, false.
	    ///</returns>
	    ///
	    public override bool IsReadOnly
	    {
            get {
                return (setterDelegate == null);
            }
	    }

	    ///<summary>
	    ///When overridden in a derived class, gets the type of the component this property is bound to.
	    ///</summary>
	    ///
	    ///<returns>
	    ///A <see cref="T:System.Type"></see> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"></see> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"></see> methods are invoked, the object specified might be an instance of this type.
	    ///</returns>
	    ///
	    public override Type ComponentType
	    {
            get { return propertyInfo.DeclaringType; }
	    }

	    ///<summary>
	    ///When overridden in a derived class, gets the type of the property.
	    ///</summary>
	    ///
	    ///<returns>
	    ///A <see cref="T:System.Type"></see> that represents the type of the property.
	    ///</returns>
	    ///
	    public override Type PropertyType
	    {
            get { return PropertyTypeOverride ?? propertyInfo.PropertyType; }
	    }

        /// <summary>
        /// Gets or sets the property type override.
        /// </summary>
        /// <value>The property type override.</value>
        protected Type PropertyTypeOverride { get; set;}

        /// <summary>
        /// Gets the meta info.
        /// </summary>
        /// <value>The meta info.</value>
        public PropertyMetaInfo MetaInfo
        {
            get { return metaInfo; }
            set { metaInfo = value;  }
        }

        /// <summary>
        /// Gets the get foreign key default group.
        /// </summary>
        /// <value>The get foreign key default group.</value>
        public ForeignKeyGroup GetForeignKeyDefaultGroup
        {
            get
            {
                if (MetaInfo.ForeignKeyGroups == null)
                    return null;

                foreach (ForeignKeyGroup group in MetaInfo.ForeignKeyGroups)
                    if (group.Number == 0)
                        return group;

                return null;
            }
        }

        /// <summary>
        /// Property Info
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        /// <summary>
        /// Returns a chacheable key for the Property
        /// </summary>
        public string Key
        {
            get { return propertyInfo == null 
                    ? metaInfo.PropertyName 
                    : string.Concat(propertyInfo.ReflectedType.Name, ".", metaInfo.PropertyName); }
        }
	}
}