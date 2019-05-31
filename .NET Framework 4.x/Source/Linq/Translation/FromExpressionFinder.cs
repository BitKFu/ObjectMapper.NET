using System;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// FromExpressionFinder
    /// </summary>
    public class FromExpressionFinder : DbExpressionVisitor
    {
        private readonly Type findType;
        private readonly ColumnDeclaration columnToFind;
        private readonly Expression expressionToFind;

        private AliasedExpression foundExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromExpressionFinder"/> class.
        /// </summary>
        /// <param name="find">The find.</param>
        private FromExpressionFinder(Type find)
        {
            findType = find;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromExpressionFinder"/> class.
        /// </summary>
        /// <param name="find">The find.</param>
        private FromExpressionFinder(ColumnDeclaration find)
        {
            columnToFind = find;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FromExpressionFinder"/> class.
        /// </summary>
        /// <param name="find">The find.</param>
        private FromExpressionFinder(Expression find)
        {
            expressionToFind = find;
        }

        /// <summary>
        /// Finds the specified search in.
        /// </summary>
        /// <param name="searchIn">The search in.</param>
        /// <param name="typeToFind">The type to find.</param>
        /// <returns></returns>
        public static AliasedExpression Find(Expression searchIn, Type typeToFind)
        {
            var fef = new FromExpressionFinder(typeToFind);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        /// <summary>
        /// Finds the specified search in.
        /// </summary>
        /// <param name="searchIn">The search in.</param>
        /// <param name="declaration">The declaration.</param>
        /// <returns></returns>
        public static AliasedExpression Find(Expression searchIn, ColumnDeclaration declaration)
        {
            var fef = new FromExpressionFinder(declaration);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        /// <summary>
        /// Finds the specified search in.
        /// </summary>
        /// <param name="searchIn">The search in.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static AliasedExpression Find(Expression searchIn, Expression expression)
        {
            var fef = new FromExpressionFinder(expression);
            fef.Visit(searchIn);
            return fef.foundExpression;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (foundExpression != null) 
                return exp;
            return base.Visit(exp);
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
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
