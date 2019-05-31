using System;
using System.Collections;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// Sometimes it's necessary to do a selection from a sub selection or a databse view. 
	/// In that case you have to use the table replacement condition. With this condition you can force 
	/// the AdFactum object mapper to select the data not from the known table, but from an other 
	/// source that can be specified in a table replacement condition.
	/// </summary>
	public class TableReplacement : ConditionList
	{
		/// <summary>
		/// Tablename that shall be replaced
		/// </summary>
		private readonly string	   tableToReplace;

		/// <summary>
		/// Condition which is used to replace the tablename in the from clause
		/// </summary>
		private readonly ICondition subCondition;

		/// <summary>
		/// View Condition, used to replace a tablename with a view
		/// </summary>
		private class ViewCondition : ConditionList
		{
		    readonly string viewName;

			/// <summary>
			/// Constructor to create a view condition
			/// </summary>
			/// <param name="_viewName"></param>
			public ViewCondition (string _viewName)
			{
                ConditionClause = ConditionClause.WhereClause;
                viewName = _viewName;
			}

			/// <summary>
			/// Returns the View Name as a condition that is set into the from clause.
			/// </summary>
			public override string ConditionString
			{
				get
				{
					return viewName;
				}
			}

		}


		/// <summary>
		/// Constructor for the table replacement
		/// </summary>
		/// <param name="_subSelect">This parameter defines sub selection. With this parameter you can replace a tablename with the select statement. Please keep in mind to define a "*" as the resultRow for the sub selection. Otherwise the selection will fail, because the expected columns can't be found in the sub selection.</param>
        public TableReplacement(SubSelect _subSelect)
		{
            ConditionClause = ConditionClause.FromClause;
            subCondition = _subSelect;
			tableToReplace = _subSelect.ResultTable;
		}

		/// <summary>
		/// Constructor for the table replacement
		/// </summary>
		/// <param name="_subSelect">This parameter defines sub selection. With this parameter you can replace a tablename with the select statement. Please keep in mind to define a "*" as the resultRow for the sub selection. Otherwise the selection will fail, because the expected columns can't be found in the sub selection.</param>
		/// <param name="_tableToReplace">That's the real database tablename which shall be replaced with the second constructor parameter.</param>
        public TableReplacement(string _tableToReplace, ICondition _subSelect)
		{
            ConditionClause = ConditionClause.FromClause;
            subCondition = _subSelect;
			tableToReplace = _tableToReplace;
		}

		/// <summary>
		/// Constructor for the table replacement
		/// </summary>
		/// <param name="_subSelect">This parameter defines sub selection. With this parameter you can replace a tablename with the select statement. Please keep in mind to define a "*" as the resultRow for the sub selection. Otherwise the selection will fail, because the expected columns can't be found in the sub selection.</param>
		/// <param name="_tableToReplace">The real database tablename will be taken out of the Type in order to get a database independence.</param>
		public TableReplacement(Type _tableToReplace, ICondition _subSelect)
		{
            ConditionClause = ConditionClause.FromClause;
            subCondition = _subSelect;
            tableToReplace = Table.GetTableInstance(_tableToReplace).DefaultName;
		}

		/// <summary>
		/// Constructor to replace a table with a view
		/// </summary>
		/// <param name="_tableToReplace">That's the real database tablename which shall be replaced with the second constructor parameter.</param>
		/// <param name="_viewName">The table name within the FROM clause will be replaced by the view name parameter.</param>
		public TableReplacement(string _tableToReplace, string _viewName)
		{
            ConditionClause = ConditionClause.FromClause;
            subCondition = new ViewCondition(_viewName);
			tableToReplace = _tableToReplace;
		}

		/// <summary>
		/// Constructor to replace a table with a view
		/// </summary>
		/// <param name="_tableToReplace">The real database tablename will be taken out of the Type in order to get a database independence.</param>
		/// <param name="_viewName">The table name within the FROM clause will be replaced by the view name parameter.</param>
		public TableReplacement(Type _tableToReplace, string _viewName)
		{
            ConditionClause = ConditionClause.FromClause;
            tableToReplace = Table.GetTableInstance(_tableToReplace).DefaultName;
			subCondition = new ViewCondition(_viewName);
		}

		/// <summary>
		/// Returns the table override
		/// </summary>
		public override Set Tables
		{
			get
			{
				Set tables = base.Tables;
				tables.Add(tableToReplace, subCondition);

				return tables;
			}
		}

        /// <summary>
        /// Returns the values
        /// </summary>
        public override IList Values
	    {
            get
            {
                return subCondition != null ? subCondition.Values : null;
            }
	    }

    }
}