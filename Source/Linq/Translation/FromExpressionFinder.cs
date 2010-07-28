using System;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class FromExpressionFinder : DbExpressionVisitor
    {
        private readonly Type findType;
        private readonly ColumnDeclaration columnToFind;
        private readonly Expression expressionToFind;

        private AliasedExpression foundExpression;

        private FromExpressionFinder(Type find)
        {
            findType = find;
        }

        private FromExpressionFinder(ColumnDeclaration find)
        {
            columnToFind = find;
        }

        private FromExpressionFinder(Expression find)
        {
            expressionToFind = find;
        }

        public static AliasedExpression Find(Expression searchIn, Type typeToFind)
        {
            var fef = new FromExpressionFinder(typeToFind);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        public static AliasedExpression Find(Expression searchIn, ColumnDeclaration declaration)
        {
            var fef = new FromExpressionFinder(declaration);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        public static AliasedExpression Find(Expression searchIn, Expression expression)
        {
            var fef = new FromExpressionFinder(expression);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        protected override Expression Visit(Expression exp)
        {
            if (foundExpression != null) 
                return exp;
            return base.Visit(exp);
        }

        protected override Expression VisitTableExpression(TableExpression expression)
        {
            if (expression.RevealedType == findType)
            {
                foundExpression = expression;
                return expression;
            }

            if (expression.Columns.Contains(columnToFind))
            {
                foundExpression = expression;
                return expression;
            }


            var column = expression.Columns.Where(c => c.Expression is PropertyExpression
                                              ? c.Expression.Equals(expressionToFind)
                                              : DbExpressionComparer.AreEqual(c.Expression, expressionToFind)).FirstOrDefault();
            if (column != null)
            {
                foundExpression = expression;
                return expression;
            }

            return base.VisitTableExpression(expression);
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (select.RevealedType == findType)
            {
                foundExpression = select;
                return select;
            }

            if (select.Columns.Contains(columnToFind))
            {
                foundExpression = select;
                return select;
            }

            var column = select.Columns.Where(c => c.Expression is PropertyExpression
                                              ? c.Expression.Equals(expressionToFind)
                                              : DbExpressionComparer.AreEqual(c.Expression, expressionToFind)).FirstOrDefault();
            if (column != null)
            {
                foundExpression = select;
                return select;
            }

            return base.VisitSelectExpression(select);
        }

        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            if (union.RevealedType == findType)
            {
                foundExpression = union;
                return union;
            }

            if (union.Columns.Contains(columnToFind))
            {
                foundExpression = union;
                return union;
            }

            var column = union.Columns.Where(c => c.Expression is PropertyExpression
                                              ? c.Expression.Equals(expressionToFind)
                                              : DbExpressionComparer.AreEqual(c.Expression, expressionToFind)).FirstOrDefault();
            if (column != null)
            {
                foundExpression = union;
                return union;
            }

            return base.VisitUnionExpression(union);
        }
    }
}
