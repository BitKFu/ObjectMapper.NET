using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// ColumnFinder
    /// </summary>
    public class ColumnFinder : DbExpressionVisitor
    {
        private Expression searchFor;
        private ColumnDeclaration foundDelcaration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnFinder"/> class.
        /// </summary>
        /// <param name="findInColumn">The find in column.</param>
        private ColumnFinder(Expression findInColumn)
        {
            searchFor = findInColumn;
        }

        /// <summary>
        /// Finds the column.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="findInColumn">The find in column.</param>
        /// <returns></returns>
        public static ColumnDeclaration FindColumn(Expression root, Expression findInColumn)
        {
            var finder = new ColumnFinder(findInColumn);
            finder.Visit(root);
            return finder.foundDelcaration;
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            foreach (var column in select.Columns)
            {
                if (column.Expression == searchFor)
                {
                    foundDelcaration = column;
                    return select;
                }
            }

            return base.VisitSelectExpression(select);
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (foundDelcaration != null)
                return exp;

            return base.Visit(exp);
        }
    }
}
