using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;

namespace AdFactum.Data
{
    /// <summary>
    /// This class is used for describing a static class attribute
    /// </summary>
	[Serializable]
    public class FieldDescription
	{
		/// <summary>
		/// Field name
		/// </summary>
		private string name;

		/// <summary>
		/// Field Type
		/// </summary>
		private Type type;

		/// <summary>
		/// Content Type
		/// </summary>
		private Type content;

        /// <summary>
        /// Content Type without Generic information
        /// </summary>
	    private Type baseContentType;

		/// <summary>
		/// Parent Type
		/// </summary>
		private Type parentType;

		/// <summary>
		/// Property Info
		/// </summary>
		private Property property;

	    private bool isAutoIncrementEnabled = true;

	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldName">Field name</param>
        /// <param name="parentTypeParameter">The type from which the content type is got</param>
		/// <param name="fieldType">Field type</param>
		/// <param name="fieldContent">Content type</param>
		/// <param name="fieldProperty">Field property if available</param>
		/// <param name="isPrimaryParameter">Defines if the field is primary</param>
		public FieldDescription(string fieldName, Type parentTypeParameter, Type fieldType, Type fieldContent, Property fieldProperty, bool isPrimaryParameter)
		{
			name = fieldName.ToUpper();
            parentType = parentTypeParameter;
			type = fieldType;
			content = fieldContent;
			property = fieldProperty;

            /*
             * Check the nullables
             */
            if (content.Name.Equals("Nullable`1"))
                baseContentType = Nullable.GetUnderlyingType(content);

            if (property == null)
                property = new Property(new PropertyMetaInfo(name, fieldName, isPrimaryParameter));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parentType">Type of the parent.</param>
		/// <param name="fieldName">Field name</param>
		/// <param name="fieldContent">Content type</param>
        /// <param name="isPrimaryParameter">Defines if the field is primary</param>
        public FieldDescription(string fieldName, Type parentType, Type fieldContent, bool isPrimaryParameter)
            :this(fieldName, parentType, typeof (Field), fieldContent, null, isPrimaryParameter)
		{
		}

		/// <summary>
		/// Returns the name of the field 
		/// </summary>
		public string Name
		{
			get { return name; }
		}

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
	    public string PropertyName
	    {
            get
            {
                if (CustomProperty != null)
                    return CustomProperty.PropertyInfo != null
                               ? CustomProperty.PropertyInfo.Name
                               : CustomProperty.Key;

                return string.Empty;
            }
	    }

		/// <summary>
		/// Returns the type of the field 
		/// </summary>
		public Type ParentType
		{
			get { return parentType; }
		}

		/// <summary>
		/// Returns the type of the field 
		/// </summary>
		public Type FieldType
		{
			get { return type; }
		}

		/// <summary>
		/// Returns the type of the content
		/// </summary>
		public Type ContentType
		{
			get { return content; }
		}

		/// <summary>
		/// Returns the custom Property
		/// </summary>
		public Property CustomProperty
		{
			get { return property; }
		}

        /// <summary>
        /// Gets a value indicating whether this instance is primary.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary; otherwise, <c>false</c>.
        /// </value>
	    public bool IsPrimary
	    {
	        get { return property.MetaInfo.IsPrimaryKey; }
	    }

        /// <summary>
        /// Gets a value indicating whether this instance is auto increment.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is auto increment; otherwise, <c>false</c>.
        /// </value>
	    public bool IsAutoIncrement
	    {
            get
            {
                return IsPrimary && 
					(
					baseContentType == typeof (int) || 
					ContentType == typeof(int)
					) 
					&& isAutoIncrementEnabled;
            }
            set { isAutoIncrementEnabled = value; }
	    }

	    /// <summary>
		/// This equal method checks the property name and the property content type
		/// </summary>
		/// <param name="obj">object that has to compare</param>
		/// <returns>true, if the objects equals</returns>
		public override bool Equals(object obj)
		{
			FieldDescription fd = obj as FieldDescription;
			if (fd != null)
			{
				return name.Equals(fd.name) && content.Equals(fd.content);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns the hascode of the object
		/// </summary>
		/// <returns>Hashcode of the object</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns a short description text with property name and type
		/// </summary>
		/// <returns>Description text</returns>
		public string DebugInfo()
		{
			return "(" + name + " as " + content.FullName + ")";
		}


	}
}