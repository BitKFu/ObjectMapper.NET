using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class SqlOrderByRewriter : OrderByRewriter
    {
        readonly Stack<SelectExpression> callStack = new Stack<SelectExpression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlOrderByRewriter"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        private SqlOrderByRewriter(Expression expression, ExpressionVisitorBackpack backpack)
            : base(expression, backpack)
        {
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, ExpressionVisitorBackpack backpack)
        {
            var writer = new SqlOrderByRewriter(expression, backpack);
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

            bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
            bool hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
            bool canHaveOrderBy = isOuterMostSelect || select.Skip == null; 
            bool canReceiveOrderings = canHaveOrderBy && !hasGroupBy && !select.IsDistinct && !AggregateChecker.HasAggregates(select);

            var orderByColumns = new List<OrderExpression>();

            if (select.IsReverse)
                ReverseOrderings();

            bool canPassOnOrderings = !isOuterMostSelect && !hasGroupBy && !select.IsDistinct && select.Take == null;// && !surContainsRowNum;

            // maybe we already gathered some orderings, than append them now
            if (canReceiveOrderings && GatheredOrderings.Count > 0 && !canPassOnOrderings)
            {
                IEnumerable<OrderExpression> bindToSelection = BindToSelection(select, GatheredOrderings, Backpack);

                if (hasOrderBy) orderByColumns.AddRange(select.OrderBy);
                if (bindToSelection != null) orderByColumns.AddRange(bindToSelection);

                // Return order)
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
                if (!GatheredOrderings.Any(x=>select.OrderBy.Any(o=>DbExpressionComparer.AreEqual(o.Expression , x.Expression))))
                    GatheredOrderings.AddRange(select.OrderBy);

                // return without ordering
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, null, select.GroupBy, select.Skip, select.Take, select.IsDistinct, false,
                                            select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }

            return select;
        }


    }
}
