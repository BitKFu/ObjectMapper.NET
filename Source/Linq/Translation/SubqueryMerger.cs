using System;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    public class SubqueryMerger : RedundanceRemover
    {
        private AliasedExpression currentFrom;
        private readonly Cache<Type, ProjectionClass> dynamicCache;

        private SubqueryMerger(ExpressionVisitorBackpack backpack)
            : base(ReferenceDirection.Referrer, backpack)
        {
            this.dynamicCache = backpack.ProjectionCache;
#if TRACE
            Console.WriteLine("\nSubqueryMerger:");
#endif
        }

        internal static Expression Merge(Expression expression, ExpressionVisitorBackpack backpack)
        {
            var result = new SubqueryMerger(backpack).Visit(expression);
            return result;
        }

        bool isTopLevel = true;

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                bool wasTopLevel = isTopLevel;
                isTopLevel = false;

                currentFrom = select = (SelectExpression)base.VisitSelectExpression(select);

                // next attempt to merge subqueries that would have been removed by the above
                // logic except for the existence of a where clause
                while (CanMergeWithFrom(select, wasTopLevel))
                {
                    SelectExpression fromSelect = GetLeftMostSelect(select.From);

                    // remove the redundant subquery
                    currentFrom = select = SubqueryRemover.Remove(select, Backpack, fromSelect);

                    // merge where expressions 
                    Expression where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = fromSelect.Where.AndAlso(where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }
                    var orderBy = select.OrderBy != null && select.OrderBy.Count > 0 ? select.OrderBy : fromSelect.OrderBy;
                    var groupBy = select.GroupBy != null && select.GroupBy.Count > 0 ? select.GroupBy : fromSelect.GroupBy;
                    Expression skip = select.Skip ?? fromSelect.Skip;
                    Expression take = select.Take ?? fromSelect.Take;
                    bool isDistinct = select.IsDistinct | fromSelect.IsDistinct;

                    if (where != select.Where
                        || orderBy != select.OrderBy
                        || groupBy != select.GroupBy
                        || isDistinct != select.IsDistinct
                        || skip != select.Skip
                        || take != select.Take
                        || select.SqlId != fromSelect.SqlId
                        || select.Hint != fromSelect.Hint)
                    {
                        var selector = select.Selector;
                        var readonlyColumns = select.Columns;

                        select = new SelectExpression(select.Type, select.Projection, select.Alias, readonlyColumns, selector, 
                                                      select.From, where, orderBy, groupBy, skip, take, isDistinct, select.IsReverse, select.SelectResult,
                                                      select.SqlId ?? fromSelect.SqlId, select.Hint ?? fromSelect.Hint, select.DefaultIfEmpty);
                    }

                    // Add the select that can be removed
                    if (RedundantSelect.ContainsKey(fromSelect.Alias))
                        RedundantSelect.Remove(fromSelect.Alias);
                    RedundantSelect.Add(fromSelect.Alias, select);
                }

                return select;
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        protected override Expression VisitColumn(PropertyExpression expression)
        {
            return base.VisitColumn(expression);
        }

        protected override Expression VisitScalarExpression(ScalarExpression expression)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                currentFrom = VisitSource(expression.From);
                var columns = VisitColumnDeclarations(expression.Columns);

                return UpdateScalarExpression(expression, columns.First(), currentFrom);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        private static SelectExpression GetLeftMostSelect(Expression source)
        {
            var select = source as SelectExpression;
            if (select != null) return select;
            var join = source as JoinExpression;
            if (join != null) return GetLeftMostSelect(join.Left);
            return null;
        }

        private static bool IsColumnProjection(SelectExpression select)
        {
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                var cd = select.Columns[i];
                if (cd.Expression.NodeType != (ExpressionType)DbExpressionType.PropertyExpression &&
                    cd.Expression.NodeType != ExpressionType.Constant)
                    return false;
            }
            return true;
        }

        private bool CanMergeWithFrom(SelectExpression select, bool isTopLevel)
        {
            SelectExpression fromSelect = GetLeftMostSelect(select.From);
            if (fromSelect == null)
                return false;
            if (!IsColumnProjection(fromSelect))
                return false;

            bool selHasNameMapProjection = RedundantSubqueryRemover.IsNameMapProjection(select);
            bool selHasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
            bool selHasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
            bool selHasAggregates = AggregateChecker.HasAggregates(select);
            bool selHasRowNum = select.Where != null && TypeChecker.Contains(DbExpressionType.RowNum, select.Where);
            bool frmHasOrderBy = fromSelect.OrderBy != null && fromSelect.OrderBy.Count > 0;
            bool frmHasGroupBy = fromSelect.GroupBy != null && fromSelect.GroupBy.Count > 0;
            bool frmHasRowNum = fromSelect.Where != null && TypeChecker.Contains(DbExpressionType.RowNum, fromSelect.Where);

            // if the from has a rownum, it can't be merged with a selection that has an order by
            if ( (selHasOrderBy && frmHasRowNum)
                 || (selHasRowNum && frmHasOrderBy) )
                return false;
            // both cannot have orderby
            if (selHasOrderBy && frmHasOrderBy)
                return false;
            // both cannot have groupby
            if (selHasGroupBy && frmHasGroupBy)
                return false;
            // this are distinct operations 
            if (select.IsReverse || fromSelect.IsReverse)
                return false;
            // cannot move forward order-by if outer has group-by
            if (frmHasOrderBy && (selHasGroupBy || selHasAggregates || select.IsDistinct))
                return false;
            // cannot move forward group-by if outer has where clause
            if (frmHasGroupBy && (select.Where != null)) // need to assert projection is the same in order to move group-by forward
                return false;
            // cannot move forward a take if outer has take or skip or distinct
            if (fromSelect.Take != null && (select.Take != null || select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy))
                return false;
            // cannot move forward a skip if outer has skip or distinct
            if (fromSelect.Skip != null && (select.Skip != null || select.IsDistinct || selHasAggregates || selHasGroupBy))
                return false;
            // cannot move forward a distinct if outer has take, skip, groupby or a different projection
            if (fromSelect.IsDistinct && (select.Take != null || select.Skip != null || !selHasNameMapProjection || selHasGroupBy || selHasAggregates || (selHasOrderBy && !isTopLevel)))
                return false;
            return true;
        }
    }
}