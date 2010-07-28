using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This Rewriter is especially used by Oracle in order to rewrite the Take (TOP SQL) Condition to a rownum expression
    /// if the SQL does not contain an ordering
    /// </summary>
    public class OracleTakeToRowNumberRewriter: DbExpressionVisitor
    {
        private readonly Cache<Type, ProjectionClass> dynamicCache;

        private OracleTakeToRowNumberRewriter(Cache<Type, ProjectionClass> cache)
        {
            dynamicCache = cache;
#if TRACE
            Console.WriteLine("\nOracleTakeToRowNumberRewriter:");
#endif
        }

        /// <summary>
        /// Rewrites the Top Expression for Oracle
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, Cache<Type, ProjectionClass> cache)
        {
            return new OracleTakeToRowNumberRewriter(cache).Visit(expression);
        }

        /// <summary>
        /// Overrides the Select Expression
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelectExpression(select);
            if (select.Take == null || select.Skip != null)
                return select;

            // if there's no order by, we can use a row num expression
            if (select.OrderBy == null || select.OrderBy.Count == 0)
            {
                var check = Expression.MakeBinary(ExpressionType.LessThanOrEqual, new RowNumExpression(), select.Take);
                if (select.IsReverse)
                {
                    select = select.SetTake(null);

                    // if it's a reverse collection, create a surrounding selection
                    var columns = ColumnProjector.Evaluate(select, dynamicCache);
                    var sselect = new SelectExpression(select.Type, select.Projection, Alias.Generate(AliasType.Select), columns, null,
                        select, check, null, null, null, null, false, false, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
                    select = sselect;
                }
                else
                {
                    select = select.SetTake(null);
                    select = select.Where == null
                                 ? select.SetWhere(check)
                                 : select.SetWhere(select.Where.AndAlso(check));
                }
            }
            else
            {
                // oterhwise we have to use the paging mechanism
                select = select.SetSkip(new ValueExpression(typeof (int), 0));
            }
            
            return select;
        }
    }
}
