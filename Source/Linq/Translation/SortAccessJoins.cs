using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This class is used to sort the join for the Access Expression Writer
    /// </summary>
    public class SortAccessJoins : DbPackedExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortAccessJoins"/> class.
        /// </summary>
        public SortAccessJoins(ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
#if TRACE
            Console.WriteLine("\nSortAccessJoins:");
#endif
        }

        /// <summary>
        /// Sorts the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns></returns>
        public static Expression Sort(Expression exp, ExpressionVisitorBackpack backpack)
        {
            return new SortAccessJoins(backpack).Visit(exp);
        }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Alias, Expression> joinsToSwitch = new Dictionary<Alias, Expression>();

        /// <summary>
        /// 
        /// </summary>
        public HashSet<Alias> usedInConditions = new HashSet<Alias>();

        /// <summary>
        /// Try to re-order the Joins for Microsoft Access
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            HashSet<Alias> declared = DeclaredAliasGatherer.Gather(join.Condition);
            usedInConditions = new HashSet<Alias>(usedInConditions.Concat(declared));

            // if we find a crossjoin, than maybe the right must be switched
            if (join.Join == JoinType.CrossJoin)
            {
                AliasedExpression left = (AliasedExpression) join.Left;
                AliasedExpression right = (AliasedExpression)join.Right;

                if ((left is TableExpression || left is SelectExpression)
                && (right is TableExpression || right is SelectExpression))
                {
                    // only try to switch, if the alias has been used in prior conditions.
                    if (usedInConditions.Contains(right.Alias))
                    {
                        joinsToSwitch.Add(right.Alias, right);
                        return left;
                    }
                }
            }

            var result = base.VisitJoinExpression(join);
            join = result as JoinExpression;
            if (join == null) return result;

            // so, maybe we have to inject the join that we previously removed
            // try to figure out which parts of the current where expression can be used for a join condition
            declared = DeclaredAliasGatherer.Gather(join.Condition);
            KeyValuePair<Alias, Expression> toSwitch = joinsToSwitch.Where(jts => declared.Contains(jts.Key)).FirstOrDefault();
            if (toSwitch.Value != null)
            {
                result = new JoinExpression(join.Type, join.Projection, JoinType.CrossJoin,
                                            join.Left,
                                            new JoinExpression(join.Type, join.Projection, join.Join,
                                                               toSwitch.Value,
                                                               join.Right, join.Condition),
                                            null
                    );
            }

            return result;
        }

    }
}
