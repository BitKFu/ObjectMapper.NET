using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// This class describes a where Clause, that is used by the persister Interface
	/// </summary>
	public class ConditionList : ICondition
	{
		/// <summary>
		/// Collection of Constraits to check
		/// </summary>
		private ICondition[] additionalConditions = new ICondition[0];

		/// <summary>
		/// Defines the condition that is put in front of the brackets
		/// </summary>
		private ConditionOperator constraintType = ConditionOperator.AND;

        /// <summary>
        /// 
        /// </summary>
        private ConditionClause conditionClause = ConditionClause.Undefined;
        
        /// <summary>
		/// Default Constructor
		/// </summary>
		public ConditionList(ConditionOperator _constraintType, params ICondition[] _conditions)
		{
			constraintType = _constraintType;
			additionalConditions = _conditions;
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ConditionList(params ICondition[] _conditions)
		{
			additionalConditions = _conditions;
		}

        /// <summary>
        /// Initializes a new instance of the  class.
        /// </summary>
	    public ConditionList()
	    {}

		/// <summary>
		/// Adds a field to the conditions
		/// </summary>
		/// <param name="condition">field condition</param>
		public void Add(ICondition condition)
		{
			ICondition[] newConditions = new ICondition[additionalConditions.Length + 1];
			additionalConditions.CopyTo(newConditions, 0);
			newConditions[additionalConditions.Length] = condition;
			additionalConditions = newConditions;
		}

		/// <summary>
		/// Add several conditions
		/// </summary>
		/// <param name="Add">field conditions</param>
		public void Add(ICondition[] Add)
		{
			ICondition[] newConditions = new ICondition[additionalConditions.Length + Add.Length];
			additionalConditions.CopyTo(newConditions, 0);
			Add.CopyTo(newConditions, additionalConditions.Length);

			additionalConditions = newConditions;
		}

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition
		/// </summary>
		public virtual Set Tables
		{
			get
			{
				Set result = new Set();
				foreach (ICondition condition in AdditionalConditions)
					result.Merge(condition.Tables);

				return result;
			}
		}

		#region ICondition Standard Members

		/// <summary>
		/// Standard AND Operator
		/// </summary>
		public ConditionOperator Type
		{
			get { return constraintType; }
		}

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		public virtual string ConditionString
		{
			get
			{
				StringBuilder result = new StringBuilder();
				for (int x = 0; x < AdditionalConditions.Length; x++)
					result.Append(Condition.NestedCondition);
				return result.ToString();
			}
		}

	    /// <summary>
	    /// Gets the parameter.
	    /// </summary>
	    /// <value>The parameter.</value>
	    public string FieldName
	    {
            get { return string.Empty; }
	    }

	    /// <summary>
        /// Returns an empty collection
        /// </summary>
	    public virtual IList Values
	    {
            get { return new ArrayList(); }
	    }

		/// <summary>
		/// Gets the conditions dependent from table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		public IList GetConditionsDependentFromTable(string table)
		{
			IList result = new ArrayList();

			for (int x = 0; x < AdditionalConditions.Length; x++)
				result.Add(AdditionalConditions[x].GetConditionsDependentFromTable(table));

			return result;
		}

        /// <summary>
        /// Gets the condition clause.
        /// </summary>
        /// <value>The condition clause.</value>
	    public ConditionClause ConditionClause
	    {
            get { return conditionClause; }
            set { conditionClause = value; }
	    }

	    /// <summary>
	    /// Gets a value indicating whether [use bind parameter].
	    /// </summary>
	    /// <value><c>true</c> if [use bind parameter]; otherwise, <c>false</c>.</value>
        bool ICondition.UseBindParameter
	    {
	        set {  }
	    }

	    /// <summary>
	    /// Tells if bind parameters shall be used for a given value at the defined index
	    /// </summary>
	    /// <param name="valueIndex">index of the value</param>
	    public bool GetUseBindParamter(int valueIndex)
	    {
	        return false;
	    }

	    /// <summary>
		/// This method returns all conditions 
		/// </summary>
		public ICondition[] AdditionalConditions
		{
			get { return additionalConditions; }
		}

		#endregion
	}
}