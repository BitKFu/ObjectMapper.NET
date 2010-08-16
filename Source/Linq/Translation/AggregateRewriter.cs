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
    /// Rewrite aggregate expressions, moving them into same select expression that has the group-by clause
    /// </summary>
    public class AggregateRewriter : DbExpressionVisitor
    {
        ILookup<Alias, AggregateSubqueryExpression> lookup;
        Dictionary<AggregateSubqueryExpression, Expression> map;
        readonly Cache<Type, ProjectionClass> dynamicCache;

        private AggregateRewriter(Expression expr, Cache<Type, ProjectionClass> cache)
        {
            dynamicCache = cache;
            map = new Dictionary<AggregateSubqueryExpression, Expression>();
            lookup = AggregateGatherer.Gather(expr).ToLookup(a => a.Alias);
        }

        public static Expression Rewrite(Expression expr, Cache<Type, ProjectionClass> cache)
        {
            return new AggregateRewriter(expr, cache).Visit(expr);
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var from = VisitSource(select.From);
            var where = Visit(select.Where);
            var orderBy = VisitOrderBy(select.OrderBy);
            var groupBy = VisitExpressionList(select.GroupBy);
            var skip = Visit(select.Skip);
            var take = Visit(select.Take);
            var defaultIfEmpty = select.DefaultIfEmpty;
            var selector = select.Selector;
            var columns = select.Columns;

            if (ExpressionTypeFinder.Find(select.Selector, typeof(AggregateSubqueryExpression)) != null)
            {
                selector = Visit(select.Selector);
                columns = ColumnProjector.Evaluate(selector, dynamicCache);
            }

            select = UpdateSelect(select, select.Projection, selector, from, where, orderBy, groupBy, skip, take, select.IsDistinct,
                                  select.IsReverse, columns, select.SqlId, select.Hint, defaultIfEmpty);

            if (lookup.Contains(select.Alias))
            {
                var aggColumns = new List<ColumnDeclaration>(select.Columns);
                foreach (AggregateSubqueryExpression ae in lookup[select.Alias])
                {
                    var cd = new ColumnDeclaration(ae.AggregateInGroupSelect, Alias.Generate(AliasType.Column));
                    map.Add(ae, new PropertyExpression(ae, cd));
                    aggColumns.Add(cd);
                }
                return new SelectExpression(select.Type, select.Projection, select.Alias,
                                            new ReadOnlyCollection<ColumnDeclaration>(aggColumns), select.Selector,
                                            select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip,
                                            select.Take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            Expression mapped;
            if (map.TryGetValue(aggregate, out mapped))
            {
                return mapped;
            }
            return aggregate; // Visit(aggregate.AggregateAsSubquery);
        }

        class AggregateGatherer : DbExpressionVisitor
        {
            List<AggregateSubqueryExpression> aggregates = new List<AggregateSubqueryExpression>();
            private AggregateGatherer()
            {
            }

            internal static List<AggregateSubqueryExpression> Gather(Expression expression)
            {
                AggregateGatherer gatherer = new AggregateGatherer();
                gatherer.Visit(expression);
                return gatherer.aggregates;
            }

            protected override Expression VisitSelectExpression(SelectExpression select)
            {
                Visit(select.Selector);
                return base.VisitSelectExpression(select);
            }

            protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
            {
                this.aggregates.Add(aggregate);
                return base.VisitAggregateSubquery(aggregate);
            }
        }
    }
}