using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
	/// <summary>
	/// Class that holds the single properties that can be put on a class to express the table properties
	/// </summary>
    public class Table
	{
		/// <summary>
		/// Static Hashtable that contains the table instances.
		/// </summary>
        private static readonly Cache<MemberInfo, Table> tableInstances = new Cache<MemberInfo, Table>("Table");
        
		#region Private attributes

		/// <summary>
		/// Defines if the table is valid
		/// </summary>
        private readonly ValidatorType validator = ValidatorType.AlwaysValid;

		/// <summary>
		/// Table Name
		/// </summary>
        private readonly string name;

        /// <summary>
        /// Alternative Table Names
        /// </summary>
	    private readonly Dictionary<DatabaseType, string> alternativeNames;

		/// <summary>
		/// True, if the table contains static data
		/// </summary>
        private readonly bool isStatic;

        /// <summary>
        /// True, if the table is a view definition
        /// </summary>
        private readonly bool isView;

		/// <summary>
		/// True, if the table must be weak referenced
		/// </summary>
        private readonly bool isWeakReferenced;

	    /// <summary>
	    /// Foreign Key List
	    /// </summary>
        private readonly List<ForeignKeyAttribute> foreignKeys = new List<ForeignKeyAttribute>();

        /// <summary>
        /// Class Type
        /// </summary>
	    private readonly Type classType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
	    public Table(Type type)
	    {
            MajorVersion = 1;
            classType = type;

            object[] attributes = type.GetCustomAttributes(true);

            /*
             * Go through all attributes
             */
            for (int x = 0; x < attributes.Length; x++)
            {
                var va = attributes[x] as ViewAttribute;
                if (va != null)
                {
                    if (va.DatabaseType == null)
                        name = va.Name;
                    else
                    {
                        if (alternativeNames == null) alternativeNames = new Dictionary<DatabaseType, string>();
                        alternativeNames.Add(va.DatabaseType.Value, va.Name);
                    }
                        

                    isView = true;
                }

                var ta = attributes[x] as TableAttribute;
                if (ta != null)
                {
                    if (ta.DatabaseType == null)
                        name = ta.Name;
                    else
                    {
                        if (alternativeNames == null) alternativeNames = new Dictionary<DatabaseType, string>();
                        alternativeNames.Add(ta.DatabaseType.Value, ta.Name);
                    }
                    continue;
                }

                var fka = attributes[x] as ForeignKeyAttribute;
                if (fka != null)
                {
                    ForeignKeys.Add(fka);
                    continue;
                }

                var sda = attributes[x] as StaticDataAttribute;
                if (sda != null)
                {
                    isStatic = true;
                    continue;
                }

                var wra = attributes[x] as WeakReferencedAttribute;
                if (wra != null)
                {
                    isWeakReferenced = true;
                    continue;
                }

                var validSince = attributes[x] as ValidSinceAttribute;
                if (validSince != null)
                {
                    validator = ValidatorType.ValidSince;
                    MajorVersion = validSince.MajorVersion;
                    MinorVersion = validSince.MinorVersion;
                    continue;
                }

                var validUntil = attributes[x] as ValidUntilAttribute;
                if (validUntil != null)
                {
                    validator = ValidatorType.ValidUntil;
                    MajorVersion = validUntil.MajorVersion;
                    MinorVersion = validUntil.MinorVersion;
                    continue;
                }

            }

            // If, no table attribute has been specified,
            // look, if there's a meaningfull name available ;)
            if (name == null)
            {
                if (type.IsGenericType && !type.ImplementsInterface(typeof(IValueObject)))
                    name = string.Empty;
                else
                    name = type.FullName;
            }

            if (!string.IsNullOrEmpty(name))
                name = name.Substring(name.LastIndexOf(".") + 1); //.ToUpper();
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        public Table()
        {
            MajorVersion = 1;
            classType = null;
        }

	    #endregion

		#region Public Members

		/// <summary>
		/// Table Name
		/// </summary>
		/// <value>The name.</value>
		public string DefaultName
		{
			get
			{
                if (name == null)
                    throw new InvalidOperationException("This object hasn't been initialized or is defect for type: " + classType);

			    return name;
			}
		}

        /// <summary>
        /// Gets the table name for a given database type
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        public string GetName(DatabaseType dbType)
        {
            string specialName;
            if (alternativeNames == null || alternativeNames.TryGetValue(dbType, out specialName) == false)
                return DefaultName;

            return specialName;
        }

	    /// <summary>
		/// True, if the table contains static data
		/// </summary>
		public bool IsStatic
		{
			get { return isStatic; }
		}

        /// <summary>
        /// Gets a value indicating whether this instance is view.
        /// </summary>
        /// <value><c>true</c> if this instance is view; otherwise, <c>false</c>.</value>
	    public bool IsView
	    {
	        get { return isView; }
	    }

		/// <summary>
		/// True, if the table must be weak referenced
		/// </summary>
		public bool IsWeakReferenced
		{
			get { return isWeakReferenced; }
		}

        /// <summary>
        /// Foreign Key List
        /// </summary>
        /// <value>The foreign keys.</value>
	    public IList ForeignKeys
	    {
	        get { return foreignKeys; }
	    }

        /// <summary>
        /// Class Type
        /// </summary>
        /// <value>The type of the class.</value>
	    public Type ClassType
	    {
	        get { return classType; }
	    }

	    /// <summary>
	    /// Defines the major version which is important for the validator object
	    /// </summary>
	    public int MajorVersion { get; set; }

	    /// <summary>
	    /// Defines the minor version which is important for the validator object
	    /// </summary>
	    public int MinorVersion { get; set; }

	    #endregion

		/// <summary>
		/// Gets the table instance.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		/// <returns></returns>
		public static Table GetTableInstance(MemberInfo memberInfo)
		{
			Table result = tableInstances.Get(memberInfo);
            if (result != null)
                return result;

            lock (tableInstances)
            {
                /*
                 * Second check, to ensure that no duplicate data will be evaluated
                 */
                result = tableInstances.Get(memberInfo);
                if (result != null)
                    return result;

                /*
                 * Convert to type
                 */
                var type = memberInfo as Type;
                if (type == null) return null;

                /*
                 * Create a new table object
                 */
                result = new Table(type);
                tableInstances.Insert(type, result);
                return result;
            }
		}

        /// <summary>
        /// Evaluates the table instance for a special property.
        /// If the property is tagged with the "ProjectOntoProperty" Attribute.
        /// The target of the projection will returned.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static Table GetTableInstance(FieldDescription description)
        {
            PropertyMetaInfo metaInfo = description.CustomProperty.MetaInfo;
            Type target = metaInfo.IsProjected ? metaInfo.LinkTarget : description.ParentType;
            return GetTableInstance(target);
        }

		/// <summary>
		/// Returns if the access to a property is allowed or denied.
		/// </summary>
		public bool IsAccessible(int majorVersion, int minorVersion)
		{
			return  (validator == ValidatorType.AlwaysValid)
		    
                || ((validator == ValidatorType.ValidSince) && (
                            (majorVersion > MajorVersion) 
                        || ((majorVersion == MajorVersion) && (minorVersion >= MinorVersion))))
		    
				|| ((validator == ValidatorType.ValidUntil) && (
                            (majorVersion < MajorVersion) 
                        || ((majorVersion == MajorVersion) && (minorVersion <= MinorVersion))));
		}

	}
}
