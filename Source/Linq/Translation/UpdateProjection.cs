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
    public class UpdateProjection : DbExpressionVisitor
    {
        private Cache<Type, ProjectionClass> dynamicCache;

        private UpdateProjection(Cache<Type, ProjectionClass> cache)
        {
            dynamicCache = cache;
        }

        public static Expression Rebind(Expression expression, Cache<Type, ProjectionClass> cache)
        {
            return new UpdateProjection(cache).Visit(expression);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp is IDbExpressionWithResult)
                return UpdateTopLevelResult((IDbExpressionWithResult)exp);

            return base.Visit(exp);
        }

        /// <summary>
        /// Finds the duplicated columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        private List<ColumnDeclaration> FindDuplicatedColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            // Remove duplicated columns
            var ignoreList = new HashSet<int>();
            var newColumns = new List<ColumnDeclaration>();
            for (int col = 0; col < columns.Count; col++)
            {
                if (ignoreList.Contains(col))
                    continue;

                for (int innerCol = col + 1; innerCol < columns.Count; innerCol++)
                    if (ExpressionComparer.AreEqual(columns[col].Expression, columns[innerCol].Expression))
                    {
                        ignoreList.Add(innerCol);
                    }

                newColumns.Add(columns[col]);
            }

            return newColumns;
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
