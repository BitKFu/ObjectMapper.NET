using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
	/// Summary for a virtual Field object
	/// </summary>
	[Serializable]
    public class VirtualFieldDescription : FieldDescription
	{
		private readonly FieldDescription resultField;
		private readonly FieldDescription currentJoinField;
		private readonly FieldDescription targetJoinField;
		private readonly FieldDescription globalJoinField;
		private readonly string globalParameter;
		private readonly string linkTable = string.Empty;
	    private readonly string subSelect;

        private readonly Table currentTable;
        private readonly Table joinTable;
	    
		/// <summary>
		/// Constructor for a virtual field
		/// </summary>
		/// <param name="currentClass">Class type in which the virtual field is defined</param>
		/// <param name="fieldName">Name of the virtual field</param>
		/// <param name="fieldContentType">Content of the virtual field</param>
		/// <param name="fieldProperty">Content type of the virtual field</param>
		/// <param name="virtualLink">The virtual Link property </param>
		public VirtualFieldDescription(
			Type currentClass,
			string fieldName,
			Type fieldContentType,
			Property fieldProperty,
			VirtualLinkAttribute virtualLink
			)
			: this(fieldName, fieldContentType, fieldProperty,
			       virtualLink.LinkedClass,
			       ReflectionHelper.GetStaticFieldTemplate(virtualLink.LinkedClass, virtualLink.LinkedResultField),
			       currentClass,
			       ReflectionHelper.GetStaticFieldTemplate(currentClass, virtualLink.JoinFieldCurrentClass),
			       ReflectionHelper.GetStaticFieldTemplate(virtualLink.LinkedClass, virtualLink.JoinFieldInLinkedClass),
			       virtualLink.JoinFieldForGlobalParameter.IsNotNullOrEmpty() ?
			       	ReflectionHelper.GetStaticFieldTemplate(virtualLink.LinkedClass, virtualLink.JoinFieldForGlobalParameter) : null,
			       virtualLink.GlobalParameterName, virtualLink.SubSelect)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		private VirtualFieldDescription(
			string fieldName,
			Type fieldContentType,
			Property fieldProperty,
			Type targetClass,
			FieldDescription resultField,
			Type currentClass,
			FieldDescription currentJoinField,
			FieldDescription targetJoinField,
			FieldDescription globalJoinField,
			string globalParameter,
		    string subSelection)
			: base(fieldName, currentClass, typeof (VirtualLinkAttribute), fieldContentType, fieldProperty, false)
		{
		    currentTable = Table.GetTableInstance(currentClass);
            joinTable = Table.GetTableInstance(targetClass);

		    subSelect = subSelection;
			this.resultField = resultField;
			this.currentJoinField = currentJoinField;
			this.targetJoinField = targetJoinField;
			this.globalJoinField = globalJoinField;
			this.globalParameter = globalParameter;

			if (this.targetJoinField.FieldType.Equals(typeof(ListLink)))
			{
                linkTable = string.Concat(joinTable.DefaultName, "_", this.targetJoinField.Name);
			}
		}

        /// <summary>
        /// Gets the name of the target table.
        /// </summary>
        /// <value>The name of the target table.</value>
	    public string SyndicatedJoinTableName
	    {
	        get
	        {
                if (subSelect.IsNotNullOrEmpty())
                    return string.Concat("(",subSelect,")");

                return string.Concat(Condition.SCHEMA_REPLACE, 
                                     Condition.QUOTE_OPEN, JoinTable.DefaultName, Condition.QUOTE_CLOSE);
	        }
	    }
	    
		/// <summary>
		/// Returns the field description of the result field from the joined class
		/// </summary>
		public FieldDescription ResultField
		{
			get { return resultField; }
		}

		/// <summary>
		/// Returns the field description of the current field that has to join with the target join field
		/// </summary>
		public FieldDescription CurrentJoinField
		{
			get { return currentJoinField; }
		}

		/// <summary>
		/// Returns the field description of the target join field that has to join with the current field
		/// </summary>
		public FieldDescription TargetJoinField
		{
			get { return targetJoinField; }
		}

		/// <summary>
		/// Returns the field description of the field that has to match with the global parameter
		/// </summary>
		public FieldDescription GlobalJoinField
		{
			get { return globalJoinField; }
		}

		/// <summary>
		/// Global Parameter which is used to match with the global join field
		/// </summary>
		public string GlobalParameter
		{
			get { return globalParameter; }
		}

		/// <summary>
		/// Gets the link table.
		/// </summary>
		/// <value>The link table.</value>
		public string LinkTable
		{
			get { return linkTable; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is link table used.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is link table used; otherwise, <c>false</c>.
		/// </value>
		public bool IsLinkTableUsed
		{
			get { return linkTable.Length>0; }
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string VirtualIdentifier
		{
			get
			{
                return string.Concat(
                    SyndicatedJoinTableName, "_", 
                    CurrentJoinField.Name, "_", 
                    TargetJoinField.Name);
			}
		}

        /// <summary>
        /// Gets the current table.
        /// </summary>
        /// <value>The current table.</value>
	    public Table CurrentTable
	    {
	        get { return currentTable; }
	    }

        /// <summary>
        /// Gets the join table.
        /// </summary>
        /// <value>The join table.</value>
	    public Table JoinTable
	    {
	        get { return joinTable; }
	    }
	}
}