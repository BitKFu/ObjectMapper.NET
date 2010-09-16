using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
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
        /// <param name="orderings">The orderings.</param>
        /// <returns></returns>
        public static List<OrderExpression> BindToSelection(AliasedExpression bindTo, List<OrderExpression> orderings)
        {
            var newOrderings = new List<OrderExpression>();

            IDbExpressionWithResult result = bindTo as IDbExpressionWithResult;       // Check if the current expression is a SelectExpression
            if (result == null) 
                return orderings;

            var fromSelects = result.FromExpression;                                  // Resolve the From Clause
            if (fromSelects == null)
                return orderings;

            foreach (var oe in orderings)
            {
                // Search for the first matching expression
                foreach (var select in fromSelects)
                {
                    var aliased = oe.Expression as AliasedExpression;
                    if (aliased != null && select.Alias.Equals(aliased.Alias))
                    {
                        //PropertyExpression prop = aliased as PropertyExpression;
                        //if (prop != null)
                        //{
                        //    var refColumn = select.Columns.Where(col => DbExpressionComparer.AreEqual(col.Expression, prop.ReferringColumn.Expression)).FirstOrDefault();
                        //    prop = (PropertyExpression) prop.Clone();
                        //    prop.ReferringColumn = refColumn;
                        //    var newOe = new OrderExpression(oe.Ordering, prop);
                        //    newOrderings.Add(newOe);
                        //}
                        //else
                            newOrderings.Add(oe);
                        continue;
                    }

                    OrderExpression orderBy = oe;
                    var dependentFrom = select.Columns.Where(col => DbExpressionComparer.AreEqual(col.Expression, orderBy.Expression)).FirstOrDefault();
                    if (dependentFrom == null)
                    {
                        dependentFrom = select.Columns.Where(col => DbExpressionFinder.Contains(col.Expression, orderBy.Expression)).FirstOrDefault();
                        if (dependentFrom == null)
                            continue;
                    }

                    if (string.IsNullOrEmpty(select.Alias.Name))
                        newOrderings.Add(new OrderExpression(oe.Ordering, dependentFrom.Expression)); // .SetType(orderBy.Type)));
                    else
                        newOrderings.Add(new OrderExpression(oe.Ordering, new PropertyExpression((AliasedExpression)select, dependentFrom).SetType(orderBy.Type)));
                }
            }

            return newOrderings;
        }

    }
}