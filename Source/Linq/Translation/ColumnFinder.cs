using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class ColumnFinder : DbExpressionVisitor
    {
        private Expression searchFor;
        private ColumnDeclaration foundDelcaration;

        private ColumnFinder(Expression findInColumn)
        {
            searchFor = findInColumn;
        }

        public static ColumnDeclaration FindColumn(Expression root, Expression findInColumn)
        {
            var finder = new ColumnFinder(findInColumn);
            finder.Visit(root);
            return finder.foundDelcaration;
        }

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

        protected override Expression Visit(Expression exp)
        {
            if (foundDelcaration != null)
                return exp;

            return base.Visit(exp);
        }
    }
}
