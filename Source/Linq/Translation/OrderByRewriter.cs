using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Queries;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// The OrderBy Rewriter is used to push inner orderings to the outer SQL
    /// </summary>
    public abstract class OrderByRewriter : DbExpressionVisitor
    {
        protected List<OrderExpression> GatheredOrderings = new List<OrderExpression>();
        private Expression root;

        protected OrderByRewriter(Expression rootEx)
        {
#if TRACE
            Console.WriteLine("\nOrderByRewriter:");
#endif
            root = rootEx;
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <value>The root.</value>
        protected Expression Root
        {
            get { return root; }
        }

        /// <summary>
        /// Reverses the orderings, if necessary
        /// </summary>
        protected void ReverseOrderings()
        {
            if (GatheredOrderings == null) return;

            for (int i = 0, n = GatheredOrderings.Count; i < n; i++)
            {
                var ord = GatheredOrderings[i];
                GatheredOrderings[i] =
                    new OrderExpression(
                        ord.Ordering == Ordering.Asc ? Ordering.Desc : Ordering.Asc,
                        ord.Expression
                        );
            }
        }

        /// <summary>
        /// Binds to selection.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="orderings">The orderings.</param>
        /// <returns></returns>
        public static List<OrderExpression> BindToSelection(Expression bindTo, List<OrderExpression> orderings)
        {
            var newOrderings = new List<OrderExpression>();

            SelectExpression select = bindTo as SelectExpression;       // Check if the current expression is a SelectExpression
            if (select == null) return orderings;

            select = select.From as SelectExpression;                   // Resolve the From Clause
            if (select == null) return orderings;

            foreach (var oe in orderings)
            {
                // If the alias is the same like the select alias, than the ordering depends on the current select, 
                // which means that we can take it as it is.
                var ae = oe.Expression as AliasedExpression;
                if (ae != null && ae.Alias.Equals(select.Alias))
                {
                    newOrderings.Add(oe);
                    continue;
                }

                // Search for the first matching expression
                OrderExpression orderBy = oe;
                var dependentFrom = select.Columns.Where(col => DbExpressionComparer.AreEqual(col.Expression, orderBy.Expression)).FirstOrDefault();
                newOrderings.Add(new OrderExpression(oe.Ordering, new PropertyExpression(select, dependentFrom).SetType(orderBy.Type)));
            }

            return newOrderings;
        }
    }
}