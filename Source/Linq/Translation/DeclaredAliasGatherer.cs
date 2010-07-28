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

        private DeclaredAliasGatherer()
        {
            aliases = new HashSet<Alias>();
        }

        public static HashSet<Alias> Gather(Expression source)
        {
            var gatherer = new DeclaredAliasGatherer();
            gatherer.Visit(source);
            return gatherer.aliases;
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitTableExpression(TableExpression table)
        {
            aliases.Add(table.Alias);
            return table;
        }

        protected override Expression VisitColumn(PropertyExpression expression)
        {
            aliases.Add(expression.Alias);
            return expression;
        }
    }
}