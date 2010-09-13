using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Thrown if the column validation fails
    /// </summary>
    public class NoValidReferingColumnFound : MapperBaseException
    {
        private const string MESSAGE = "No valid column found in source select:\nColumn: {0}\nSource: {1}";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="source"></param>
        public NoValidReferingColumnFound(ColumnDeclaration searchFor, IDbExpressionWithResult source)
            :base(string.Format(MESSAGE, searchFor, source))
        {
            
        }
    }

    /// <summary>
    /// This class is used to check, if the refering columns matches the from clause of a select expression.
    /// It's only useful when debugging Linq
    /// </summary>
    public class ReferingColumnChecker : DbExpressionVisitor
    {
        /// <summary>
        /// Starts the validation
        /// </summary>
        /// <param name="expression"></param>
        public static void Validate(Expression expression)
        {
#if DEBUG
            new ReferingColumnChecker().Visit(expression);
#endif
        }

        /// <summary>
        /// Checks all columns of the select expression against it's from clause columns
        /// </summary>
        protected override Expression VisitSelectExpression(SelectExpression expression)
        {
            var result = base.VisitSelectExpression(expression);
            var select = result as SelectExpression;
            if (select == null || select.From == null)
                return result;

            var from = select.From as IDbExpressionWithResult;
            if (from == null)
                return result;

            foreach (var column in select.Columns)
            {
                var property = column.Expression as PropertyExpression;
                if (property == null || property.ReferringColumn == null)
                    continue;

                var refColumn = property.ReferringColumn;
                if (!from.Columns.Any(fc => fc.Equals(refColumn) ||  // First, use a fast equal
                                            DbExpressionComparer.AreEqual(fc.Expression, refColumn.Expression, false))) // sometimes, that's not enough
                    throw new NoValidReferingColumnFound(refColumn, from);
            }

            // If we don't have an order by - everything is ok
            if (select.OrderBy == null)
                return result;

            foreach (var orderBy in select.OrderBy)
            {
                var property = orderBy.Expression as PropertyExpression;
                if (property == null || property.ReferringColumn == null)
                    continue;

                var refColumn = property.ReferringColumn;
                if (!from.Columns.Any(fc => fc.Equals(refColumn) ||  // First, use a fast equal
                                            DbExpressionComparer.AreEqual(fc.Expression, refColumn.Expression, false))) // sometimes, that's not enough
                    throw new NoValidReferingColumnFound(refColumn, from);
            }

            return result;
        }
    }
}
