using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AdFactum.Data.Linq.Expressions
{
    public class DbExpressionFinder : DbExpressionVisitor
    {
        Expression ToFind { get; set; }
        public bool Found { get; private set; }

        private DbExpressionFinder(Expression toFind)
        {
            ToFind = toFind;
        }

        /// <summary>
        /// Tries to find the ToFind Expression in the toSearchIn
        /// </summary>
        /// <param name="toSearchIn"></param>
        /// <param name="toFind"></param>
        /// <returns></returns>
        public static bool Contains(Expression toSearchIn, Expression toFind)
        {
            var finder = new DbExpressionFinder(toFind);
            finder.Visit(toSearchIn);
            return finder.Found;
        }

        protected override Expression Visit(Expression exp)
        {
            if (Found)
                return exp;

            if (DbExpressionComparer.AreEqual(exp, ToFind))
            {
                Found = true;
                return exp;
            }

            return base.Visit(exp);
        }

        protected override Expression VisitColumn(PropertyExpression expression)
        {
            if (expression.ReferringColumn != null)
                expression.ReferringColumn = new ColumnDeclaration(Visit(expression.ReferringColumn.Expression), expression.ReferringColumn.Alias);   

            return base.VisitColumn(expression);
        }
    }
}
