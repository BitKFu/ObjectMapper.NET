using System.Collections.Generic;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    public class ReferencedAliasGatherer : DbExpressionVisitor
    {
        readonly HashSet<Alias> aliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencedAliasGatherer"/> class.
        /// </summary>
        private ReferencedAliasGatherer()
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
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        /// <summary>
        /// Visits the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression column)
        {
            aliases.Add(column.Alias);
            return column;
        }
    }
}