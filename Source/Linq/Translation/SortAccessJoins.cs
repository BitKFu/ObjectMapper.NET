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
    public class SortAccessJoins : DbExpressionVisitor
    {
        Stack<Type> leftJoinTypes = new Stack<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SortAccessJoins"/> class.
        /// </summary>
        public SortAccessJoins()
        {
#if TRACE
            Console.WriteLine("\nSortAccessJoins:");
#endif
        }

        /// <summary>
        /// Sorts the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        public static Expression Sort(Expression exp)
        {
            return new SortAccessJoins().Visit(exp);
        }

        private Dictionary<Alias, Expression> joinsToSwitch = new Dictionary<Alias, Expression>();
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

        ///// <summary>
        ///// Visits the join expression.
        ///// </summary>
        ///// <param name="join"></param>
        ///// <returns></returns>
        //protected override Expression VisitJoinExpression(JoinExpression join)
        //{
        //    leftJoinTypes.Push(join.RevealedType);
        //    try
        //    {
        //        var compareTo = ExpressionTypeFinder.Find(join.Condition, typeof (PropertyExpression)) as PropertyExpression;

        //        if (compareTo != null && join.RevealedType == compareTo.Projection.ProjectedType && join.Right is JoinExpression)
        //            join = UpdateJoin(join, join.Join, join.Right, join.Left, join.Condition);

        //        return base.VisitJoinExpression(join);

        //        //if (result != null && !(result.Left is JoinExpression) && !(result.Right is JoinExpression))
        //        //    result = UpdateJoin(result, result.Join, result.Right, result.Left, result.Condition);

        //        //return result;
        //    }
        //    finally
        //    {
        //        leftJoinTypes.Pop();
        //    }
        //}
    }
}
