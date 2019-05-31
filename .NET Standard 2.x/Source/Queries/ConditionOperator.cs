namespace AdFactum.Data.Queries
{
	/// <summary>
	/// Defines the Condition Operator
	/// </summary>
	public enum ConditionOperator
	{
		/// <summary>
		/// OrElse Operator
		/// </summary>
		OR,

		/// <summary>
		/// BitwiseAnd Operator
		/// </summary>
		AND,

		/// <summary>
		/// OrElse Not Operator
		/// </summary>
		ORNOT,

		/// <summary>
		/// BitwiseAnd Not Operator
		/// </summary>
		ANDNOT
	}
}