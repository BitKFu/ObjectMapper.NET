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
    public class RedundantSubqueryRemover : RedundanceRemover
    {
        private readonly Cache<Type, ProjectionClass> dynamicCache;
        //private readonly Dictionary<Alias, IDbExpressionWithResult> RedundantSelect = new Dictionary<Alias, IDbExpressionWithResult>();

        private RedundantSubqueryRemover(ExpressionVisitorBackpack backpack) 
            :base(ReferenceDirection.Referrer, backpack)
        {
            this.dynamicCache = backpack.ProjectionCache;
#if TRACE
            Console.WriteLine("\nRedundantSubqueryRemover:");
#endif
        }

        ///<summary>
        /// Rewrites the expression and removes redundant subqueries
        ///</summary>
        public static Expression Remove(Expression expression, ExpressionVisitorBackpack backpack) 
        {
            expression = new RedundantSubqueryRemover(backpack).Visit(expression);
            ReferingColumnChecker.Validate(expression);

            expression = SubqueryMerger.Merge(expression, backpack);
            return expression;
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelectExpression(select);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(select.From);
            if (redundant != null)
            {
                SelectExpression replacedBy = SubqueryRemover.Remove(select, redundant, Backpack);

                if (RedundantSelect.ContainsKey(select.Alias))
                    RedundantSelect.Remove(select.Alias);

                if (select.Alias != replacedBy.Alias)
                    RedundantSelect.Add(select.Alias, replacedBy);

                // Add this removement in order to adjust the columns
                select = replacedBy;

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

        protected override Expression VisitColumn(PropertyExpression expression)
        {
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
    }
}