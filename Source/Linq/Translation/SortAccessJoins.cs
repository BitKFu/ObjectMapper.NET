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
