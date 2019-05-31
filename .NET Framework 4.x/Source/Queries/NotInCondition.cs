using System;
using System.Collections;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// A InCondition handles comparings with sub selects. 
	/// Imagine that you want to develop a blogger system. Every user can write new articals and put it on the system. 
	/// On your mainpage you want to show all bloggers who did not write an artical this day. How would you do this?
	/// Sure - with an NotInCondition.
	/// </summary>
	public class NotInCondition : InCondition
	{
		/// <summary>
		/// The InCondition constructor is used for building NOT IN Constructs
		/// </summary>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="subSelect">This parameter handles a special subselect which represents the compare value of the InCondition.</param>
		public NotInCondition(Type queriedType, string field, SubSelect subSelect)
			: base(queriedType, field, subSelect)
		{
		    CompareOperator = QueryOperator.NotIn;
		}

		/// <summary>
		/// The InCondition constructor is used for building NOT IN Constructs
		/// </summary>
		/// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="subSelect">This parameter handles a special subselect which represents the compare value of the InCondition.</param>
		public NotInCondition(ConditionOperator conditionOperator, Type queriedType, string field, SubSelect subSelect)
			: base(conditionOperator, queriedType, field, subSelect)
		{
            CompareOperator = QueryOperator.NotIn;
        }

		/// <summary>
		/// The InCondition constructor is used for building NOT IN Constructs
		/// </summary>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="unionSelect">This parameter handles a Union Select. A Union Select is a collection of SubSelects.</param>
		public NotInCondition(Type queriedType, string field, Union unionSelect)
			: base(queriedType, field, unionSelect)
		{
            CompareOperator = QueryOperator.NotIn;
        }

		/// <summary>
		/// The InCondition constructor is used for building NOT IN Constructs
		/// </summary>
		/// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="unionSelect">This parameter handles a Union Select. A Union Select is a collection of SubSelects.</param>
		public NotInCondition(ConditionOperator conditionOperator, Type queriedType, string field, Union unionSelect)
			: base(conditionOperator, queriedType, field, unionSelect)
		{
            CompareOperator = QueryOperator.NotIn;
        }

        /// <summary>
        /// The InCondition constructor is used for building IN Constructs
        /// </summary>
        /// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
        /// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="valueListParameter">The value list parameter.</param>
        public NotInCondition(Type queriedType, string field, IList valueListParameter)
            :base(queriedType, field, valueListParameter)
        {
            CompareOperator = QueryOperator.NotIn;
        }

        /// <summary>
        /// The InCondition constructor is used for building IN Constructs
        /// </summary>
        /// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
        /// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="valueListParameter">The value list parameter.</param>
        public NotInCondition(Type queriedType, string field, params object[] valueListParameter)
            :base(queriedType, field, valueListParameter)
        {
            CompareOperator = QueryOperator.NotIn;
        }
	}
}