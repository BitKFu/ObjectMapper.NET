using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// DbExpressionFinder
    /// </summary>
    public class DbExpressionFinder : DbExpressionVisitor
    {
        Expression ToFind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DbExpressionFinder"/> is found.
        /// </summary>
        /// <value><c>true</c> if found; otherwise, <c>false</c>.</value>
        public bool Found { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbExpressionFinder"/> class.
        /// </summary>
        /// <param name="toFind">To find.</param>
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

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            if (expression.ReferringColumn != null)
                expression.ReferringColumn.Expression = Visit(expression.ReferringColumn.Expression);   

            return base.VisitColumn(expression);
        }
    }
}
