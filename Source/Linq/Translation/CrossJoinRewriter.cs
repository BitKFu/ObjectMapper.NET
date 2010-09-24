using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Attempt to rewrite cross joins as inner joins
    /// </summary>
    public class CrossJoinRewriter : DbPackedExpressionVisitor
    {
        private Expression currentWhere;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="backpack"></param>
        private CrossJoinRewriter(ExpressionVisitorBackpack backpack) : base(backpack)
        {
        }

        ///<summary>
        /// Try to remove cross join and replace it with inner joins
        ///</summary>
        public static Expression Rewrite(Expression expression, ExpressionVisitorBackpack backpack)
        {
            return new CrossJoinRewriter(backpack).Visit(expression);
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            Expression saveWhere = currentWhere;
            try
            {
                currentWhere = select.Where;
                var result = (SelectExpression) base.VisitSelectExpression(select);
                if (currentWhere != result.Where)
                {
                    return result.SetWhere(currentWhere);
                }
                return result;
            }
            finally
            {
                currentWhere = saveWhere;
            }
        }

        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            join = (JoinExpression) base.VisitJoinExpression(join);
            if (join.Join == JoinType.CrossJoin && currentWhere != null)
            {
                // try to figure out which parts of the current where expression can be used for a join condition
                HashSet<Alias> declaredLeft = DeclaredAliasGatherer.Gather(join.Left);
                HashSet<Alias> declaredRight = DeclaredAliasGatherer.Gather(join.Right);
                var declared = new HashSet<Alias>(declaredLeft.Union(declaredRight));
                Expression[] exprs = currentWhere.Split(ExpressionType.And, ExpressionType.AndAlso);
                List<Expression> good = exprs.Where(e => CanBeJoinCondition(e, declaredLeft, declaredRight, declared)).ToList();
                if (good.Count > 0)
                {
                    Expression condition = good.Join(ExpressionType.AndAlso);
                    join = UpdateJoin(join, JoinType.InnerJoin, join.Left, join.Right, condition);
                    Expression newWhere = exprs.Where(e => !good.Contains(e)).Join(ExpressionType.AndAlso);
                    currentWhere = newWhere;
                }
            }
            return join;
        }

        private static bool CanBeJoinCondition(Expression expression, IEnumerable<Alias> left, IEnumerable<Alias> right, IEnumerable<Alias> all)
        {
            // an expression is good if it has at least one reference to an alias from both left & right sets and does
            // not have any additional references that are not in both left & right sets
            HashSet<Alias> referenced = ReferencedAliasGatherer.Gather(expression);
            bool leftOkay = referenced.Intersect(left).Any();
            bool rightOkay = referenced.Intersect(right).Any();
            bool subset = referenced.IsSubsetOf(all);
            return leftOkay && rightOkay && subset;
        }
    }
}