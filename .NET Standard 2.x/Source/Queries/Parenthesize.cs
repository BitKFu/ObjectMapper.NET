namespace AdFactum.Data.Queries
{
	/// <summary>
	/// In the Ad Factum object mapper you can use the Parenthesize Condition class in order to parenthesize conditions.
	/// </summary>
	public class Parenthesize : ConditionList
	{
		/// <summary>
		/// In the Ad Factum object mapper you can use the Parenthesize Condition class in order to parenthesize conditions.
		/// </summary>
		/// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="conditions">A Parenthesize class consists of one or more Conditions. The conditions parameter is used to put in the nested Condition objects.</param>
		public Parenthesize(ConditionOperator conditionOperator, params ICondition[] conditions)
			: base(conditionOperator, conditions)
		{
            ConditionClause = ConditionClause.WhereClause;
        }

		/// <summary>
		/// In the Ad Factum object mapper you can use the Parenthesize Condition class in order to parenthesize conditions.
		/// </summary>
		/// <param name="conditions">A Parenthesize class consists of one or more Conditions. The conditions parameter is used to put in the nested Condition objects.</param>
		public Parenthesize(params ICondition[] conditions)
			: base(conditions)
		{
            ConditionClause = ConditionClause.WhereClause;
        }

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		public override string ConditionString
		{
			get { return "(" + base.ConditionString + ")"; }
		}
	}
}