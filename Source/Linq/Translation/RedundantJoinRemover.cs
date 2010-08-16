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
        Dictionary<Alias, AliasedExpression> map;

        private RedundantJoinRemover()
        {
            this.map = new Dictionary<Alias, AliasedExpression>();
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
            if (map.TryGetValue(column.Alias, out mapped))
                return new PropertyExpression(column.Type, column.Projection, mapped.Alias, column);

            return column;
        }
    }
}