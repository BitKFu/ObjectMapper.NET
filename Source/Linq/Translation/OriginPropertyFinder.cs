using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// OriginPropertyFinder
    /// </summary>
    public class OriginPropertyFinder : DbExpressionVisitor
    {
        /// <summary>
        /// Gets or sets the found property.
        /// </summary>
        /// <value>The found property.</value>
        public PropertyExpression FoundProperty { get; private set; }

        /// <summary>
        /// Finds the specified to search in.
        /// </summary>
        /// <param name="toSearchIn">To search in.</param>
        /// <returns></returns>
        public static PropertyExpression Find (Expression toSearchIn)
        {
            var opf = new OriginPropertyFinder();
            opf.Visit(toSearchIn);
            return opf.FoundProperty;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (FoundProperty != null)
                return exp;

            if (exp is MemberExpression || 
                exp is PropertyExpression || 
                exp is AggregateExpression || 
                exp is OrderExpression)
                return base.Visit(exp);

            return exp;
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            if (expression.ReferringColumn != null)
            {
                Visit(expression.ReferringColumn.Expression);
            }
            else
                FoundProperty = expression;
            return expression;
        }
    }
}
