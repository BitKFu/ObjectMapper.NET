using System;

namespace AdFactum.Data
{
	/// <summary>
	/// Summary description for VirtualLinkAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public class VirtualLinkAttribute : Attribute
	{
		private readonly Type linkedClass;
		private readonly string linkedResultField;
		private readonly string joinFieldInLinkedClass;
		private readonly string joinFieldCurrentClass;
		private readonly string joinFieldForGlobalParameter;
		private readonly string globalParameterName = null;
	    private string subSelect = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualLinkAttribute"/> class.
        /// </summary>
	    internal VirtualLinkAttribute ()
	    {
	        
	    }

        /// <summary>
        /// Base Constructor for linking virtual fields
        /// </summary>
        /// <param name="_subSelectSQL">The _sub select SQL.</param>
        /// <param name="_linkedClass">Linked class type</param>
        /// <param name="_linkedResultField">linked result field</param>
        /// <param name="_joinFieldInLinkedClass">Linked join field</param>
        /// <param name="_joinFieldCurrentClass">Current join field</param>
        /// <param name="_joinFieldForGlobalParameter">Field that is joined with the global parameter</param>
        /// <param name="_globalParameterName">Global Parameter name</param>
        public VirtualLinkAttribute(
            string _subSelectSQL,
            Type _linkedClass,
            string _linkedResultField,
            string _joinFieldInLinkedClass,
            string _joinFieldCurrentClass,
            string _joinFieldForGlobalParameter,
            string _globalParameterName)
            : this( _linkedClass, 
                    _linkedResultField, 
                    _joinFieldInLinkedClass, 
                    _joinFieldCurrentClass, 
                    _joinFieldForGlobalParameter, 
                    _globalParameterName)
        {
            subSelect = _subSelectSQL;
        }

		/// <summary>
		/// Base Constructor for linking virtual fields
		/// </summary>
		/// <param name="_linkedClass">Linked class type</param>
		/// <param name="_linkedResultField">linked result field</param>
		/// <param name="_joinFieldInLinkedClass">Linked join field</param>
		/// <param name="_joinFieldCurrentClass">Current join field</param>
		/// <param name="_joinFieldForGlobalParameter">Field that is joined with the global parameter</param>
		/// <param name="_globalParameterName">Global Parameter name</param>
		public VirtualLinkAttribute(
			Type _linkedClass,
			string _linkedResultField,
			string _joinFieldInLinkedClass,
			string _joinFieldCurrentClass,
			string _joinFieldForGlobalParameter,
			string _globalParameterName)
		{
			linkedClass = _linkedClass;
			linkedResultField = _linkedResultField;
			joinFieldInLinkedClass = _joinFieldInLinkedClass;
			joinFieldCurrentClass = _joinFieldCurrentClass;
			joinFieldForGlobalParameter = _joinFieldForGlobalParameter;
			globalParameterName = _globalParameterName;
		}

		/// <summary>
		/// Base Constructor for linking virtual fields
		/// </summary>
		/// <param name="_linkedClass">Linked class type</param>
		/// <param name="_linkedResultField">linked result field</param>
		/// <param name="_joinFieldInLinkedClass">Linked join field</param>
		/// <param name="_joinFieldCurrentClass">Current join field</param>
		public VirtualLinkAttribute(
			Type _linkedClass,
			string _linkedResultField,
			string _joinFieldInLinkedClass,
			string _joinFieldCurrentClass)
		{
			linkedClass = _linkedClass;
			linkedResultField = _linkedResultField;
			joinFieldInLinkedClass = _joinFieldInLinkedClass;
			joinFieldCurrentClass = _joinFieldCurrentClass;
		}

        /// <summary>
        /// Base Constructor for linking virtual fields
        /// </summary>
        /// <param name="_subSelectSQL">The _sub select SQL.</param>
        /// <param name="_linkedClass">Linked class type</param>
        /// <param name="_linkedResultField">linked result field</param>
        /// <param name="_joinFieldInLinkedClass">Linked join field</param>
        /// <param name="_joinFieldCurrentClass">Current join field</param>
        public VirtualLinkAttribute(
            string _subSelectSQL,
            Type _linkedClass,
            string _linkedResultField,
            string _joinFieldInLinkedClass,
            string _joinFieldCurrentClass)
        {
            subSelect = _subSelectSQL;
            linkedClass = _linkedClass;
            linkedResultField = _linkedResultField;
            joinFieldInLinkedClass = _joinFieldInLinkedClass;
            joinFieldCurrentClass = _joinFieldCurrentClass;
        }
        
	    /// <summary>
		/// Getter for the linked class
		/// </summary>
		public Type LinkedClass
		{
			get { return linkedClass; }
		}

		/// <summary>
		/// Getter for the linked result field
		/// </summary>
		public string LinkedResultField
		{
			get { return linkedResultField; }
		}

		/// <summary>
		/// Getter for the joined field in the linked class
		/// </summary>
		public string JoinFieldInLinkedClass
		{
			get { return joinFieldInLinkedClass; }
		}

		/// <summary>
		/// Getter for the joined field in the current class
		/// </summary>
		public string JoinFieldCurrentClass
		{
			get { return joinFieldCurrentClass; }
		}

		/// <summary>
		/// Getter for the joined field in the current class
		/// </summary>
		public string JoinFieldForGlobalParameter
		{
			get { return joinFieldForGlobalParameter; }
		}

		/// <summary>
		/// Getter for the global parameter that is used
		/// </summary>
		public string GlobalParameterName
		{
			get { return globalParameterName; }
		}

        /// <summary>
        /// Gets or sets the sub select.
        /// </summary>
        /// <value>The sub select.</value>
	    public string SubSelect
	    {
	        get { return subSelect; }
	        set { subSelect = value; }
	    }
	}
}