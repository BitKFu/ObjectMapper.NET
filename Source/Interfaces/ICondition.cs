using System.Collections;
using AdFactum.Data.Queries;
using AdFactum.Data.Internal;

namespace AdFactum.Data
{
	/// <summary>
	/// Interface for ICondition
	/// </summary>
	public interface ICondition
	{
		/// <summary>
		/// Returns all additional Conditions that belongs to the condition
		/// </summary>
		/// <value>The additional conditions.</value>
		ICondition[] AdditionalConditions { get; }

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition
		/// </summary>
		/// <value>The tables.</value>
		Set Tables { get; }

		/// <summary>
		/// Adds a field to the conditions
		/// </summary>
		/// <param name="condition">field condition</param>
		void Add(ICondition condition);

		/// <summary>
		/// Add several conditions
		/// </summary>
		/// <param name="Add">field conditions</param>
		void Add(ICondition[] Add);

		/// <summary>
		/// Getter for the Condition type
		/// </summary>
		/// <value>The type.</value>
		ConditionOperator Type { get; }

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <value>The condition string.</value>
		string ConditionString { get; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        string FieldName { get; }

        /// <summary>
        /// Returns the condition parameter values
        /// </summary>
        IList Values { get;}

		/// <summary>
		/// Gets the conditions dependent from table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		IList GetConditionsDependentFromTable (string table);

        /// <summary>
        /// Gets the condition clause.
        /// </summary>
        /// <value>The condition clause.</value>
        ConditionClause ConditionClause { get; set;  }

        /// <summary>
        /// Gets a value indicating whether [use bind parameter].
        /// </summary>
        /// <value><c>true</c> if [use bind parameter]; otherwise, <c>false</c>.</value>
        bool UseBindParameter { get; set;}
	}
}