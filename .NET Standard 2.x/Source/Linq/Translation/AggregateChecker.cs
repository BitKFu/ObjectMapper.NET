using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Determines if a SelectExpression contains any aggregate expressions
    /// </summary>
    public class AggregateChecker : DbExpressionVisitor
    {
        bool hasAggregate;
        private AggregateChecker()
        {
        }

        public static bool HasAggregates(SelectExpression expression)
        {
            var checker = new AggregateChecker();
            checker.Visit(expression);
            return checker.hasAggregate;
        }

        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            hasAggregate = true;
            return aggregate;
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            // only consider aggregates in these locations
            Visit(select.Where);
            VisitOrderBy(select.OrderBy);
            VisitColumnDeclarations(select.Columns);
            return select;
        }

    }
}