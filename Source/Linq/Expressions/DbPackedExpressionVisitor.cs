using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// DbPackedExpressionVisitor
    /// </summary>
    public class DbPackedExpressionVisitor : DbExpressionVisitor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="backpack"></param>
        public DbPackedExpressionVisitor(ExpressionVisitorBackpack backpack)
        {
            this.backpack = backpack;
        }

        /// <summary>
        /// Gets the backpack.
        /// </summary>
        /// <value>The backpack.</value>
        public ExpressionVisitorBackpack Backpack
        {
            get { return backpack; }
        }

        private readonly ExpressionVisitorBackpack backpack;

        /// <summary> Visits the column declarations. </summary>
        protected override ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                var column = columns[i];
                var e = Visit(column.Expression);
                if (e == null)
                    continue;

                if (alternate == null && e != column.Expression)
                    alternate = columns.Take(i).ToList();

                if (alternate != null)
                {
                    var newCd = new ColumnDeclaration(e, column);
                    if (backpack.ColumnExchange.ContainsKey(column))
                        backpack.ColumnExchange.Remove(column);
                    backpack.ColumnExchange.Add(column, newCd);
                    alternate.Add(newCd);
                }
            }

            return alternate != null ? alternate.AsReadOnly() : columns;
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression expression)
        {
            Expression result = base.VisitSelectExpression(expression);

#if DEBUG
                // Check, if ater the SelectExpression the columns are valid
                if (!(this is ReferingColumnChecker))
                    ReferingColumnChecker.Validate(result as SelectExpression);
#endif
            return result;
        }

        /// <summary> Visits the column expression </summary>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            // Maybe we must exchange a referring column
            if (expression.ReferringColumn != null && backpack != null)
            {
                ColumnDeclaration exchangedCd;
                if (backpack.ColumnExchange.TryGetValue(expression.ReferringColumn, out exchangedCd))
                    expression.ReferringColumn = exchangedCd;
            }

            return expression;
        }

    }
}
