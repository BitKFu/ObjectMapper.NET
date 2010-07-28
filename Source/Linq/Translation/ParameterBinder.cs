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

        private ParameterBinder(ParameterExpression searchFor, ConstantExpression replaceWith)
        {
            SearchFor = searchFor;
            ReplaceWith = replaceWith;
        }

        public static Expression BindParameter(Expression exp, ParameterExpression pe, ConstantExpression constValue)
        {
            return new ParameterBinder(pe, constValue).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return p == SearchFor ? ReplaceWith : base.VisitParameter(p);
        }
    }
}
