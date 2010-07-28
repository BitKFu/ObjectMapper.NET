using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Projection.Attributes;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// Key Group 
    /// </summary>
    public class KeyGroup
    {
        /// <summary>
        ///Key Group Nr.
        /// </summary>
        public int Number;

        /// <summary>
        /// Ordering
        /// </summary>
        public int Ordering;
    }

    /// <summary>
    /// Foreign Key Group
    /// </summary>
    public class ForeignKeyGroup : KeyGroup
    {
        /// <summary>
        /// Column
        /// </summary>
        public string Column;

        /// <summary>
        /// Foreign Column
        /// </summary>
        public string ForeignColumn;

        /// <summary>
        /// Foreign Table
        /// </summary>
        public string ForeignTable;
    }

    /// <summary>
    /// Property Meta Info
    /// </summary>
    [Serializable]
    public class PropertyMetaInfo
    {
        /// <summary>
        /// Defines if the property is valid
        /// </summary>
        public ValidatorType Validator { get; set; }

        /// <summary>
        /// Column Name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Property Name
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Defines if a property is an unique constraint
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Defines if a property is a foreign key of another table/or object
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Defines if the property can be ignored by the mapper
        /// </summary>
        public bool IsIgnore { get; set; }

        /// <summary>
        /// Defines if it is a must that the field has a value
        /// </summary>
        public bool IsRequiered { get; set; }

        /// <summary>
        /// Defines if a property is a virtual Link property
        /// </summary>
        public bool IsVirtualLink { get; set; }

        /// <summary>
        /// Defines if the property is a primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Defines the length of a property, if a length value must be specified
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Defines the major version which is important for the validator object
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// Defines the minor version which is important for the validator object
        /// </summary>
        public int MinorVersion { get; set; }

        /// <summary>
        /// Defines if a link value is a general link
        /// </summary>
        public bool IsGeneralLinked { get; set; }

        /// <summary>
        /// Defines if the field is a property that is projected on another target property.
        /// </summary>
        public bool IsProjected { get; set; }

        /// <summary>
        /// Defines the primary key type of the linked object
        /// </summary>
        public Type LinkedPrimaryKeyType { get; set; }

        /// <summary>
        /// This property defines the linktarget if such is available
        /// </summary>
        public Type LinkTarget { get; set; }

        /// <summary>
        /// This property defines the property within the linktarget type that is used to bind a one to many collection
        /// </summary>
        public string LinkedTargetProperty { get; set; }

        /// <summary>
        /// True, if the association is one to many
        /// </summary>
        public bool IsOneToManyAssociation { get; set; }

        /// <summary>
        /// Specifies the default value for that property
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Unique Key Groups
        /// </summary>
        public List<KeyGroup> UniqueKeyGroups { get; set; }

        /// <summary>
        /// Foreign Key Groups
        /// </summary>
        public List<KeyGroup> ForeignKeyGroups { get; set; }

        /// <summary>
        /// Called when inserting a property
        /// </summary>
        public string InsertFunction { get; set; }

        /// <summary>
        /// Called when updating a property
        /// </summary>
        public string UpdateFunction { get; set; }

        /// <summary>
        /// Called when selecting a property
        /// </summary>
        public string SelectFunction { get; set; }

        /// <summary>
        /// Set to true, if the column shall be Unicode
        /// </summary>
        public bool IsUnicode { get; set; }

        /// <summary>
        /// Returns the preferred relation type
        /// </summary>
        public EntityRelation.OrmType? RelationType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaInfo"/> class.
        /// </summary>
        internal PropertyMetaInfo()
        {
            Length = 255;
            MajorVersion = 1;
            Validator = ValidatorType.AlwaysValid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaInfo"/> class.
        /// </summary>
        internal PropertyMetaInfo(
            string columnName,
            string propertyName,
            bool primaryKey
            )
            : this()
        {
            ColumnName = columnName;
            PropertyName = propertyName;
            IsPrimaryKey = primaryKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaInfo"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="type">The type.</param>
        public PropertyMetaInfo(string columnName, Type type)
            : this()
        {
            PropertyName = columnName;
            ColumnName = columnName.ToUpper();
            Type bindingType = InitializeFromType(type);

            LinkTarget = bindingType ?? (
                ((type.IsPrimitive == false)
              && (type.IsClass)
              && (type.IsValueObjectType()))
            ? LinkTarget = type : null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaInfo"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        public PropertyMetaInfo(PropertyInfo propertyInfo)
            : this()
        {
            /*
             * Set the attributes
             */
            object[] attributes = propertyInfo.GetCustomAttributes(true);

            /*
             * Set the property name
             */
            PropertyName = ColumnName = propertyInfo.Name;

            /*
             * Check the generic type binding in VS 2005
             */
            Type bindingType = InitializeFromType(propertyInfo.PropertyType);

            /*
             * Check the custom attributes
             */
            for (int x = 0; x < attributes.Length; x++)
            {
                var columnAttribute = attributes[x] as ColumnAttribute;
                if (columnAttribute != null)
                {
                    ColumnName = columnAttribute.Name ?? ColumnName;
                    IsRequiered = columnAttribute.IsRequired ?? IsRequiered;
                    Length = columnAttribute.Length ?? Length;
                    continue;
                }

                var nameAttribute = attributes[x] as PropertyNameAttribute;
                if (nameAttribute != null)
                {
                    ColumnName = nameAttribute.Name;
                    continue;
                }

                var lengthAttribute = attributes[x] as PropertyLengthAttribute;
                if (lengthAttribute != null)
                {
                    Length = lengthAttribute.Length;
                    continue;
                }

                var generalAttribute = attributes[x] as GeneralLinkAttribute;
                if (generalAttribute != null)
                {
                    IsGeneralLinked = true;
                    LinkedPrimaryKeyType = generalAttribute.PrimaryKeyType;

                    continue;
                }

                if (attributes[x] is VirtualLinkAttribute)
                {
                    IsVirtualLink = true;
                    continue;
                }

                if (attributes[x] is IgnoreAttribute)
                {
                    IsIgnore = true;
                    continue;
                }

                if (attributes[x] is PrimaryKeyAttribute)
                {
                    IsPrimaryKey = true;
                    continue;
                }

                if (attributes[x] is RequiredAttribute)
                {
                    IsRequiered = true;
                    continue;
                }

                var uniqueAttribute = attributes[x] as UniqueAttribute;
                if (uniqueAttribute != null)
                {
                    IsUnique = true;

                    /*
                     * Add the key group to which the unique attribute belongs
                     * If KeyGroup == 0, the unique key is a single key
                     */
                    if (UniqueKeyGroups == null)
                        UniqueKeyGroups = new List<KeyGroup>();
                    var group = new KeyGroup
                                    {
                                        Number = uniqueAttribute.KeyGroup,
                                        Ordering = uniqueAttribute.Position
                                    };
                    UniqueKeyGroups.Add(group);
                    continue;
                }

                var foreignKeyAttribute = attributes[x] as ForeignKeyAttribute;
                if (foreignKeyAttribute != null)
                {
                    IsForeignKey = true;

                    /*
                     * Add the key group to which the foreign key attribute belongs
                     * If KeyGroup == 0, the foreign key is a single foreign key
                     */
                    if (ForeignKeyGroups == null)
                        ForeignKeyGroups = new List<KeyGroup>();
                    var group = new ForeignKeyGroup
                                    {
                                        Number = foreignKeyAttribute.KeyGroup,
                                        Ordering = foreignKeyAttribute.Position,
                                        ForeignTable = foreignKeyAttribute.ForeignKeyTable,
                                        ForeignColumn = foreignKeyAttribute.ForeignKeyColumn
                                    };
                    ForeignKeyGroups.Add(group);
                    continue;
                }

                var defaultValueAttribute = attributes[x] as DefaultValueAttribute;
                if (defaultValueAttribute != null)
                {
                    DefaultValue = defaultValueAttribute.Value;
                    continue;
                }

                var bindPropertyTo = attributes[x] as BindPropertyToAttribute;
                if (bindPropertyTo != null)
                {
                    bindingType = bindPropertyTo.BindingType;
                    RelationType = bindPropertyTo.RelationType;
                    continue;
                }

                var oneToMany = attributes[x] as OneToManyAttribute;
                if (oneToMany != null)
                {
                    bindingType = oneToMany.JoinedType;
                    LinkedTargetProperty = oneToMany.JoinedProperty;
                    IsOneToManyAssociation = true;
                    RelationType = oneToMany.RelationType;
                    continue;
                }

                var validSince = attributes[x] as ValidSinceAttribute;
                if (validSince != null)
                {
                    Validator = ValidatorType.ValidSince;
                    MajorVersion = validSince.MajorVersion;
                    MinorVersion = validSince.MinorVersion;
                    continue;
                }

                var validUntil = attributes[x] as ValidUntilAttribute;
                if (validUntil != null)
                {
                    Validator = ValidatorType.ValidUntil;
                    MajorVersion = validUntil.MajorVersion;
                    MinorVersion = validUntil.MinorVersion;
                    continue;
                }

                var insert = attributes[x] as InsertFunctionAttribute;
                if (insert != null)
                {
                    InsertFunction = insert.Function;
                    continue;
                }

                var projectOnto = attributes[x] as ProjectOntoPropertyAttribute;
                if (projectOnto != null)
                {
                    bindingType = projectOnto.ProjectedType;
                    IsProjected = true;
                }

                var update = attributes[x] as UpdateFunctionAttribute;
                if (update != null)
                {
                    UpdateFunction = update.Function;
                    continue;
                }

                var select = attributes[x] as SelectFunctionAttribute;
                if (select != null)
                {
                    SelectFunction = select.Function;
                    continue;
                }

                var unicodeAttribute = attributes[x] as UnicodeAttribute;
                if (unicodeAttribute != null)
                {
                    IsUnicode = true;
                    continue;
                }
            }

            /*
             * Adjust Memo Size for Unicode types
             */
            if (IsUnicode && Length == int.MaxValue)
                Length /= 2;

            Type linkedType = propertyInfo.PropertyType;

            /*
             * Set the link target
             */
            if (!IsGeneralLinked)
            {
                /*
                 * Check if the Property Type shall be set
                 */
                if ((bindingType == null)
                    && (linkedType.IsPrimitive == false)
                    && (linkedType.IsClass)
                    && (linkedType.IsValueObjectType()))
                    LinkTarget = linkedType;
                else
                    /*
                     * If it's an interface use the interface type to retrieve the primary key type
                     */ if (bindingType == null && linkedType.IsInterface)
                    {
                        LinkTarget = null;

                        try
                        {
                            LinkedPrimaryKeyType = typeof (Guid); // take Guid as a Fallback solution
                            if (linkedType.ImplementsInterface(typeof (IValueObject)))
                                LinkedPrimaryKeyType =
                                    ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(linkedType).PropertyType;
                        }
                        catch (NoPrimaryKeyFoundException)
                        {
                            LinkedPrimaryKeyType = typeof (Guid); // take Guid as a Fallback solution
                        }

                        IsGeneralLinked = true; // mark it as a general link   
                    }
                    else
                        LinkTarget = bindingType;
            }

            /*
             * Set binded primary key (if it's not set)
             */
            if ((LinkedPrimaryKeyType == null) && (LinkTarget != null))
            {
                try
                {
                    LinkedPrimaryKeyType = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(LinkTarget).PropertyType;
                }
                catch (NoPrimaryKeyFoundException)
                {
                    LinkedPrimaryKeyType = typeof (Guid); // take Guid as a Fallback solution
                }
            }

            /*
             * On Un-Bound list classes, set the linked primary key type to guid
             */
            if ((LinkedPrimaryKeyType == null) && (linkedType != null) &&
                (linkedType.IsDictionaryType() || linkedType.IsListType()))
            {
                LinkedPrimaryKeyType = typeof (Guid);
                IsGeneralLinked = true;
            }

            /*
             * Uppercase the property Name
             */
            ColumnName = ColumnName.ToUpper();
        }

        /// <summary>
        /// Initializes from type.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        private Type InitializeFromType(Type propertyType)
        {
            Type bindingType = null;

            if (propertyType.IsGenericType)
            {
                Type[] genericTypes = propertyType.GetGenericArguments();
                bindingType = genericTypes[genericTypes.Length - 1];

                if (bindingType.IsValueObjectType() == false)
                    bindingType = null;
            }

            /*
             * Check the nullables
             */
            bool nullable = propertyType.Name.Equals("Nullable`1");
            if (nullable)
                propertyType = Nullable.GetUnderlyingType(propertyType);

            /*
             * Set the default length for float and double 
             */
            if (propertyType == typeof (double)
                || propertyType == typeof (Single))
                Length = 49;

            else if (propertyType == typeof (byte))
                Length = 3;

            else if (propertyType == typeof (short))
                Length = 6;
            else if (propertyType.IsEnum)
            {
                IsRequiered = true;
            }
            else if (propertyType == typeof (bool))
            {
                IsRequiered = true;
                DefaultValue = false;
            }

            /*
             * If it's nullable, set required to false
             */
            if (nullable)
                IsRequiered = false;

            return bindingType;
        }

        /// <summary>
        /// Returns if the access to a property is allowed or denied.
        /// </summary>
        /// <param name="majorVersionParameter">The major version parameter.</param>
        /// <param name="minorVersionParameter">The minor version parameter.</param>
        /// <returns>
        /// 	<c>true</c> if the specified major version parameter is accessible; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAccessible(int majorVersionParameter, int minorVersionParameter)
        {
            /*
             * First check the simple property
             */
            bool result =
                (!IsIgnore) &&
                ((Validator == ValidatorType.AlwaysValid)
                 || ((Validator == ValidatorType.ValidSince) && (
                                                                    (majorVersionParameter > MajorVersion)
                                                                    ||
                                                                    ((majorVersionParameter == MajorVersion) &&
                                                                     (minorVersionParameter >= MinorVersion))))
                 || ((Validator == ValidatorType.ValidUntil) && (
                                                                    (majorVersionParameter < MajorVersion)
                                                                    ||
                                                                    ((majorVersionParameter == MinorVersion) &&
                                                                     (minorVersionParameter <= MinorVersion))))
                );

            /*
             * Now check the linked table, if possible
             */
            if ((result) && (LinkTarget != null))
            {
                result = Table.GetTableInstance(LinkTarget).IsAccessible(majorVersionParameter, minorVersionParameter);
            }

            return result;
        }

        /// <summary>
        /// Returns if the access to a property is allowed or denied.
        /// </summary>
        public bool IsAccessible()
        {
            return !IsIgnore;
        }
    }
}