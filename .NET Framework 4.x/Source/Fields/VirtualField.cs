using System;

namespace AdFactum.Data.Fields
{
	/// <summary>
	/// Defines a single virtual field
	/// </summary>
	[Serializable]
    public sealed class VirtualField : IModification
	{
		/// <summary>
		/// Containts the description of the field
		/// </summary>
		private readonly VirtualFieldDescription fieldDescription;

		/// <summary>
		/// Contains the value of the field
		/// </summary>
		private Object fieldValue;

		/// <summary>
		/// Internal used Constructor
		/// </summary>
		private VirtualField()
		{
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="source">Field to copy</param>
		private VirtualField(
			VirtualField source
			)
		{
			fieldDescription = source.fieldDescription;

		    if (source.fieldValue == null) return;
		    
            if (source.fieldValue is ICloneable)
		        fieldValue = ((ICloneable) source.fieldValue).Clone();
		    else
		        fieldValue = source.fieldValue;
		}

		/// <summary>
		/// Creates a new field from a fielddescription with a defined attribute and a given field validator
		/// </summary>
        /// <param name="fieldDescriptionParameter">Field description</param>
		/// <param name="voAttrib">attribute</param>
		public VirtualField(VirtualFieldDescription fieldDescriptionParameter, object voAttrib)
		{
            fieldDescription = fieldDescriptionParameter;
			fieldValue = voAttrib;
			IsModified = true;
		}

		/// <summary>
		/// Implementation of the equals method
		/// </summary>
		/// <param name="test">Test field object to check if equal with the current field object.
		/// This method does not check the content of the fields.</param>
		/// <returns>true, if both objects are equal in field name and field type. </returns>
		public override bool Equals(Object test)
		{
			var testVirtualField = test as VirtualField;
			if (testVirtualField != null)
				return (fieldDescription.Equals(testVirtualField.fieldDescription));

			var testFieldDescription = test as FieldDescription;
			if (testFieldDescription != null)
				return (fieldDescription.Equals(testFieldDescription));

			return base.Equals(test);
		}

		/// <summary>
		/// Returns the GetHashCode from the field description
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return fieldDescription.GetHashCode();
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
				 * Only set the new value, if the new content differs with the stored content.
				 */
				if ((fieldValue == null)
					|| ((fieldValue != value) && (!fieldValue.Equals(value)))
					)
				{
					fieldValue = value;
					IsModified = true;
				}
			}
		}

		/// <summary>
		/// Getter for the field description attribute
		/// </summary>
		public FieldDescription FieldDescription
		{
			get { return fieldDescription; }
		}

		#region IModification Member

        /// <summary>
        /// Returns the property Name
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
			get { return false; }
			set
			{
			}
		}

		/// <summary>
		/// Only used for internal checking, if the field has to delete
		/// </summary>
		public bool IsDeleted
		{
			get { return false; }
			set { ; }
		}

		/// <summary>
		/// Always false, because a field does not need a sql insert
		/// </summary>
		public bool IsNew
		{
			get { return false; }
			set { ; }
		}

		#endregion

		#region ICloneable Member

		/// <summary>
		/// Copy method for a field object
		/// </summary>
		public Object Clone()
		{
			return new VirtualField(this);
		}

		#endregion
	}
}