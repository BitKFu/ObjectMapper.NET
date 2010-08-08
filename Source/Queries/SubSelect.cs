using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// Defines a subSelect Query
	/// </summary>
	public class SubSelect : ICondition
	{
		/// <summary>
		/// Tablename
		/// </summary>
		protected string tableName;

		/// <summary>
		/// Field Description for the result row
		/// </summary>
        protected string rowName;

		/// <summary>
		/// Where Clause for selection
		/// </summary>
        protected ICondition[] conditions;

	    private ConditionClause conditionClause = Queries.ConditionClause.WhereClause;

        /// <summary>
        /// Result Type of the sub select
        /// </summary>
	    private readonly Type resultType;

	    /// <summary>
        /// Initializes a new instance of the <see cref="SubSelect"/> class.
        /// </summary>
        protected SubSelect()
        {
            
        }

		/// <summary>
		/// Basic Constructor to define a sub select with one result row for the IN operator
		/// </summary>
		/// <param name="selectionType">This is the type of an object that shall be queried. </param>
        /// <param name="resultRowParameter">A SubSelect clause only returns one column. That's enough information for the surrounding InCondition.</param>
        /// <param name="conditionsParameter">The condition parameter is used as a filter to decrease the results.</param>
		public SubSelect(Type selectionType, string resultRowParameter, params ICondition[] conditionsParameter)
		{
		    resultType = selectionType;
            tableName = Table.GetTableInstance(selectionType).Name;

            if (resultRowParameter != "*")
                rowName = ReflectionHelper.GetStaticFieldTemplate(selectionType, resultRowParameter).Name;
			else
                rowName = resultRowParameter;

            conditions = conditionsParameter;
		    if (conditions == null) conditions = new ICondition[]{};
		}


		/// <summary>
		/// Returns the tableName name of the result tableName
		/// </summary>
		public string ResultTable
		{
			get { return tableName; }
		}


		/// <summary>
		/// Getter for the Result Row
		/// </summary>
		public string ResultRow
		{
			get { return rowName; }
		}

		/// <summary>
		/// Returns all conditions of the where clause
		/// </summary>
		public ICondition[] AdditionalConditions
		{
			get { return conditions; }
		}

		/// <summary>
		/// Adds a field to the conditions
		/// </summary>
		/// <param name="condition">field condition</param>
		public void Add(ICondition condition)
		{
			ICondition[] newConditions = new ICondition[conditions.Length + 1];
			conditions.CopyTo(newConditions, 0);
			newConditions[conditions.Length] = condition;
			conditions = newConditions;
		}

		/// <summary>
		/// Add several conditions
		/// </summary>
		/// <param name="Add">field conditions</param>
		public void Add(ICondition[] Add)
		{
			ICondition[] newConditions = new ICondition[conditions.Length + Add.Length];
			conditions.CopyTo(newConditions, 0);
			Add.CopyTo(newConditions, conditions.Length);

			conditions = newConditions;
		}

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition
		/// </summary>
		public virtual Set Tables
		{
			get
			{
				Set result = new Set();
				result.Add(tableName);
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
			get { return ConditionOperator.AND; }
		}

		/// <summary>
		/// Returns the condition string
		/// </summary>
		public virtual string ConditionString
		{
			get
			{
				StringBuilder result = new StringBuilder();

                result.Append("SELECT " + Condition.QUOTE_OPEN + ResultTable + Condition.QUOTE_CLOSE + "." + Condition.QUOTE_OPEN + ResultRow + Condition.QUOTE_CLOSE);
				result.Append(" FROM " );

				IEnumerator tableEnumerator = Tables.GetEnumerator();
				bool first = true;
				while (tableEnumerator.MoveNext())
				{
					Set.Tupel tupel = tableEnumerator.Current as Set.Tupel;
                    if (tupel == null) continue;

					if (!first) result.Append(", ");
					result.Append(tupel.TupelString());
					first = false;
				}

                if (AdditionalConditions.Length > 0)
				{
				    result.Append(string.Concat(" ", Condition.WhereCondition, " "));

					for (int x = 0; x < AdditionalConditions.Length; x++)
						result.Append(Condition.NestedCondition);
				}

				return result.ToString();
			}
		}

	    /// <summary>
	    /// Gets the parameter.
	    /// </summary>
	    /// <value>The parameter.</value>
	    public string FieldName
	    {
            get { return ResultRow; }
	    }

	    /// <summary>
		/// Gets the conditions dependent from tableName.
		/// </summary>
		/// <param name="table">The tableName.</param>
		/// <returns></returns>
		public IList GetConditionsDependentFromTable(string table)
		{
			IList result = new ArrayList();
			
			if (TableName.Equals(table))
				result.Add(this);

		    for (int x = 0; x < AdditionalConditions.Length; x++)
			    result.Add(AdditionalConditions[x].GetConditionsDependentFromTable(table));

			return result;
		}

        /// <summary>
        /// Gets the condition clause.
        /// </summary>
        /// <value>The condition clause.</value>
	    public virtual ConditionClause ConditionClause
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
	        get { return false; }
	        set {  }
	    }


	    /// <summary>
        /// Returns the value list of all SubSelect conditions
        /// </summary>
	    public virtual IList Values
	    {
	        get
	        {
	            IList result = new ArrayList();

                foreach (ICondition addCondition in AdditionalConditions)
                    foreach (object value in addCondition.Values)
                        result.Add(value);

	            return result;
	        }
	    }

	    /// <summary>
		/// Gets the name of the tableName.
		/// </summary>
		/// <value>The name of the tableName.</value>
		private string TableName
		{
			get { return tableName; }
		}

	    /// <summary>
	    /// Result Type of the sub select
	    /// </summary>
	    public Type ResultType
	    {
	        get { return resultType; }
	    }

	    #endregion
	}
}