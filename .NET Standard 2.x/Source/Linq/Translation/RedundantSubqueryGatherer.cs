using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// RedundantSubqueryGatherer
    /// </summary>
    public class RedundantSubqueryGatherer : DbExpressionVisitor
    {
        List<SelectExpression> redundant;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedundantSubqueryGatherer"/> class.
        /// </summary>
        private RedundantSubqueryGatherer()
        {
#if TRACE
            Console.WriteLine("\nRedundantSubqueryGatherer:");
#endif
        }

        /// <summary>
        /// Gathers the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        internal static List<SelectExpression> Gather(Expression source)
        {
            var gatherer = new RedundantSubqueryGatherer();
            gatherer.Visit(source);

#if TRACE
            if (gatherer.redundant != null)
            {
                Console.WriteLine("------------------");
                foreach (var redundant in gatherer.redundant)
                    Console.WriteLine("Redundant Select: " + redundant);
                Console.WriteLine("------------------");
            }
#endif

            return gatherer.redundant;
        }

        /// <summary>
        /// Determines whether [is redudant subquery] [the specified select].
        /// </summary>
        /// <param name="select">The select.</param>
        /// <returns>
        /// 	<c>true</c> if [is redudant subquery] [the specified select]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRedudantSubquery(SelectExpression select)
        {
            return (RedundantSubqueryRemover.IsSimpleProjection(select) || RedundantSubqueryRemover.IsNameMapProjection(select))
                   && !select.IsDistinct
                   && !select.IsReverse
                   && select.Take == null
                   && select.Skip == null
                   && select.Where == null
                   && (select.OrderBy == null || select.OrderBy.Count == 0)
                   && (select.GroupBy == null || select.GroupBy.Count == 0);
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (IsRedudantSubquery(select))
            {
                if (redundant == null)
                    redundant = new List<SelectExpression>();
                    
                redundant.Add(select);
            }
            return select;
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            var result = base.VisitUnionExpression(union);

            // If the selects of the union are removed, only the table is left.
            // But only a table condition is too less to use within a Union SQL
            var first = union.First as SelectExpression;
            var second = union.Second as SelectExpression;

            if (first != null && redundant != null && redundant.Contains(first))
                redundant.Remove(first);

            if (second != null && redundant != null && redundant.Contains(second))
                redundant.Remove(second);

            return result;
        }

    }
}