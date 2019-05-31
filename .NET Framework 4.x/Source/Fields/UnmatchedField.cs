using System;

namespace AdFactum.Data.Fields
{
	/// <summary>
	/// Defines an unmatched field
	/// </summary>
	[Serializable]
    public sealed class UnmatchedField : IModification
	{
		private bool isDeleted;
		private bool isNew;

		private object fieldValue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fieldValueParameter"></param>
        public UnmatchedField(object fieldValueParameter)
		{
            fieldValue = fieldValueParameter;
		}

		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <param name="field"></param>
		public UnmatchedField(UnmatchedField field)
		{
			isDeleted = field.isDeleted;
			isNew = field.isNew;
			fieldValue = field.fieldValue;
		}

		/// <summary>
		/// Getter and Setter to change the IsModified Flag
		/// </summary>
		public bool IsModified
		{
			get { return false; }
			set
			{
			}
		}

        /// <summary>
        /// Returns the property Name
        /// </summary>
        public string PropertyName
        {
            get { return string.Empty; }
        }

		/// <summary>
		/// Getter and Setter to change the IsDeleted Flag
		/// </summary>
		public bool IsDeleted
		{
			get { return isDeleted; }
			set { isDeleted = value; }
		}

		/// <summary>
		/// Getter and Setter to change the IsNew Flag
		/// </summary>
		public bool IsNew
		{
			get { return isNew; }
			set { isNew = value; }
		}

		/// <summary>
		/// Returns the unmatched field fieldValue
		/// </summary>
		public object Fieldvalue
		{
			get { return fieldValue; }
			set { fieldValue = value; }
		}

		/// <summary>
		/// Clones an object
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new UnmatchedField(this);
		}
	}
}