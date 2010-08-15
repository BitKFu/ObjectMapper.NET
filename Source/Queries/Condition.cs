using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// Defines a condition
	/// </summary>
	public class Condition : ICondition
	{
		#region STATICS

		/// <summary>
		/// SCHEMA_REPLACE
		/// </summary>
		public const string SCHEMA_REPLACE = "#SR#";

		/// <summary>
		/// UPPER FUNCTION REPLACE
		/// </summary>
		public const string UPPER = "#UP#";

		/// <summary>
		/// TRIM FUNCTION REPLACE
		/// </summary>
		public const string TRIM = "#TR#";

		/// <summary>
		/// QuoteTable OPEN
		/// </summary>
		public const string QUOTE_OPEN = "[-";

		/// <summary>
		/// QuoteTable Close
		/// </summary>
		public const string QUOTE_CLOSE = "-]";

		#endregion

		/// <summary>
		/// NestedCondition 
		/// </summary>
		public const string NestedCondition = "#NC#";

		/// <summary>
		/// ParameterValue
		/// </summary>
        public const string ParameterValue = "#PV#";

		/// <summary>
		/// SCHEMA_REPLACE
		/// </summary>
        public const string GlobalJoin = "#GJ#";

        /// <summary>
        /// Used to replace the WHERE 
        /// </summary>
	    public const string WhereCondition = "#WC#";

		/// <summary>
		/// Type of the condition
		/// </summary>
		private readonly ConditionOperator type;

		/// <summary>
		/// Additional Conditions which are not a primary part of the condition
		/// </summary>
		private ICondition[] additionalConditions;

		/// <summary>
		/// Defines the compare Operator
		/// </summary>
		private QueryOperator compareOperator;

		/// <summary>
		/// Type of the object which will be queried
		/// </summary>
		private string tableName = "";

		/// <summary>
		/// Type of the object which will be queried
		/// </summary>
		private string rightSideTableName = "";

	    /// <summary>
	    /// True, if bind parameter shall be used.
	    /// </summary>
	    private bool useBindParameter = true;

	    /// <summary>
	    /// Defines the condition clause 
	    /// </summary>
        private ConditionClause conditionClause = ConditionClause.WhereClause;
		
		/// <summary>
		/// Getter for the condition type
		/// </summary>
		public ConditionOperator Type
		{
			get { return type; }
		}

		/// <summary>
		/// Getter for the condition type
		/// </summary>
		public QueryOperator CompareOperator
		{
			get { return compareOperator; }
            set { compareOperator = value;  }
		}

		/// <summary>
		/// Returns the compare Operator as a string value
		/// </summary>
		public virtual string Comparer
		{
			get
			{
				string result;

				switch (compareOperator)
				{
					case QueryOperator.Equals:
						result = "=";
						break;

					case QueryOperator.NotEqual:
						result = "<>";
						break;

					case QueryOperator.Greater:
						result = ">";
						break;

					case QueryOperator.GreaterEqual:
						result = ">=";
						break;

					case QueryOperator.Is:
						result = " IS ";
						break;

					case QueryOperator.IsNot:
						result = " IS NOT ";
						break;

					case QueryOperator.Lesser:
						result = "<";
						break;

					case QueryOperator.LesserEqual:
						result = "<=";
						break;

					case QueryOperator.In:
						result = " IN ";
						break;

					case QueryOperator.Like:
					case QueryOperator.Like_NoCaseSensitive:
						result = " LIKE ";
						break;

					case QueryOperator.NotIn:
						result = " NOT IN ";
						break;

					case QueryOperator.NotLike:
					case QueryOperator.NotLike_NoCaseSensitive:
						result = " NOT LIKE ";
						break;

					default:
						throw new NotSupportedException("Unkown operator " + compareOperator);
				}

				return result;
			}
		}


		/// <summary>
		/// Field 
		/// </summary>
		private readonly Field field;

		/// <summary>
		/// Getter for the field
		/// </summary>
		public Field Field
		{
			get { return field; }
		}

        /// <summary> Gets the parameter. </summary>
	    public string FieldName
	    {
            get { return field != null ? field.Name : string.Empty; }
	    }

		/// <summary>
		/// Field 
		/// </summary>
		private readonly Field rightSideField;

		/// <summary>
		/// Getter for the right side field
		/// </summary>
		public Field RightSideField
		{
			get { return rightSideField; }
		}

		/// <summary>
		/// True if the right side field is defined
		/// </summary>
		public bool IsRightSideFieldDefined
		{
			get { return rightSideField != null; }
		}


		#region Simple Constructors 

        /// <summary>
        /// Protected Constructor can be used by derived condition classes.
        /// </summary>
        protected Condition()
        {
        }

		/// <summary>
		/// Constructs a simple condition for a defined field object. By default the condition operator is set to AND and the
		/// compare operator is set to EQUALS.
		/// </summary>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(Field pField)
		{
			field = pField;
			type = ConditionOperator.AND;
			compareOperator = QueryOperator.Equals;
            tableName = Table.GetTableInstance(field.FieldDescription).DefaultName;

			AdjustCondition ();
		}

		/// <summary>
		/// Constructs a simple condition for a defined field object that uses a special condition operator. By default the compare
		/// operator is set TO EQUALS.
		/// </summary>
		/// <param name="pType">This parameter defines, whether the Condition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(ConditionOperator pType, Field pField)
		{
			type = pType;
			field = pField;
			compareOperator = QueryOperator.Equals;
            tableName = Table.GetTableInstance(field.FieldDescription).DefaultName;

			AdjustCondition ();
		}

		/// <summary>
		/// Constructs a simple condition for a defined field object.
		/// </summary>
		/// <param name="pType">This parameter defines, whether the Condition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="pCompareOperator">This parameter defines the query operator, which can be set to one of the values that represents a query operator. E.g. Equals, Lesser, Greater and so on.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(ConditionOperator pType, QueryOperator pCompareOperator, Field pField)
		{
			type = pType;
			field = pField;
			compareOperator = pCompareOperator;
            tableName = Table.GetTableInstance(field.FieldDescription).DefaultName;

			AdjustCondition ();
		}

		#endregion

		#region Constructors with Type objects

		/// <summary>
		/// Constructs a simple condition for a defined field object.
		/// </summary>
		/// <param name="pType">This parameter defines, whether the Condition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="pCompareOperator">This parameter defines the query operator, which can be set to one of the values that represents a query operator. E.g. Equals, Lesser, Greater and so on.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		/// <param name="pRightField">The field parameter defines the value that has to be compared.</param>
		public Condition(ConditionOperator pType, QueryOperator pCompareOperator, Field pField, Field pRightField)
		{
			type = pType;
			field = pField;
			rightSideField = pRightField;
			compareOperator = pCompareOperator;

            tableName = Table.GetTableInstance(Field.FieldDescription).DefaultName;
            rightSideTableName = Table.GetTableInstance(RightSideField.FieldDescription).DefaultName;

			AdjustCondition ();
		}

		#endregion

		#region Constructors with Tablenames

		/// <summary>
		/// Constructs a simple condition for a defined field object. By default the condition operator is set to AND and the
		/// compare operator is set to EQUALS. 
		/// </summary>
		/// <param name="pTableName">Instead of using a type object to extract the name of the table. The name of the table can be set directly using the table name parameter.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(string pTableName, Field pField)
		{
			field = pField;
			type = ConditionOperator.AND;
			compareOperator = QueryOperator.Equals;
			tableName = pTableName;

			AdjustCondition ();
		}

		/// <summary>
		/// Constructs a simple condition for a defined field object that uses a special condition operator. By default the compare
		/// operator is set To EQUALS.
		/// </summary>
		/// <param name="pTableName">Instead of using a type object to extract the name of the table. The name of the table can be set directly using the table name parameter.</param>
		/// <param name="pType">This parameter defines, whether the Condition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(string pTableName, ConditionOperator pType, Field pField)
		{
			type = pType;
			field = pField;
			compareOperator = QueryOperator.Equals;
			tableName = pTableName;

			AdjustCondition ();
		}

		/// <summary>
		/// Constructs a simple condition for a defined field object.
		/// </summary>
		/// <param name="pTableName">Instead of using a type object to extract the name of the table. The name of the table can be set directly using the table name parameter.</param>
		/// <param name="pType">This parameter defines, whether the Condition is connected with an "AND" or and "OR" condition to the rest of the query.</param>
		/// <param name="pCompareOperator">This parameter defines the query operator, which can be set to one of the values that represents a query operator. E.g. Equals, Lesser, Greater and so on.</param>
		/// <param name="pField">The field parameter defines the value that has to be compared.</param>
		public Condition(string pTableName, ConditionOperator pType, QueryOperator pCompareOperator, Field pField)
		{
			type = pType;
			field = pField;
			compareOperator = pCompareOperator;
			tableName = pTableName;

			AdjustCondition ();
		}

		#endregion

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition	
		/// </summary>
		public Set Tables
		{
			get
			{
				Set result = new Set();
				string sideTableName = TableName;

				if (sideTableName.Length != 0)
				{
					result.Add(sideTableName);
					
					// add right table name too
					sideTableName = RightSideTableName;
					if(sideTableName != null) result.Add(sideTableName);

					for (int x = 0; x < AdditionalConditions.Length; x++)
						result.Merge(AdditionalConditions[x].Tables);
				}
				return result;
			}
		}

		/// <summary>
		/// Gets the conditions dependent from table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns></returns>
		public virtual IList GetConditionsDependentFromTable (string table)
		{
			IList result = new ArrayList();
			
			if (TableName.Equals(table))
				result.Add(this);

			for (int x = 0; x < AdditionalConditions.Length; x++)
				result.Add(AdditionalConditions[x].GetConditionsDependentFromTable(table));

			return result;
		}

		/// <summary>
		/// Returns the table name if specified
		/// </summary>
		public string TableName
		{
			get
			{
				if (Field.FieldDescription is VirtualFieldDescription)
				{
					VirtualFieldDescription vfd = (VirtualFieldDescription) Field.FieldDescription;
                    return vfd.JoinTable.DefaultName;
				}

				return tableName;
			}
			set { tableName = value; }
		}

		/// <summary>
		/// Returns the column name
		/// </summary>
		public string ColumName
		{
			get
			{
                if (Field == null)
                    return string.Empty;

                string columnName;
                if (Field.FieldDescription is VirtualFieldDescription)
				{
					VirtualFieldDescription vfd = (VirtualFieldDescription) Field.FieldDescription;
                    columnName = vfd.JoinTable.DefaultName + "." + vfd.ResultField.Name;
				}
				else
				{
					if (tableName.Length != 0)
						columnName = string.Concat(QUOTE_OPEN, tableName, QUOTE_CLOSE,  "." , QUOTE_OPEN, Field.Name, QUOTE_CLOSE);
					else
						columnName = string.Concat(QUOTE_OPEN, Field.Name, QUOTE_CLOSE);
				}

				return columnName;
			}
		}



		/// <summary>
		/// Returns the table name if specified
		/// </summary>
		public string RightSideTableName
		{
			get
			{
				if(!IsRightSideFieldDefined) return null;

				if (RightSideField.FieldDescription is VirtualFieldDescription)
				{
					VirtualFieldDescription vfd = (VirtualFieldDescription) RightSideField.FieldDescription;
                    return vfd.JoinTable.DefaultName;
				}

				return rightSideTableName;
			}
			set { rightSideTableName = value; }
		}


		/// <summary>
		/// Right Column Name
		/// </summary>
		public string RightSideColumName
		{
			get
			{
				if(!IsRightSideFieldDefined) return null;

				string columnName;

				if (RightSideField.FieldDescription is VirtualFieldDescription)
				{
					VirtualFieldDescription vfd = (VirtualFieldDescription) RightSideField.FieldDescription;
                    columnName = vfd.JoinTable.DefaultName + "." + vfd.ResultField.Name;
				}
				else
				{
					if (rightSideTableName.Length != 0)
						columnName = rightSideTableName + "." + RightSideField.Name;
					else
						columnName = RightSideField.Name;
				}

				return columnName;
			}
		}


		#region ICondition Members

		/// <summary>
		/// Returns the additional conditions
		/// </summary>
		public ICondition[] AdditionalConditions
		{
			get
			{
				if (additionalConditions == null)
				{
					/*
					 * If virtual field
					 */
					if (Field.FieldDescription is VirtualFieldDescription)
					{
						VirtualFieldDescription vfd = (VirtualFieldDescription) Field.FieldDescription;
                        string targetClassTypeName = vfd.JoinTable.DefaultName;

						/*
						 * Add a join to the child table
						 */
						additionalConditions = new ICondition[(vfd.GlobalJoinField != null)?2:1];
					    additionalConditions[0] = new Join(vfd.CurrentTable.DefaultName, vfd.CurrentJoinField.Name, targetClassTypeName, vfd.TargetJoinField.Name);

						/*
						 * Add a localization constraint
						 */
						if (vfd.GlobalJoinField != null)
							additionalConditions[1] = new Condition(
                                targetClassTypeName,
								new Field(vfd.GlobalJoinField, vfd.GlobalParameter));
					}
					else
						additionalConditions = new ICondition[0];
				}

				return additionalConditions;
			}
		}

		/// <summary>
		/// Adds a field to the conditions
		/// </summary>
		/// <param name="condition">field condition</param>
		public void Add(ICondition condition)
		{
			ICondition[] tempConditions = AdditionalConditions;
			ICondition[] newConditions = new ICondition[tempConditions.Length + 1];

			tempConditions.CopyTo(newConditions, 0);
			newConditions[tempConditions.Length] = condition;

			additionalConditions = newConditions;
		}

		/// <summary>
		/// Add several conditions
		/// </summary>
		/// <param name="Add">field conditions</param>
		public void Add(ICondition[] Add)
		{
			ICondition[] tempConditions = AdditionalConditions;
			ICondition[] newConditions = new ICondition[tempConditions.Length + Add.Length];

			tempConditions.CopyTo(newConditions, 0);
			Add.CopyTo(newConditions, tempConditions.Length);

			additionalConditions = newConditions;
		}

        /// <summary>
        /// Gets the context dependent condition string.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <returns></returns>
        public virtual string GetContextDependentConditionString (ProjectionClass projection)
        {
            if (field == null) 
                return ConditionString;

            MemberProjectionTupel mpt;
            projection.MemberProjections.TryGetValue(field.FieldDescription.CustomProperty.Key, out mpt);

            return GetConditionString(mpt != null ? mpt.WhereColumn : ColumName);
        }

        /// <summary>
        /// Gets the context dependent condition clause
        /// </summary>
        /// <param name="projection"></param>
        /// <returns></returns>
        public virtual ConditionClause GetContextDependentConditionClause (ProjectionClass projection)
        {
            if (field == null || ConditionClause != ConditionClause.WhereClause)
                return ConditionClause;

            MemberProjectionTupel mpt;
            projection.MemberProjections.TryGetValue(field.FieldDescription.CustomProperty.Key, out mpt);

            return mpt == null || (mpt.MemberAggregation == null && mpt.MemberGrouping == null)
                       ? ConditionClause.WhereClause
                       : ConditionClause.HavingClause;
        }

        /// <summary>
        /// Conditions the string.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        protected virtual string GetConditionString(string columnName)
        {
            StringBuilder result = new StringBuilder();

            if (!IsRightSideFieldDefined) // in case we dont have right value
            {
                object curValue = Field.Value;
                if ((curValue == null) || (curValue.Equals(DBNull.Value)))
                {
                    if (compareOperator == QueryOperator.IsNot)
                        result.Append(string.Concat(columnName, " IS NOT NULL"));
                    else
                        result.Append(string.Concat(columnName, " IS NULL"));
                }
                else
                {
                    if ((compareOperator == QueryOperator.Like_NoCaseSensitive)
                        || (compareOperator == QueryOperator.NotLike_NoCaseSensitive))
                        result.Append(string.Concat(UPPER, "(", TRIM, "(", columnName, "))", Comparer, UPPER, "(", TRIM, "(", ParameterValue, "))"));
                    else
                        result.Append(string.Concat(columnName, Comparer, ParameterValue));
                }
            }
            else
            {
                if ((compareOperator == QueryOperator.Like_NoCaseSensitive)
                    || (compareOperator == QueryOperator.NotLike_NoCaseSensitive))
                    result.Append(string.Concat(UPPER, "(", TRIM, "(", columnName, "))", Comparer, UPPER, "(", TRIM, "(", RightSideColumName, "))"));
                else
                    result.Append(string.Concat(columnName, Comparer, RightSideColumName));
            }

            /*
             * Add additional conditions
             */
            for (int x = 0; x < AdditionalConditions.Length; x++)
                result.Append(NestedCondition);

            return result.ToString();
        }

	    /// <summary>
		/// Returns the string representation of the object
		/// </summary>
		public string ConditionString
		{
			get
			{
			    return GetConditionString(ColumName);
			}
		}

	    /// <summary>
	    /// True, if bind parameter shall be used.
	    /// </summary>
	    public bool UseBindParameter
	    {
	        get { return useBindParameter; }
	        set { useBindParameter = value; }
	    }

	    #endregion

		/// <summary>
		/// Adjust the equal Operator to is, if the queried value is null.
		/// </summary>
		private void AdjustCondition ()
        {
            conditionClause = Field.FieldDescription.CustomProperty.MetaInfo.IsProjected ? ConditionClause.HavingClause : ConditionClause.WhereClause;

			if (!IsRightSideFieldDefined && (field != null) && (field.Value == null))
			{
				switch (CompareOperator)
				{
					case QueryOperator.Equals :
						compareOperator = QueryOperator.Is;
						break;

					case QueryOperator.NotEqual:
						compareOperator = QueryOperator.IsNot;
						break;
				}
			}
		}

        /// <summary>
        /// Returns the query values
        /// </summary>
        /// <returns></returns>
        public virtual IList Values 
        {
            get
            {
                ArrayList values = new ArrayList();
                values.Add(Field.Value);
                return values;
            }
        }

        /// <summary>
        /// Return the Clause Type where the condition resists
        /// </summary>
        /// <value>The condition clause.</value>
        public ConditionClause ConditionClause
	    {
	        get { return conditionClause; }
            set { conditionClause = value; }
	    }
	}
}