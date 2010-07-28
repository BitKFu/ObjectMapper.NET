using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Rewrites take & skip expressions into uses of TSQL row_number function
    /// </summary>
    public class SkipToRowNumberRewriter : DbExpressionVisitor
    {
        private readonly Cache<Type, ProjectionClass> dynamicCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipToRowNumberRewriter"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        private SkipToRowNumberRewriter(Cache<Type, ProjectionClass> cache)
        {
            dynamicCache = cache;

#if TRACE
            Console.WriteLine("\nSkipToRowNumberRewriter:");
#endif
        }


        /// <summary>
        /// Rewrites the SkipTo Expression for SQL Server
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, Cache<Type, ProjectionClass> cache)
        {
            return new SkipToRowNumberRewriter(cache).Visit(expression);
        }

        /// <summary>
        /// Overrides the Select Expression
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelectExpression(select);
            if (select.Skip != null)
            {
                SelectExpression newSelect = select.SetSkip(null).SetTake(null);
                bool canAddColumn = !select.IsDistinct && (select.GroupBy == null || select.GroupBy.Count == 0);
                if (!canAddColumn)
                {
                    newSelect = newSelect.AddRedundantSelect(Alias.Generate(AliasType.Select));
                }

                List<OrderExpression> orderExpressions;
                if (select.OrderBy != null)
                    orderExpressions = new List<OrderExpression>(select.OrderBy);
                else
                {
                    var fromAlias = select.From;
                    var selectionType = fromAlias.Type.RevealType();
                    var selectionProjection = ReflectionHelper.GetProjection(selectionType, dynamicCache);

                    if (selectionProjection != null)
                    {
                        var primaryKey = selectionProjection.GetPrimaryKeyDescription();
                        var orderExpression = new PropertyExpression(fromAlias, primaryKey.CustomProperty.PropertyInfo);
                        
                        // Find the original column, because thus have referenced columsn info set
                        var column = select.Columns.FirstOrDefault(x => x.Expression.Equals(orderExpression));

                        orderExpressions = new List<OrderExpression>{
                            new OrderExpression(
                                Ordering.Asc,
                                column.Expression)}; //selectionType
                    }
                    else
                    {
                        orderExpressions = new List<OrderExpression>{
                            new OrderExpression(
                                Ordering.Asc,
                                newSelect.Columns.First().Expression)};
                    }
                }

                var newBoundOrderings = OrderByRewriter.BindToSelection(newSelect, orderExpressions);
                var rownum = new ColumnDeclaration(new RowNumberExpression(newBoundOrderings),
                                                   Alias.Generate(AliasType.Column));
                newSelect = newSelect.AddColumn(rownum);

                // add layer for WHERE clause that references new rownum column
                newSelect = newSelect.AddRedundantSelect(Alias.Generate(AliasType.Select));
                newSelect = newSelect.RemoveColumn(newSelect.Columns.Single(c =>
                    {
                        var inner = c.Expression as PropertyExpression;
                        if (inner != null && inner.ReferringColumn != null)
                        {
                            if (inner.ReferringColumn.Expression is RowNumberExpression)
                                return true;
                        }
                        return false;
                    }));

                var newAlias = ((AliasedExpression)newSelect.From);
                var rnCol = new PropertyExpression(newAlias, rownum);
                Expression where;
                if (select.Take != null)
                {
                    where = new BetweenExpression(rnCol, Expression.Add(select.Skip, new ValueExpression(typeof(int), 1)), Expression.Add(select.Skip, select.Take));
                }
                else
                {
                    where = rnCol.GreaterThan(select.Skip);
                }
                if (newSelect.Where != null)
                {
                    where = newSelect.Where.AndAlso(where);
                }
                newSelect = newSelect.SetWhere(where);

                select = newSelect;
            }
            return select;
        }
    }
}