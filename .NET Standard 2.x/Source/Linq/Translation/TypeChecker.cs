using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Checks if a specified expression type is used within the subtree
    /// </summary>
    public class TypeChecker : DbExpressionVisitor
    {
        private bool found;
        private readonly DbExpressionType searchForType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeChecker"/> class.
        /// </summary>
        /// <param name="searchFor">The search for.</param>
        private TypeChecker(DbExpressionType searchFor)
        {
            searchForType = searchFor;
        }

        /// <summary>
        /// Checks if a special expression type is contained within the epxression tree
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static bool Contains(DbExpressionType type, Expression expr)
        {
            var checker = new TypeChecker(type);
            checker.Visit(expr);
            return checker.found;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

            if (found) return exp;
            found = exp.NodeType == (ExpressionType) searchForType;
            return found ? exp : base.Visit(exp);
        }
    }
}
