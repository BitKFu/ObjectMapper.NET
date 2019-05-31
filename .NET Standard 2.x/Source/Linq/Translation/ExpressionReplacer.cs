using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// The expression replacer is used to replace parameter placeholders with concrete sql parameters
    /// </summary>
    public class ExpressionReplacer : DbExpressionVisitor
    {
        readonly Expression searchFor;
        readonly Expression replaceWith;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionReplacer"/> class.
        /// </summary>
        /// <param name="searchFor">The search for.</param>
        /// <param name="replaceWith">The replace with.</param>
        private ExpressionReplacer(Expression searchFor, Expression replaceWith)
        {
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }

        /// <summary>
        /// Replaces the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="searchFor">The search for.</param>
        /// <param name="replaceWith">The replace with.</param>
        /// <returns></returns>
        public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new ExpressionReplacer(searchFor, replaceWith).Visit(expression);
        }

        /// <summary>
        /// Replaces all.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="searchFor">The search for.</param>
        /// <param name="replaceWith">The replace with.</param>
        /// <returns></returns>
        public static Expression ReplaceAll(Expression expression, Expression[] searchFor, Expression[] replaceWith)
        {
            for (int i = 0, n = searchFor.Length; i < n; i++)
                expression = Replace(expression, searchFor[i], replaceWith[i]);
            return expression;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            return exp == searchFor ? replaceWith : base.Visit(exp);
        }
    }
}
