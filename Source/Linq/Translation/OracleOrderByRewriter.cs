using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Used to rewrite Oracle specific order by clause
    /// </summary>
    public class OracleOrderByRewriter : OrderByRewriter
    {
        readonly Stack<SelectExpression> callStack = new Stack<SelectExpression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleOrderByRewriter"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        private OracleOrderByRewriter(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression)
        {
            var writer = new OracleOrderByRewriter(expression);
            return writer.Visit(expression);
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            callStack.Push(select);
            select = (SelectExpression)base.VisitSelectExpression(select);
            callStack.Pop();

            bool isOuterMostSelect = callStack.Count == 0; 
            var surrounding = callStack.Count > 0 ? callStack.Peek() : null;

            var surContainsRowNum = surrounding != null
                                        ? (surrounding.Where != null && TypeChecker.Contains(DbExpressionType.RowNum, surrounding.Where))
                                        : false;

            bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
            bool hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
            bool containsRowNum = select.Where != null && TypeChecker.Contains(DbExpressionType.RowNum, select.Where);
            bool containsAggregates = AggregateChecker.HasAggregates(select);
            bool canHaveOrderBy = !(select.Take != null || select.Skip != null) && !containsRowNum;
            bool canReceiveOrderings = canHaveOrderBy && !hasGroupBy && !select.IsDistinct && !containsAggregates;

            var orderByColumns = new List<OrderExpression>();

            if (select.IsReverse)
                ReverseOrderings();

            bool canPassOnOrderings = !isOuterMostSelect && !hasGroupBy && !select.IsDistinct && !surContainsRowNum;

            // maybe we already gathered some orderings, than append them now
            if (canReceiveOrderings && GatheredOrderings.Count > 0 && !canPassOnOrderings)
            {
                GatheredOrderings = BindToSelection(select, GatheredOrderings);

                if (hasOrderBy) orderByColumns.AddRange(select.OrderBy);
                if (GatheredOrderings != null) orderByColumns.AddRange(GatheredOrderings);

                // Return order
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From, select.Where,
                                            orderByColumns.Count > 0 ? new ReadOnlyCollection<OrderExpression>(orderByColumns) : null,
                                            select.GroupBy, select.Skip, select.Take, select.IsDistinct, false, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }

            // if we can't pass on the ordering, we something have to do with it
            if (!canPassOnOrderings && canHaveOrderBy)
            {
                GatheredOrderings = new List<OrderExpression>();
                return select.IsReverse ? select.SetReverse(false) : select;
            }

            // if the current expression has an order by, than gather it
            if (select.OrderBy != null)
            {
                // Check if the current ordering, does not exisit in the list
                if (!GatheredOrderings.Any(x => select.OrderBy.Any(o => DbExpressionComparer.AreEqual(o.Expression, x.Expression))))
                {
                    GatheredOrderings.AddRange(select.OrderBy);

                    // Try to update all gatheredOrderings to the current selection
                    GatheredOrderings = BindToSelection(select, GatheredOrderings);
                }

                // return without ordering
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where,null, select.GroupBy, select.Skip, select.Take, select.IsDistinct, false,
                                            select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }

            // Maybe we have to rebind the orderings
            if (GatheredOrderings.Count>0)
            {
                // Try to update all gatheredOrderings to the current selection
                GatheredOrderings = BindToSelection(select, GatheredOrderings);
            }

            return select;
        }
    }
}
