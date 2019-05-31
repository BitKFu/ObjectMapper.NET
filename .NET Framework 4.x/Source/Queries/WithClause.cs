using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Queries
{
    /// <summary>
    /// Defines a withclause
    /// </summary>
    public class WithClause : SubSelect
	{
	    /// <summary>
	    /// Name of the with clause
	    /// </summary>
        private string withClauseName;

        /// <summary>
        /// Basic Constructor to define a sub select with one result row for the IN operator
        /// </summary>
        /// <param name="withClauseName">Name of the with clause.</param>
        /// <param name="selectionType">This is the type of an object that shall be queried.</param>
        /// <param name="conditionsParameter">The condition parameter is used as a filter to decrease the results.</param>
        public WithClause(string withClauseName, Type selectionType, params ICondition[] conditionsParameter)
		{
            tableName = Table.GetTableInstance(selectionType).DefaultName;
            this.withClauseName = withClauseName;

            conditions = conditionsParameter;
		    if (conditions == null || conditions.Length==0) conditions = new ICondition[]{};

            rowName = "*";
		}


		#region ICondition Standard Members

		/// <summary>
		/// Returns the condition string
		/// </summary>
		public override string ConditionString
		{
			get
			{
				StringBuilder result = new StringBuilder();

			    result.Append("WITH " + WithClauseName + " AS (");
				result.Append("SELECT " + ResultTable + "." + ResultRow);
				result.Append(" FROM " );

				IEnumerator tableEnumerator = base.Tables.GetEnumerator();
				bool first = true;
				while (tableEnumerator.MoveNext())
				{
					Set.Tupel tupel = tableEnumerator.Current as Set.Tupel;
					if (!first) result.Append(", ");
					if (tupel != null) result.Append(tupel.TupelString());
					first = false;
				}

                if (AdditionalConditions.Length > 0)
				{
					result.Append(" WHERE ");

					for (int x = 0; x < AdditionalConditions.Length; x++)
						result.Append(Condition.NestedCondition);
				}
			    result.Append(") ");

				return result.ToString();
			}
		}

        /// <summary>
        /// Retruns a list containing the parameter values
        /// </summary>
        public override IList Values
        {
            get
            {
                IList result = new ArrayList();

                // Add values from tupels
                foreach (Set.Tupel tupel in base.Tables)
                    foreach (object value in tupel.Values)
                        result.Add(value);

                return result;
            }
        }

        /// <summary>
        /// Name of the with clause
        /// </summary>
        public string WithClauseName
        {
            get { return withClauseName; }
        }

        /// <summary>
        /// Returns the list of tables which are needed to fulfill the condition
        /// </summary>
        /// <value></value>
        public override Set Tables
        {
            get
            {
                // Expose a empty table set
                // This is for preventing the Parent SQL to use the tables of the With-Clause within the own From-Clause.
                return new Set();
            }
        }

        /// <summary>
        /// Gets the condition clause.
        /// </summary>
        /// <value>The condition clause.</value>
        public override ConditionClause ConditionClause
        {
            get
            {
                return ConditionClause.WithClause;
            }
        }

        #endregion
	}
}