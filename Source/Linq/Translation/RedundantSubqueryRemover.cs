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
    /// Removes select expressions that don't add any additional semantic value
    /// </summary>
    public class RedundantSubqueryRemover : DbExpressionVisitor
    {
        private readonly Cache<Type, ProjectionClass> dynamicCache;
        private readonly Dictionary<Alias, IDbExpressionWithResult> redundantSelect = new Dictionary<Alias, IDbExpressionWithResult>();

        private RedundantSubqueryRemover(Cache<Type, ProjectionClass> dynamicCache) 
        {
            this.dynamicCache = dynamicCache;
#if TRACE
            Console.WriteLine("\nRedundantSubqueryRemover:");
#endif
        }

        ///<summary>
        /// Rewrites the expression and removes redundant subqueries
        ///</summary>
        ///<param name="expression"></param>
        ///<returns></returns>
        public static Expression Remove(Expression expression, Cache<Type, ProjectionClass> dynamicCache) 
        {
            expression = new RedundantSubqueryRemover(dynamicCache).Visit(expression);
            ReferingColumnChecker.Validate(expression);

            expression = SubqueryMerger.Merge(expression, dynamicCache);
            return expression;
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelectExpression(select);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(select.From, dynamicCache);
            if (redundant != null)
            {
                select = SubqueryRemover.Remove(select, redundant, dynamicCache);

                // Add this removement in order to adjust the columns
                SelectExpression removedWith = select;
                redundant.ForEach(removedSelection =>
                                      {
                                          if (!redundantSelect.ContainsKey(removedSelection.Alias))
                                              redundantSelect.Add(removedSelection.Alias, removedWith);
                                      });

                // Gather the SQL Id and first hint
                var sqlId = redundant.Where(selection => !string.IsNullOrEmpty(selection.SqlId)).Select(selection=>selection.SqlId).FirstOrDefault();
                var hint = redundant.Where(selection => !string.IsNullOrEmpty(selection.Hint)).Select(selection=>selection.Hint).FirstOrDefault();

                if (sqlId != null || hint != null)
                {
                    select = UpdateSelect(select, select.Projection, select.Selector, select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip,
                        select.Take, select.IsDistinct, select.IsReverse, select.Columns, sqlId ?? select.SqlId, hint ?? select.Hint, select.DefaultIfEmpty);
                }
            }
            return select;
        }

        /// <summary>
        /// Check the property expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            var refColumn = expression.ReferringColumn;
            if (refColumn == null)
                return base.VisitColumn(expression);

            var refProperty = refColumn.Expression as PropertyExpression;
            if (refProperty == null)
                return base.VisitColumn(expression);

            IDbExpressionWithResult newFromSelection;
            if (redundantSelect.TryGetValue(refProperty.Alias, out newFromSelection))
            {
                // Now shortcut the ReferringColumn
                expression.ReferringColumn = refProperty.ReferringColumn;
            }

            return base.VisitColumn(expression);
        }

        internal static bool IsSimpleProjection(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                var col = decl.Expression as PropertyExpression;
                if (col == null || !string.Equals(decl.Alias.Name, col.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNameMapProjection(SelectExpression select)
        {
            if (select.From is TableExpression) return false;
            var fromSelect = select.From as SelectExpression;
            if (fromSelect == null || select.Columns.Count != fromSelect.Columns.Count)
                return false;

            ReadOnlyCollection<ColumnDeclaration> fromColumns = fromSelect.Columns;
            // test that all columns in 'select' are refering to columns in the same position
            // in from.
            for (int i = 0, n = select.Columns.Count; i < n; i++)
            {
                var col = select.Columns[i].Expression as PropertyExpression;
                if (col == null || !(string.Equals(col.Name, fromColumns[i].Alias.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return false;
            }
            return true;
        }

        internal static bool IsInitialProjection(SelectExpression select)
        {
            return select.From is TableExpression;
        }

        class RedundantSubqueryGatherer : DbExpressionVisitor
        {
            Cache<Type, ProjectionClass> dynamicCache;
            List<SelectExpression> redundant;

            private RedundantSubqueryGatherer(Cache<Type, ProjectionClass> dynamicCache)
            {
                this.dynamicCache = dynamicCache;
#if TRACE
                Console.WriteLine("\nRedundantSubqueryGatherer:");
#endif
            }

            internal static List<SelectExpression> Gather(Expression source, Cache<Type, ProjectionClass> dynamicCache)
            {
                var gatherer = new RedundantSubqueryGatherer(dynamicCache);
                gatherer.Visit(source);
                return gatherer.redundant;
            }

            public static bool IsRedudantSubquery(SelectExpression select)
            {
                return (IsSimpleProjection(select) || IsNameMapProjection(select))
                       && !select.IsDistinct
                       && !select.IsReverse
                       && select.Take == null
                       && select.Skip == null
                       && select.Where == null
                       && (select.OrderBy == null || select.OrderBy.Count == 0)
                       && (select.GroupBy == null || select.GroupBy.Count == 0);
            }

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

            //protected override Expression VisitSubquery(SubqueryExpression subquery)
            //{
            //    // don't gather inside scalar & exists
            //    return subquery;
            //}
        }

        class SubqueryMerger : DbExpressionVisitor
        {
            Cache<Type, ProjectionClass> dynamicCache;
            private AliasedExpression currentFrom;

            private SubqueryMerger(Cache<Type, ProjectionClass> dynamicCache)
            {
                this.dynamicCache = dynamicCache;
#if TRACE
                Console.WriteLine("\nSubqueryMerger:");
#endif
            }

            internal static Expression Merge(Expression expression, Cache<Type, ProjectionClass> dynamicCache)
            {
                return new SubqueryMerger(dynamicCache).Visit(expression);
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
                        currentFrom = select = SubqueryRemover.Remove(select, dynamicCache, fromSelect);

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
                    }

                    return select;
                }
                finally
                {
                    currentFrom = saveCurrentFrom;
                }
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

            ///// <summary>
            ///// Visits the column expression
            ///// </summary>
            ///// <param name="expression"></param>
            ///// <returns></returns>
            //protected override Expression VisitColumn(PropertyExpression expression)
            //{
            //    IDbExpressionWithResult resultFrom = ((IDbExpressionWithResult)currentFrom);

            //    var originalProperty = OriginPropertyFinder.Find(expression);
            //    if (originalProperty == null || resultFrom == null || resultFrom.FromExpression == null)
            //        return base.VisitColumn(expression);

            //    // return the column of the current select expression
            //    var columns = resultFrom.FromExpression.Columns;
            //    var result = columns.Where(x => x.OriginalProperty == originalProperty).FirstOrDefault();

            //    return new PropertyExpression(resultFrom.FromExpression as AliasedExpression, result).SetType(expression.Type);
            //}

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

            private static bool CanMergeWithFrom(SelectExpression select, bool isTopLevel)
            {
                SelectExpression fromSelect = GetLeftMostSelect(select.From);
                if (fromSelect == null)
                    return false;
                if (!IsColumnProjection(fromSelect))
                    return false;

                bool selHasNameMapProjection = IsNameMapProjection(select);
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
}