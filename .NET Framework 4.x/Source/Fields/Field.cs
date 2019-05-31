using System;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Fields
{
	/// <summary>
	/// The Field class contains a abstract description of a field with it's attributes, class types and names. Furthermore a Field class contains the concrete value of a field.
	/// </summary>
	[Serializable]
    public sealed class Field : IModification
	{
		/// <summary>
		/// Containts the description of the field
		/// </summary>
		private readonly FieldDescription fieldDescription;

		/// <summary>
		/// Contains the value of the field
		/// </summary>
		private object fieldValue;

        /// <summary>
        /// Stores the old value in order to get a diff
        /// </summary>
	    private object oldValue;

		/// <summary>
		/// Defines, if the field is modified since the last store
		/// </summary>
		private bool isModified; 

		/// <summary>
		/// Internal used Constructor
		/// </summary>
		private Field()
		{
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="source">Field to copy</param>
		internal Field(
			Field source
			)
		{
			fieldDescription = source.fieldDescription;
			isModified = source.IsModified;

		    if (source.fieldValue == null) return;
		    
            if (source.fieldValue is ICloneable)
		        oldValue = fieldValue = ((ICloneable) source.fieldValue).Clone();
		    else
		        oldValue = fieldValue = source.fieldValue;
		}

		/// <summary>
		/// Creates a new field from a fielddescription with a defined attribute and a given field validator
		/// </summary>
        /// <param name="fieldDescriptionParameter">Field description</param>
		/// <param name="voAttrib">attribute</param>
		public Field(FieldDescription fieldDescriptionParameter, object voAttrib)
		{
            fieldDescription = fieldDescriptionParameter;
			Value = voAttrib;
		}

		/// <summary>
		/// Copy method for a field object
		/// </summary>
		public Object Clone()
		{
            if (Name == DBConst.LastUpdateField)    // The LastUpdate Field must not be cloned
                return this;

			return new Field(this);
		}

		/// <summary>
		/// Implementation of the equals method
		/// </summary>
		/// <param name="test">Test field object to check if equal with the current field object.
		/// This method does not check the content of the fields.</param>
		/// <returns>true, if both objects are equal in field name and field type. </returns>
		new public bool Equals(Object test)
		{
			var testField = test as Field;
			if (testField != null)
				return (fieldDescription.Equals(testField.fieldDescription));

			var testFieldDescription = test as FieldDescription;
			if (testFieldDescription != null)
				return (fieldDescription.Equals(testFieldDescription));

			return base.Equals(test);
		}

		/// <summary>
		/// Returns the hashcode
		/// </summary>
		/// <returns>Returns the hascode from the field description member</returns>
		public override int GetHashCode()
		{
			return fieldDescription.GetHashCode();
		}

		/// <summary>
		/// Returns the field name
		/// </summary>
		public String Name
		{
			get { return fieldDescription.Name; }
		}

		/// <summary>
		/// Returns the field type
		/// </summary>
		public Type Type
		{
			get { return fieldDescription.ContentType; }
		}

		/// <summary>
		/// Returns the field content
		/// </summary>
		public Object Value
		{
			get { return fieldValue; }
			set
			{
                /*
                 * If value is NULL, DateTime.MinValue or Guid.Empty return
                 * That happens, if the value hasn't been modified.
                 */
                if (
                     ((value == null) || (value.Equals(DateTime.MinValue)) || (value.Equals(Guid.Empty)) || value.Equals('\0'))
                  && ((fieldValue == null) || (fieldValue.Equals(DateTime.MinValue)) || (fieldValue.Equals(Guid.Empty)) || (fieldValue.Equals('\0')))
                  ) 
                    return;


				/*
				 * If value is ENUM
				 */
				if (value is Enum)
				{
					if ((fieldValue == null) 
					|| (!Enum.Parse(TypeHelper.GetBaseType(fieldDescription.ContentType), fieldValue.ToString(), true).Equals(value) ))
					{
					    oldValue = fieldValue;
						fieldValue = value;
						IsModified = true;
					}

					return;
				}

                /*
                 * Convert Char to String
                 */
                if (value is char)
                {
                    if ((char)value == '\0')
                        value = null;
                    else
                        value = value.ToString();
                }

			    /*
                 * Only set the new value, if the new content differs with the stored content.
                 */
				if ((fieldValue == null) || (!fieldValue.Equals(value)))
				{
                    oldValue = fieldValue;
                    fieldValue = value;
					IsModified = true;
				}
			}
		}

        /// <summary>
        /// Returns the property name
        /// </summary>
	    public string PropertyName
	    {
            get { return FieldDescription.PropertyName; }
	    }

	    /// <summary>
		/// Returns true, if the field object changed since the last save.
		/// </summary>
		public bool IsModified
		{
			get { return isModified; }
			set { isModified = value; }
		}

		private bool isDeleted;

		/// <summary>
		/// Only used for internal checking, if the field has to delete
		/// </summary>
		public bool IsDeleted
		{
			get { return isDeleted; }
			set { isDeleted = value; }
		}

		/// <summary>
		/// Getter for the field description attribute
		/// </summary>
		public FieldDescription FieldDescription
		{
			get { return fieldDescription; }
		}

		/// <summary>
		/// Always false, because a field does not need a sql insert
		/// </summary>
		public bool IsNew
		{
			get { return false; }
			set { ; }
		}

	    /// <summary>
	    /// Stores the old value in order to get a diff
	    /// </summary>
	    public object OldValue
	    {
	        get { return oldValue; }
	    }
	}
}