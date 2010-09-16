using System.Collections.Generic;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Removes joins expressions that are identical to joins that already exist
    /// </summary>
    public class RedundantJoinRemover : DbExpressionVisitor
    {
        private Dictionary<Alias, AliasedExpression> map;
        private Dictionary<Expression, PropertyExpression> followUpProperties;

        private RedundantJoinRemover()
        {
            map = new Dictionary<Alias, AliasedExpression>();
            followUpProperties = new Dictionary<Expression, PropertyExpression>();
        }

        public static Expression Remove(Expression expression)
        {
            return new RedundantJoinRemover().Visit(expression);
        }

        protected override Expression VisitTableExpression(TableExpression expression)
        {
            AliasedExpression similarTable;
            if (map.TryGetValue(expression.Alias, out similarTable))
                return similarTable;
            return expression;
        }

        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            Expression result = base.VisitJoinExpression(join);
            join = result as JoinExpression;
            if (join != null)
            {
                var right = join.Right as AliasedExpression;
                if (right != null)
                {
                    var similarRight = (AliasedExpression)FindSimilarRight(join.Left as JoinExpression, join);
                    if (similarRight != null)
                    {
                        map.Add(right.Alias, similarRight);
                        return join.Left;
                    }
                }
            }
            return result;
        }

        private Expression FindSimilarRight(JoinExpression join, JoinExpression compareTo)
        {
            if (join == null)
                return null;

            if (join.Join == compareTo.Join)
            {
                if (join.Right.NodeType == compareTo.Right.NodeType
                    && DbExpressionComparer.AreEqual(join.Right, compareTo.Right))
                {
                    if (join.Condition == compareTo.Condition)
                        return join.Right;
                    var scope = new ScopedDictionary<Alias, Alias>(null);
                    scope.Add(((AliasedExpression)join.Right).Alias, ((AliasedExpression)compareTo.Right).Alias);

                    if (DbExpressionComparer.AreEqual(null, scope, join.Condition, compareTo.Condition))
                        return join.Right;
                }
            }
            Expression result = FindSimilarRight(join.Left as JoinExpression, compareTo) ??
                                FindSimilarRight(join.Right as JoinExpression, compareTo);
            return result;
        }

        protected override Expression VisitColumn(PropertyExpression column)
        {
            AliasedExpression mapped;
            PropertyExpression followUpProperty;

            // Try to evaluate the column itself
            // HINT: This is initial the case, if a table has to be replaced.
            if (map.TryGetValue(column.Alias, out mapped))
            {
                // Maybe we have a followUp Property that maches, so we don't have to create a new one
                if (followUpProperties.TryGetValue(column, out followUpProperty))
                    return followUpProperty;

                // So - nothing found, create a new one and set the property for lookup into the followUpProperties
                var result = new PropertyExpression(column.Type, column.Projection, mapped.Alias, column);
                followUpProperties.Add(column, result); // Place the result as a follow Up Property 
                return result;
            }

            // Try to evaluate Follow Up Properties
            // HINT: This is the case, if Selects, based on a prior Table exchange have to be amendet.
            if (column.ReferringColumn != null &&
                followUpProperties.TryGetValue(column.ReferringColumn.Expression, out followUpProperty))
            {
                var result = new PropertyExpression(column.Projection, column.Alias, column.ReferringColumn);
//                    new ColumnDeclaration(followUpProperty, Alias.Generate(followUpProperty.Name)));
                followUpProperties.Add(column, result); // Place the result as a follow Up Property 
                return result;
            }

            return column;
        }
    }
}