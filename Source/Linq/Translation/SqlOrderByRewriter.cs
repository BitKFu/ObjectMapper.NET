using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        private SqlOrderByRewriter(Expression expression) : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression)
        {
            var writer = new SqlOrderByRewriter(expression);
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

            var surrounding = callStack.Count > 0 ? callStack.Peek() : null;
            bool saveIsOuterMostSelect = surrounding == Root;
            bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
            bool hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
            bool canHaveOrderBy = saveIsOuterMostSelect || select.Take != null || select.Skip != null;
            bool canReceiveOrderings = canHaveOrderBy && !hasGroupBy && !select.IsDistinct && !AggregateChecker.HasAggregates(select);

            var orderByColumns = new List<OrderExpression>();

            if (select.IsReverse)
                ReverseOrderings();

            bool canPassOnOrderings = !saveIsOuterMostSelect && !hasGroupBy && !select.IsDistinct;

            // maybe we already gathered some orderings, than append them now
            if (canReceiveOrderings && GatheredOrderings.Count > 0)
            {
                IEnumerable<OrderExpression> bindToSelection = BindToSelection(select, GatheredOrderings);

                if (hasOrderBy) orderByColumns.AddRange(select.OrderBy);
                if (bindToSelection != null) orderByColumns.AddRange(bindToSelection);

                // Reset gathering
                if (!canPassOnOrderings) GatheredOrderings = new List<OrderExpression>();
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From, select.Where,
                                            new ReadOnlyCollection<OrderExpression>(orderByColumns), select.GroupBy, select.Skip, select.Take, select.IsDistinct, false, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }

            // if the expression does not have an order by or can have an order by than return immidiatly
            if (!hasOrderBy || canHaveOrderBy) return select.SetReverse(false);

            // if the current expression has an order by, than gather it
            GatheredOrderings.AddRange(select.OrderBy);

            // return without ordering
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From, select.Where,
                                        null, select.GroupBy, select.Skip, select.Take, select.IsDistinct, false, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }


    }
}
