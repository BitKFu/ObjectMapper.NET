using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            if (GatheredOrderings != null)
            {
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
        }

        /// <summary>
        /// Binds to selection.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="orderings">The orderings.</param>
        /// <returns></returns>
        public static IList<OrderExpression> BindToSelection(SelectExpression select, List<OrderExpression> orderings)
        {
            var newOrderings = new List<OrderExpression>();
            foreach (var oe in orderings)
            {
                var aliases = DeclaredAliasGatherer.Gather(oe);
                var reboundOrdering = RebindToSelection.Rebind(select, select.From, oe, aliases);
                newOrderings.Add((OrderExpression) reboundOrdering);
            }

            return newOrderings;
        }
    }
}