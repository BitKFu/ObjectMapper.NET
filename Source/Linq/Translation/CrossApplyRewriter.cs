using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Attempts to rewrite cross-apply and outer-apply joins as inner and left-outer joins
    /// </summary>
    public class CrossApplyRewriter : DbPackedExpressionVisitor
    {
        private CrossApplyRewriter(ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
#if TRACE
            Console.WriteLine("\nCrossApply Rewriter:");
#endif

        }

        public static Expression Rewrite(Expression expression, ExpressionVisitorBackpack backpack)
        {
            return new CrossApplyRewriter(backpack).Visit(expression);
        }

        /// <summary> Visits the select expression. </summary>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var from = VisitSource(select.From);
            
            var where = Visit(select.Where);
            var orderBy = VisitOrderBy(select.OrderBy);
            var groupBy = VisitExpressionList(select.GroupBy);
            var skip = Visit(select.Skip);
            var take = Visit(select.Take);
            var selector = select.Selector;

            if (from != select.From
                 || where != select.Where
                 || orderBy != select.OrderBy
                 || groupBy != select.GroupBy
                 || take != select.Take
                 || skip != select.Skip
                 || selector != select.Selector
                  )
            {
                List<ColumnDeclaration> columns = GetColumns(from, select.Columns, selector, select.Projection);

                return UpdateSelect(select, select.Projection, selector, from, where, orderBy, groupBy, skip, take, select.IsDistinct,
                                    select.IsReverse, new ReadOnlyCollection<ColumnDeclaration>(columns), select.SqlId, select.Hint, select.DefaultIfEmpty);
            }

            return select;
        }

        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            join = (JoinExpression)base.VisitJoinExpression(join);

            if (join.Join == JoinType.CrossApply || join.Join == JoinType.OuterApply)
            {
                if (join.Right is TableExpression)
                {
                    return new JoinExpression(join.Type, join.Projection, JoinType.CrossJoin, join.Left, join.Right, null);
                }
                else
                {
                    var select = join.Right as SelectExpression;
                    // Only consider rewriting cross apply if 
                    //   1) right side is a select
                    //   2) other than in the where clause in the right-side select, no left-side declared aliases are referenced
                    //   3) and has no behavior that would change semantics if the where clause is removed (like groups, aggregates, take, skip, etc).
                    // Note: it is best to attempt this after redundant subqueries have been removed.
                    if (select != null
                        && select.Take == null
                        && select.Skip == null
                        && !AggregateChecker.HasAggregates(select)
                        && (select.GroupBy == null || select.GroupBy.Count == 0))
                    {
                        SelectExpression selectWithoutWhere = select.SetWhere(null);
                        HashSet<Alias> referencedAliases = ReferencedAliasGatherer.Gather(selectWithoutWhere);
                        HashSet<Alias> declaredAliases = DeclaredAliasGatherer.Gather(join.Left);
                        referencedAliases.IntersectWith(declaredAliases);
                        if (referencedAliases.Count == 0)
                        {
                            Expression where = select.Where;
                            where = RebindToSelection.Rebind(select, select, where, DeclaredAliasGatherer.Gather(select.From), Backpack);
                            
                            select = selectWithoutWhere;
                            JoinType jt = (where == null) ? JoinType.CrossJoin : (join.Join == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftOuter);
                            return new JoinExpression(join.Type, join.Projection, jt, join.Left, select, where);
                        }
                    }
                }
            }

            return join;
        }
    }
}