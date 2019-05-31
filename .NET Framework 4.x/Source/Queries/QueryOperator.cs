namespace AdFactum.Data.Queries
{
	/// <summary>
	/// Enumeration class to define the operators for the where clause
	/// </summary>
	public enum QueryOperator
	{
        /// <summary>
        /// Don't use a query operator
        /// </summary>
        Nothing,

		/// <summary>
		/// Equals compare operator 
		/// </summary>
		Equals,

		/// <summary>
		/// Not Equal compare operator 
		/// </summary>
		NotEqual,

		/// <summary>
		/// Lesser compare operator 
		/// </summary>
		Lesser,

		/// <summary>
		/// Greater compare operator 
		/// </summary>
		Greater,

		/// <summary>
		/// Lesser Equal compare operator 
		/// </summary>
		LesserEqual,

		/// <summary>
		/// Greater Equal compare operator 
		/// </summary>
		GreaterEqual,

		/// <summary>
		/// Is compare operator 
		/// </summary>
		Is,

		/// <summary>
		/// Is compare operator 
		/// </summary>
		IsNot,

		/// <summary>
		/// Defines a sub select comparer
		/// </summary>
		In,

		/// <summary>
		/// Defines a like operator
		/// </summary>
		Like,

		/// <summary>
		/// Defines a sub select comparer
		/// </summary>
		NotIn,

		/// <summary>
		/// Defines a not like operator
		/// </summary>
		NotLike,

		/// <summary>
		/// Defines a like that is not case sensitive
		/// </summary>
		Like_NoCaseSensitive,

		/// <summary>
		/// Defines a not like that is not case sensitive
		/// </summary>
		NotLike_NoCaseSensitive
	}
}