using System.Collections.Generic;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class DeclaredAliasGatherer : DbExpressionVisitor
    {
        readonly HashSet<Alias> aliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclaredAliasGatherer"/> class.
        /// </summary>
        private DeclaredAliasGatherer()
        {
            aliases = new HashSet<Alias>();
        }

        /// <summary>
        /// Gathers the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static HashSet<Alias> Gather(Expression source)
        {
            var gatherer = new DeclaredAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            aliases.Add(select.Alias);
            return select;
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        protected override Expression VisitTableExpression(TableExpression table)
        {
            aliases.Add(table.Alias);
            return table;
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            aliases.Add(expression.Alias);
            return expression;
        }
    }
}