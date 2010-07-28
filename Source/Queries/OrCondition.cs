using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Util;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// The OrCondition is a basic condition to compare a simple field with a value. The condition itself is bound with an OR operator. 
	/// </summary>
	public class OrCondition : Condition
	{
		/// <summary>
		/// Constructor for building the OR Condition. 
		/// </summary>
		/// <param name="queriedObjectType">This parameter defines the object which is queried. E.g.: If you want to query the age of a employee the parameter would be the type of the employee object.</param>
		/// <param name="field">This parameter defines the name of a property of the queriedObjectType. In our example the field value would be the name of the "Age" property. Keep in mind, that this parameter is independent of the used database column name.</param>
		/// <param name="comparer">This parameter defines the query operator, which can be set to one of the values that represents a query operator. E.g. Equals, Lesser, Greater and so on.</param>
		/// <param name="compareValue">This parameter is the value that will be compared with the content of the database. The compareValue can be of any base .NET Framework type, like string, integer and so on. Furthermore it can be a object which implements the interface IValueObject or is derived from the object type ValueObject.</param>
		public OrCondition(Type queriedObjectType, string field, QueryOperator comparer, object compareValue)
			: base(
				ConditionOperator.OR,
				comparer,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedObjectType, field), compareValue))
		{
		}

		/// <summary>
		/// Constructor for building the OR Condition. Because the QueryOperator is mssing, the comparer is set to EQUAL, which is more or less the most used query operator.
		/// </summary>
		/// <param name="queriedObjectType">This parameter defines the object which is queried. E.g.: If you want to query the age of a employee the parameter would be the type of the employee object.</param>
		/// <param name="field">This parameter defines the name of a property of the queriedObjectType. In our example the field value would be the name of the "Age" property. Keep in mind, that this parameter is independent of the used database column name.</param>
		/// <param name="compareValue">This parameter is the value that will be compared with the content of the database. The compareValue can be of any base .NET Framework type, like string, integer and so on. Furthermore it can be a object which implements the interface IValueObject or is derived from the object type ValueObject.</param>
		public OrCondition(Type queriedObjectType, string field, object compareValue)
			: base(
				ConditionOperator.OR,
				QueryOperator.Equals,
				new Field(ReflectionHelper.GetStaticFieldTemplate(queriedObjectType, field), compareValue))
		{
		}
	}
}