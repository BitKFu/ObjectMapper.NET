using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This Rewriter moves the SqlId from the lowest sql to the highest one
    /// </summary>
    public class SqlIdRewriter : DbPackedExpressionVisitor
    {
        private Expression root;
        private string storedSqlId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlIdRewriter"/> class.
        /// </summary>
        protected SqlIdRewriter(Expression rootEx, ExpressionVisitorBackpack backpack)
            : base(backpack)
        {
#if TRACE
            Console.WriteLine("\nSqlIdRewriter:");
#endif
            root = rootEx;
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <value>The root.</value>
        protected Expression Root
        {
            get { return root; }
        }

        /// <summary>
        /// Rewrites the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, ExpressionVisitorBackpack backpack)
        {
            Expression rootSelect = ExpressionTypeFinder.Find(expression, typeof (SelectExpression));
            var writer = new SqlIdRewriter(rootSelect, backpack);
            return writer.Visit(expression);
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            bool isRoot = select == Root;

            if ((isRoot && !string.IsNullOrEmpty(select.SqlId)) || !string.IsNullOrEmpty(storedSqlId))
                return select;

            // Maybe we have to store a SqlId
            if (!string.IsNullOrEmpty(select.SqlId))
                storedSqlId = select.SqlId;

            select = (SelectExpression)base.VisitSelectExpression(select);

            // Maybe we are back at the root level, than take the stored id
            if (isRoot && !string.IsNullOrEmpty(storedSqlId))
                return UpdateSelect(select, select.Projection, select.Selector, select.From, select.Where,
                                    select.OrderBy, select.GroupBy, select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                    select.Columns, storedSqlId, select.Hint, select.DefaultIfEmpty);

            // Delete the SqlId, if we go back
            if (!string.IsNullOrEmpty(select.SqlId))
                return UpdateSelect(select, select.Projection, select.Selector, select.From, select.Where,
                                    select.OrderBy, select.GroupBy, select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                    select.Columns, null, select.Hint, select.DefaultIfEmpty);

            return select;
        }
    }
}
