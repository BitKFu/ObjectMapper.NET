using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Util;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// A InCondition handles comparings with sub selects. 
	/// Imagine that you want to develop a blogger system. Every user can write new articals and put it on the system. 
	/// On your mainpage you want to show all bloggers who wrote an artical this day. How would you do this?
	/// Sure - with an InCondition.
	/// </summary>
	/// <example>
	/// SubSelect  bloggerIds = new SubSelect (typeof(Blog), "Blogger", 
	///	new AndCondition(typeof(Blog), "EntryTime", QueryOperator.Equals, DateTime.Today));
	///
	///	IList bloggers = mapper.FlatSelect (typeof(User), 
	///	new InCondition (typeof(User), "Id",  bloggerIds));
	/// </example>
	public class InCondition : Condition
	{
	    /// <summary>
	    /// Sub Selection for IDs
	    /// </summary>
	    private readonly SubSelect subSelect;

        /// <summary>
        /// Union Selection for IDs
        /// </summary>
        private readonly Union unionSelect;

        /// <summary>
        /// Value List
        /// </summary>
	    private readonly IList valueList;
        
        /// <summary>
		/// The InCondition constructor is used for building IN Constructs
		/// </summary>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="subSelectParam">This parameter handles a special subselect which represents the compare value of the InCondition.</param>
        public InCondition(Type queriedType, string field, SubSelect subSelectParam)
			: base(
				ConditionOperator.AND,
				QueryOperator.In,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
		{
            subSelect = subSelectParam;
		}

		/// <summary>
		/// The InCondition constructor is used for building IN Constructs
		/// </summary>
		/// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
		/// <param name="subSelectParam">This parameter handles a special subselect which represents the compare value of the InCondition.</param>
		public InCondition(ConditionOperator conditionOperator, Type queriedType, string field, SubSelect subSelectParam)
			: base(
				conditionOperator,
				QueryOperator.In,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
		{
            subSelect = subSelectParam;
        }

		/// <summary>
		/// The InCondition constructor is used for building IN Constructs
		/// </summary>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="unionSelectParam">This parameter handles a Union Select. A Union Select is a collection of SubSelects.</param>
        public InCondition(Type queriedType, string field, Union unionSelectParam)
			: base(
				ConditionOperator.AND,
				QueryOperator.In,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
		{
		    unionSelect = unionSelectParam;
		}

		/// <summary>
		/// The InCondition constructor is used for building IN Constructs
		/// </summary>
		/// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
		/// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="unionSelectParam">This parameter handles a Union Select. A Union Select is a collection of SubSelects.</param>
        public InCondition(ConditionOperator conditionOperator, Type queriedType, string field, Union unionSelectParam)
			: base(
				conditionOperator,
				QueryOperator.In,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
		{
            unionSelect = unionSelectParam;
        }

        /// <summary>
        /// The InCondition constructor is used for building IN Constructs
        /// </summary>
        /// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
        /// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="valueListParameter">The value list parameter.</param>
        public InCondition(Type queriedType, string field, IList valueListParameter)
            : base(
                ConditionOperator.AND,
                QueryOperator.In,
                new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
        {
            valueList = valueListParameter;
        }

        /// <summary>
        /// The InCondition constructor is used for building IN Constructs
        /// </summary>
        /// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
        /// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="valueListParameter">The value list parameter.</param>
        public InCondition(Type queriedType, string field, params object[] valueListParameter)
            : base(
                ConditionOperator.AND,
                QueryOperator.In,
                new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
        {
            valueList = valueListParameter;
        }

        /// <summary>
        /// The InCondition constructor is used for building IN Constructs
        /// </summary>
        /// <param name="conditionOperator">This parameter defines, whether the InCondition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
        /// <param name="queriedType">This is the type of an object which shall be queried. E.g. If you want to make a query like "User.Id in ...". The queried object type is the class User.</param>
        /// <param name="field">This is the queried name of the property field within the queriedType. Think about our example "User.Id in ...". The queried field name is "Id".</param>
        /// <param name="valueListParameter">The value list parameter.</param>
        public InCondition(ConditionOperator conditionOperator, Type queriedType, string field, IList valueListParameter)
            : base(
                conditionOperator,
                QueryOperator.In,
                new Field(ReflectionHelper.GetStaticFieldTemplate(queriedType, field), null))
        {
            valueList = valueListParameter;
        }

        /// <summary>
        /// Accessor for SubSelects
        /// </summary>
        public IList SubSelects
        {
            get
            {
                if (subSelect != null)
                {
                    ArrayList result = new ArrayList();
                    result.Add(subSelect);
                    return result;
                }

                if (unionSelect != null)
                    return unionSelect.SubSelects;

                return null;
            }
        }

        /// <summary>
        /// Returns the string representation of the object
        /// </summary>
        /// <value></value>
        protected override string GetConditionString(string columnName)
        {
            StringBuilder result = new StringBuilder();

            /*
             * Try to build an In-Condition with Sub Selects
             */
            IList subSelects = SubSelects;
            if (subSelects != null)
            {
                for (int counter = 1; counter <= subSelects.Count; counter++)
                {
                    SubSelect select = (SubSelect) subSelects[counter - 1];

                    if (counter == 1)
                        result.Append(
                            string.Concat(
                                QUOTE_OPEN, TableName, QUOTE_CLOSE, ".",
                                QUOTE_OPEN, Field.Name, QUOTE_CLOSE,
                                Comparer, " ("));

                    result.Append(select.ConditionString);

                    if (counter < subSelects.Count)
                        result.Append(string.Concat(" ",unionSelect.Connector," "));
                }

                result.Append(" )");
            }

            /*
             * Try to build an In-Condition with values
             */
            if (valueList != null)
            {
                bool first = true;
                for (int counter = 0; counter < valueList.Count; counter++ )
                {
                    if (first)
                        result.Append(
                            string.Concat(
                                QUOTE_OPEN, TableName, QUOTE_CLOSE, ".",
                                QUOTE_OPEN, Field.Name, QUOTE_CLOSE,
                                Comparer, " ("));
                    else
                        result.Append(", ");

                    result.Append(ParameterValue);
                    first = false;
                }
                result.Append(")");
            }

            /*
             * Add additional conditions
             */
            for (int x = 0; x < AdditionalConditions.Length; x++)
                result.Append(NestedCondition);

            return result.ToString();
        }

        /// <summary>
        /// Returns the query values
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public override IList Values
        {
            get
            {
                return valueList;
            }
        }
    }
}