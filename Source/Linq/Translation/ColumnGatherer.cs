using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class ColumnGatherer : DbExpressionVisitor
    {
        private List<ColumnDeclaration> foundColumns = new List<ColumnDeclaration>();

        public List<ColumnDeclaration> Columns { get { return foundColumns; } }

        private int maxLevel = int.MaxValue;
        private int curLevel;

        private ColumnGatherer(int level)
        {
            maxLevel = level;
        }

        private ColumnGatherer()
        {
        }

        public static List<ColumnDeclaration> Gather (Expression expression, int maxLevel)
        {
            var gatherer = new ColumnGatherer(maxLevel);
            gatherer.Visit(expression);
            return gatherer.Columns;
        }

        public static List<ColumnDeclaration> Gather(Expression expression)
        {
            var gatherer = new ColumnGatherer();
            gatherer.Visit(expression);
            return gatherer.Columns;
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (curLevel >= maxLevel)
                return select;

            AddColumns(select.Columns);
            try
            {
                curLevel++;
                return base.VisitSelectExpression(select);
            }
            finally { curLevel--; }
        }

        private void AddColumns(ReadOnlyCollection<ColumnDeclaration> declarations)
        {
            foundColumns.AddRange(declarations);
        }

        protected override Expression VisitTableExpression(TableExpression expression)
        {
            if (curLevel >= maxLevel)
                return expression;

            AddColumns(expression.Columns);
            try
            {
                curLevel++;
                return base.VisitTableExpression(expression);
            }
            finally { curLevel--; }
        }
    }
}
