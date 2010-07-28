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

        private ReferencedAliasGatherer()
        {
            aliases = new HashSet<Alias>();
        }

        public static HashSet<Alias> Gather(Expression source)
        {
            var gatherer = new ReferencedAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        protected override Expression VisitColumn(PropertyExpression column)
        {
            aliases.Add(column.Alias);
            return column;
        }
    }
}