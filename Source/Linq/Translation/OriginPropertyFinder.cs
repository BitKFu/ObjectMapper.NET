using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class OriginPropertyFinder : DbExpressionVisitor
    {
        public PropertyExpression FoundProperty { get; private set; }

        public static PropertyExpression Find (Expression toSearchIn)
        {
            var opf = new OriginPropertyFinder();
            opf.Visit(toSearchIn);
            return opf.FoundProperty;
        }

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
