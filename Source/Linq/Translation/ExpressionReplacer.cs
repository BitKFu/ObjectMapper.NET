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

        private ExpressionReplacer(Expression searchFor, Expression replaceWith)
        {
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }

        public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new ExpressionReplacer(searchFor, replaceWith).Visit(expression);
        }

        public static Expression ReplaceAll(Expression expression, Expression[] searchFor, Expression[] replaceWith)
        {
            for (int i = 0, n = searchFor.Length; i < n; i++)
                expression = Replace(expression, searchFor[i], replaceWith[i]);
            return expression;
        }

        protected override Expression Visit(Expression exp)
        {
            return exp == searchFor ? replaceWith : base.Visit(exp);
        }
    }
}
