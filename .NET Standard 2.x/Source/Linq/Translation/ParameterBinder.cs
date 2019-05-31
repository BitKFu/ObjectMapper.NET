using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This Binder replaces all named parameters with a const value expression
    /// </summary>
    public class ParameterBinder : DbExpressionVisitor
    {
        private ParameterExpression SearchFor { get; set; }
        private ConstantExpression ReplaceWith { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBinder"/> class.
        /// </summary>
        /// <param name="searchFor">The search for.</param>
        /// <param name="replaceWith">The replace with.</param>
        private ParameterBinder(ParameterExpression searchFor, ConstantExpression replaceWith)
        {
            SearchFor = searchFor;
            ReplaceWith = replaceWith;
        }

        /// <summary>
        /// Binds the parameter.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="pe">The pe.</param>
        /// <param name="constValue">The const value.</param>
        /// <returns></returns>
        public static Expression BindParameter(Expression exp, ParameterExpression pe, ConstantExpression constValue)
        {
            return new ParameterBinder(pe, constValue).Visit(exp);
        }

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return p == SearchFor ? ReplaceWith : base.VisitParameter(p);
        }
    }
}
