using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// Summary description for ReflectionHelper.
	/// </summary>
	public static class ReflectionHelper
	{
		/// <summary>
		/// Hashtable for all virtual Links
		/// </summary>
        private static readonly Cache<MemberInfo, VirtualLinkAttribute> VirtualLinks = new Cache<MemberInfo, VirtualLinkAttribute>("Virtual Links");

        /// <summary>
        /// Place where the static projections are stored
        /// </summary>
        private static readonly Cache<Type, ProjectionClass> StaticProjectionCache = new Cache<Type, ProjectionClass>("Static Type Projections");
	    
		/// <summary>
		/// This methods checks, if a interface has been implemented by a type
		/// </summary>
		/// <param name="toValidate"></param>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public static bool ImplementsInterface(this Type toValidate, Type interfaceType)
		{
            /*
             * Perhaps it is the interface itself, or it has an implementation
             */
            if (toValidate == null) 
                return false;

            if (toValidate.Equals(interfaceType)) 
                return true;

		    Type[] implementedInterfaces = toValidate.GetInterfaces();
            for (int x=0; x<implementedInterfaces.Length; x++)
                if (implementedInterfaces[x] == interfaceType)
                    return true;

		    return false;
		}

		/// <summary>
		/// This methods checks if the toValidate Type is derived from the baseType
		/// </summary>
		/// <param name="toValidate">To validate.</param>
		/// <param name="baseType">Base Type.</param>
		/// <returns></returns>
		public static bool IsDerivedFrom(this Type toValidate, Type baseType)
		{
		    return baseType.IsAssignableFrom(toValidate);
		}

		/// <summary>
		/// Returns the property info object. It's used to get around the AmbiguousMatchException.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="realPropertyName">Name of the real property.</param>
		/// <returns></returns>
		public static PropertyInfo GetPropertyInfo(this Type type, string realPropertyName)
		{
			PropertyInfo property = null;

			try
			{
				property = type.GetProperty(realPropertyName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) ??
				           type.GetProperty(realPropertyName, BindingFlags.Instance | BindingFlags.Public);
			}
			catch (Exception)
			{
				var properties = type.GetProperties();
				var propertyEnumerator = properties.GetEnumerator();
				while (propertyEnumerator.MoveNext())
				{
					var current = (PropertyInfo) propertyEnumerator.Current;
					if ((current.Name.Equals(realPropertyName))
						&& ((property == null) || (current.DeclaringType.Equals(type))))
					{
						property = current;
						if (current.DeclaringType.Equals(type))
							break;
					}
				}
			}

			if (property == null)
                throw new ArgumentNullException("realPropertyName", "Could not find " + realPropertyName + " in " + type);

			return property;
		}

		/// <summary>
		/// Gets the static field template.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="property">The property.</param>
		/// <param name="propertyCustomInfo">The property custom info.</param>
		/// <param name="propertyVirtualLink">The property virtual link.</param>
		/// <returns></returns>
		private static FieldDescription BuildStaticFieldTemplate (Type type, PropertyInfo property, Property propertyCustomInfo, VirtualLinkAttribute propertyVirtualLink)
		{
			/*
			 * Is it a virtual Link?
			 */
            var propertyName = propertyCustomInfo.MetaInfo.ColumnName;
		    if (propertyVirtualLink != null)
			{
				return new VirtualFieldDescription
                    (property.ReflectedType, propertyName, property.PropertyType, propertyCustomInfo,
					propertyVirtualLink);
			}

			/*
			 * Is it a dictionary ?
			 */
            if (property.PropertyType.IsDictionaryType() || property.PropertyType.IsListType())
			{
                return new FieldDescription(propertyName, type, typeof(ListLink), property.PropertyType, propertyCustomInfo, false);
			}

			/*
			 * Is it a link ?
			 */
            if (property.PropertyType.IsValueObjectType())
			{
			    return propertyCustomInfo.MetaInfo.IsGeneralLinked 
                    ? new FieldDescription(propertyName, type, typeof(Link), property.PropertyType, propertyCustomInfo, false) 
                    : new FieldDescription(propertyName, type, typeof(SpecializedLink), propertyCustomInfo.MetaInfo.LinkTarget, propertyCustomInfo, false);
			}

		    /*
			 * It's a normal field ...
			 */
            return new FieldDescription(propertyName, type, typeof(Field), property.PropertyType, propertyCustomInfo, false);
		}

        /// <summary>
		/// Returns a field description
		/// </summary>
		/// <param name="property">Property Info</param>
		/// <returns></returns>
        internal static FieldDescription GetStaticFieldTemplate(PropertyInfo property)
        {
            var propertyCustomInfo = Property.GetPropertyInstance(property);
            var propertyVirtualLink = GetVirtualLinkInstance(property);

            /*
             * Do we have access to the property ? 
             */
            if ((propertyCustomInfo != null)
                && (!propertyCustomInfo.MetaInfo.IsAccessible()))
            {
                Debug.Assert(false, "Property is not accessible.");
                return null;
            }

            return BuildStaticFieldTemplate(property.ReflectedType, property, propertyCustomInfo, propertyVirtualLink);
        }

	    /// <summary>
		/// Returns a field description
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="realPropertyName">Name of the real property.</param>
		/// <returns></returns>
		internal static FieldDescription GetStaticFieldTemplate(Type type, string realPropertyName)
		{
			var property = type.GetPropertyInfo(realPropertyName);

			if (property == null)
                throw new ArgumentNullException("realPropertyName", "The Property (" + realPropertyName + ") could not be found in type " + type.FullName);

	        return GetStaticFieldTemplate(property);
		}

        /// <summary>
        /// Gets the primary key property name non recursive.
        /// That's a slower version, but prevents recursive calls which may result in stack overflow.
        /// E.g. called on BackLinkAttribute constructor
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        internal static PropertyInfo GetPrimaryKeyPropertyInfoNonRecursive(Type type)
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var primaries = (PrimaryKeyAttribute[])property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (primaries.Length > 0)
                    return property;
            }

            throw new NoPrimaryKeyFoundException(type);
        }

        /// <summary>
		/// Returns a field description
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="realPropertyName">Name of the real property.</param>
		/// <param name="databaseMajorVersion">The database major version.</param>
		/// <param name="databaseMinorVersion">The database minor version.</param>
		/// <returns></returns>
		internal static FieldDescription GetStaticFieldTemplate(Type type, string realPropertyName, int databaseMajorVersion, int databaseMinorVersion)
		{
			var property = GetPropertyInfo(type, realPropertyName);

			if (property == null)
                throw new ArgumentNullException("realPropertyName", "The Property (" + realPropertyName + ") could not be found in type " + type.FullName);

			var propertyCustomInfo = Property.GetPropertyInstance(property);
			var propertyVirtualLink = GetVirtualLinkInstance(property);

			/*
			 * Do we have access to the property ? 
			 */
			if ((propertyCustomInfo != null)
                && (!propertyCustomInfo.MetaInfo.IsAccessible(databaseMajorVersion, databaseMinorVersion)))
			{
				Debug.Assert(false, "Property is not accessible because of the selected data model version (" + databaseMajorVersion +"."+databaseMinorVersion+ ")");
				return null;
			}

			return BuildStaticFieldTemplate(type, property, propertyCustomInfo, propertyVirtualLink);
		}

        /// <summary>
        /// Determines whether [is complex type] [the specified to test].
        /// </summary>
        /// <param name="objectType">To test.</param>
        /// <returns>
        /// 	<c>true</c> if [is complex type] [the specified to test]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsComplexType(this Type objectType)
        {
            return IsListType(objectType) || IsDictionaryType(objectType) || IsValueObjectType(objectType);
        }

        /// <summary>
        /// Determines whether [is list type] [the specified to test].
        /// </summary>
        /// <param name="objectType">To test.</param>
        /// <returns>
        /// 	<c>true</c> if [is list type] [the specified to test]; otherwise, <c>false</c>.
        /// </returns>
	    public static bool IsListType (this Type objectType)
	    {
            return (objectType != typeof(string) && objectType != typeof(byte[]))
                && !IsDictionaryType(objectType)
                && (objectType.ImplementsInterface(typeof(IList)) || objectType.ImplementsInterface(typeof(IEnumerable)));
	    }

        /// <summary>
        /// Determines whether [is list type] [the specified to test].
        /// </summary>
        /// <param name="objectType">To test.</param>
        /// <returns>
        /// 	<c>true</c> if [is list type] [the specified to test]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDictionaryType(this Type objectType)
        {
            return objectType.ImplementsInterface(typeof(IDictionary));
        }

	    /// <summary>
        /// Determines whether [is list type] [the specified to test].
        /// </summary>
        /// <param name="objectType">To test.</param>
        /// <returns>
        /// 	<c>true</c> if [is list type] [the specified to test]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueObjectType(this Type objectType)
        {
            return (objectType.IsInterface ||
                    objectType.ImplementsInterface(typeof(IValueObject)))
                   && (!IsListType(objectType));
        }

        /// <summary>
        /// Returns true, if it's a queryable type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsQueryable(this Type type)
        {
            return type.ImplementsInterface(typeof(IQueryable));
        }

        /// <summary>
        /// Determines whether [is projection type] [the specified object type].
        /// </summary>
        /// <param name="projectionClassType">Type of the projection class.</param>
        /// <param name="dynamicCache">The dynamic cache.</param>
        /// <returns>
        /// 	<c>true</c> if [is projection type] [the specified object type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProjectedType(this Type projectionClassType, Cache<Type, ProjectionClass> dynamicCache)
        {
            if (projectionClassType == null)
                return false;

            ProjectionClass cachedProjection;

            /*
             * Try to get it out of the dynamic chache
             */
            if (dynamicCache != null)
            {
                cachedProjection = dynamicCache.Get(projectionClassType);
                if (cachedProjection != null) return true;
            }

            /*
             * Return projection if it's cached
             */
            cachedProjection = StaticProjectionCache.Get(projectionClassType);
            return (cachedProjection != null);
        }

	    /// <summary>
		/// Creates a unique join field name, in order to retrieve the link ids
		/// </summary>
		/// <param name="parent">Parent Type</param>
		/// <param name="child">Child Type</param>
		/// <returns>Name of the join field</returns>
		public static string GetJoinField (Type parent, Type child)
		{
            string parentTable = Table.GetTableInstance(parent).Name;
            string childTable = Table.GetTableInstance(child).Name;

			return GetJoinField(parentTable, childTable);
		}

		/// <summary>
		/// Creates a unique join field name, in order to retrieve the link ids
		/// </summary>
		/// <param name="parentTable">Name of the parent table</param>
		/// <param name="childTable">Name of the child table</param>
		/// <returns></returns>
		public static string GetJoinField (string parentTable, string childTable)
		{
			int parentLength = parentTable.Length;
			if (parentLength>10) parentLength=10;

			int childLength = childTable.Length;
			if (childLength>10) childLength = 10;

			return string.Concat( 
				parentTable.Substring(0,parentLength),
				"_",
				childTable.Substring(0,childLength),
				"_",
				DBConst.LinkIdField);
		}

		/// <summary>
		/// Returns a virtual link object if one exists.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		/// <returns>VirtualLinkAttribute instance</returns>
		public static VirtualLinkAttribute GetVirtualLinkInstance(MemberInfo memberInfo)
		{
            VirtualLinkAttribute result;

            if (memberInfo == null) 
                return null;

            /*
             * First Check
             */
            if (VirtualLinks.TryGetValue(memberInfo, out result))
                return result;

            lock (VirtualLinks)
            {
                /*
                 * Second Check
                 */
                if (VirtualLinks.TryGetValue(memberInfo, out result))
                    return result;

                /*
                 * Evaluate
                 */
                var propertyNames = (VirtualLinkAttribute[])memberInfo.GetCustomAttributes(typeof(VirtualLinkAttribute), true);
                if (propertyNames.GetLength(0) > 0)
                    result = propertyNames[0];

                VirtualLinks.Insert(memberInfo, result);
                return result;
            }
		}

        /// <summary>
        /// Gets the property meta infos.
        /// </summary>
        /// <returns></returns>
        public static PropertyMetaInfo[] GetPropertyMetaInfos(Type type)
        {
            var result = new Hashtable();
            var propertiesInfo = type.GetProperties();
            for (var counter = 0; counter < propertiesInfo.Length; counter++)
            {
                var propertyName = propertiesInfo[counter].Name;
                result[propertyName] = new PropertyMetaInfo(propertiesInfo[counter]);
            }

            var metaInfo = new PropertyMetaInfo[result.Values.Count];
            result.Values.CopyTo(metaInfo, 0);
            return metaInfo;
        }

        /// <summary>
        /// Gets the projection for a given projection class.
        /// </summary>
        /// <param name="projectionClassType">Type of the projection class.</param>
        /// <param name="dynamicCache">The dynamic cache.</param>
        /// <returns></returns>
        public static ProjectionClass GetProjection(Type projectionClassType, Cache<Type, ProjectionClass> dynamicCache)
        {
            ProjectionClass cachedProjection;

            /*
             * Try to get it out of the dynamic chache
             */
            if (dynamicCache != null)
            {
                cachedProjection = dynamicCache.Get(projectionClassType);
                if (cachedProjection != null) return cachedProjection;
            }

            /*
             * Return projection if it's cached
             */
            cachedProjection = StaticProjectionCache.Get(projectionClassType);
            if (cachedProjection != null) return cachedProjection;
            
            /*
             * Basic Types, can't be used as a projection
             */
            switch (projectionClassType.Name)
            {
                case "String":
                case "Boolean":
                    return null;
            }
            if (projectionClassType.IsValueType)
                return null;

            /*
             * Create a new projection if necessary
             */
            var projection = new ProjectionClass(projectionClassType);
            StaticProjectionCache.Insert(projectionClassType, projection);
            return projection;
        }


        /// <summary>
        /// Reveals an type of the IQueryable interface
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type RevealType(this Type type)
        {
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                if (definition == typeof (IQueryable<>)
                    || definition == typeof (IOrderedQueryable<>)
                    || definition == typeof (IEnumerable<>)
                    || definition == typeof (Query<>)) 
                return type.GetGenericArguments().First();
            }

            return type;
        }

        /// <summary>
        /// Unpacks all covered types
        /// </summary>
        /// <returns></returns>
	    public static List<Type> UnpackTypes(this Type[] types)
	    {
            var result = new List<Type>();

            foreach (var type in types)
                if (type.IsGenericType)
                {
                    result.Add(type);
                    result.AddRange(type.GetGenericArguments().UnpackTypes());
                }
                else
                    result.Add(type);

            return result;
	    }

        /// <summary>
        /// Unpacks the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static List<Type> UnpackType(this Type type)
        {
            return new[] {type}.UnpackTypes();
        }

	}
}