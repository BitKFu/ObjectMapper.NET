using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// Default Factory
    /// </summary>
	public class UniversalFactory : IObjectFactory
	{
        private readonly static Cache<string, Type> typeCache = new Cache<string, Type>("Type");
        private readonly static Cache<string, IValueObject> instanceCache = new Cache<string, IValueObject>("ObjectFactory");

        /// <summary>
        /// Gets the type cache.
        /// </summary>
        /// <value>The type cache.</value>
        public static Cache<string, Type> TypeCache
        {
            get { return typeCache; }
        }

        /// <summary>
        /// Gets the instance cache.
        /// </summary>
        /// <value>The instance cache.</value>
        public static Cache<string, IValueObject> InstanceCache
        {
            get { return instanceCache; }
        }

        /// <summary>
		/// Creates a new object from a given type name
		/// </summary>
		/// <param name="typeName">type name</param>
		/// <returns>Returns the created object</returns>
		public object Create(string typeName)
		{
            /*
             * Try to create a quick simple type
             */
            object result = CreateQuickType(typeName);
            if (result != null)
                return result;

            /*
             * Try to get factory method from instance cache
             */
            ICreateObject template = InstanceCache.Get(typeName) as ICreateObject;
            if (template != null)
                return template.CreateNewObject();
            
            Type type = GetType(typeName);

            /*
             * Create instance
             */
            result = Activator.CreateInstance(type);

            /*
             * If it's a factory method, put into cache
             */
            template = result as ICreateObject;
            if (template != null)
                InstanceCache.Insert(typeName, template.CreateNewObject());

            return result;
		}

		/// <summary>
		/// Creates a new object from a given object type
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>Returns the created object</returns>
		public object Create(Type type)
		{
		    string typeName = type.FullName;

            /*
             * Try to create a quick simple type
             */
		    object result = CreateQuickType(typeName);
            if (result != null)
                return result;

            /*
             * Try to get factory method from instance cache
             */
		    ICreateObject template = (ICreateObject) InstanceCache.Get(typeName);
            if (template != null)
                return template.CreateNewObject();

            /*
             * Create instance
             */
            if (ReflectionHelper.IsListType(type) && type.IsInterface && type.IsGenericType)
                result = Activator.CreateInstance(typeof (List<>).MakeGenericType(type.GetGenericArguments()));
            else
                result = Activator.CreateInstance(type);

            /*
             * If it's a factory method, put into cache
             */
            template = result as ICreateObject;
            if (template != null)
                InstanceCache.Insert(typeName, template.CreateNewObject());

            return result;
		}

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public Type GetType(string typeName)
        {
            Type result = FindQuickType(typeName);
            if (result != null)
                return result;

            /*
             * Get from cache
             */
            bool inCache = TypeCache.TryGetValue(typeName, out result);
            if (inCache)
                return result;
               
            /*
             * Try to retrieve dynamic
             */
            result = Type.GetType(typeName) ?? FindType(typeName, AppDomain.CurrentDomain.GetAssemblies());

            /*
             * Add to cache
             */
            TypeCache.Insert(typeName, result);
            return result;
        }

        /// <summary>
        /// Finds the type of the quick.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        private static Type FindQuickType(string typeName)
        {
            switch(typeName)
            {
                case "System.Byte":
                case "byte": return typeof(Byte);

                case "System.SByte":
                case "sbyte": return typeof(SByte);

                case "System.Int16":
                case "short": return typeof(Int16);

                case "System.Int32":
                case "int": return typeof(Int32);

                case "System.Int64":
                case "long": return typeof(Int64);

                case "System.UInt16":
                case "ushort": return typeof(UInt16);

                case "System.UInt32":
                case "uint": return typeof(UInt32);

                case "System.UInt64":
                case "ulong": return typeof(UInt64);

                case "System.Single":
                case "float": return typeof(Single);

                case "System.Double":
                case "double": return typeof(Double);

                case "System.Decimal":
                case "decimal": return typeof(Decimal);

                case "System.Char":
                case "char": return typeof(Char);

                case "System.Boolean":
                case "bool": return typeof(Boolean);

                case "System.String":
                case "string":
                case "String": return typeof(String);
            }

            return null;
        }

        /// <summary>
        /// Finds the type of the quick.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        private static object CreateQuickType(string typeName)
        {
            switch (typeName)
            {
                case "System.Byte":
                case "byte":  return new Byte();

                case "System.SByte":
                case "sbyte": return new SByte();

                case "System.Int16":
                case "short": return new Int16();

                case "System.Int32":
                case "int": return new Int32();

                case "System.Int64":
                case "long": return new Int64();

                case "System.UInt16":
                case "ushort": return new UInt16();

                case "System.UInt32":
                case "uint": return new UInt32();

                case "System.UInt64":
                case "ulong": return new UInt64();

                case "System.Single":
                case "float": return new Single();

                case "System.Double":
                case "double": return new Double();

                case "System.Decimal":
                case "decimal": return new Decimal();

                case "System.Char":
                case "char": return new Char();

                case "System.Boolean":
                case "bool": return new Boolean();

                case "System.String":
                case "string":
                case "String": return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Finds the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns></returns>
        private static Type FindType(string type, Assembly[] assemblies)
        {
            foreach (Assembly a in assemblies)
            {
                Type result = a.GetType(type);
                if (result != null)
                    return result;
            }

            return null;
        }
 
	}
}
