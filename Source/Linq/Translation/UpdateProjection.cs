using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateProjection : DbPackedExpressionVisitor
    {
        private Cache<Type, ProjectionClass> dynamicCache;

        private UpdateProjection(ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
            dynamicCache = backpack.ProjectionCache;
        }

        public static Expression Rebind(Expression expression, ExpressionVisitorBackpack backpack)
        {
            return new UpdateProjection(backpack).Visit(expression);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp is IDbExpressionWithResult)
                return UpdateTopLevelResult((IDbExpressionWithResult)exp);

            return base.Visit(exp);
        }

        /// <summary>
        /// Updates the top level result.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private Expression UpdateTopLevelResult(IDbExpressionWithResult expression)
        {
            ProjectionClass projection = expression.Projection;
            if (projection == null)
                return (Expression) expression;

            // Update Complex Members
            if (projection.ComplexTypeColumnMapping != null)
            {
                for (int i = 0; i < projection.ComplexTypeColumnMapping.Length; i++)
                {
                    var mapping = projection.ComplexTypeColumnMapping[i];
                    if (mapping == null)
                        continue;

                    List<ColumnDeclaration> newList = new List<ColumnDeclaration>();
                    foreach (var column in mapping)
                    {
                        var found = FindSourceColumn(expression as AliasedExpression, column.Expression);
                        newList.Add(found);
                    }

                    projection.ComplexTypeColumnMapping[i] = new ReadOnlyCollection<ColumnDeclaration>(newList);
                }

                // Add the new projection into the cache
                dynamicCache.Insert(projection.ProjectedType, projection);
            }

            return (Expression) expression;
        }



    }
}
