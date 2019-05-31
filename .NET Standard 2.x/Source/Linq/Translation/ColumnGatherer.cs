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
    /// 
    /// </summary>
    public class ColumnGatherer : DbExpressionVisitor
    {
        private List<ColumnDeclaration> foundColumns = new List<ColumnDeclaration>();

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public List<ColumnDeclaration> Columns { get { return foundColumns; } }

        private int maxLevel = int.MaxValue;
        private int curLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnGatherer"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        private ColumnGatherer(int level)
        {
            maxLevel = level;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnGatherer"/> class.
        /// </summary>
        private ColumnGatherer()
        {
        }

        /// <summary>
        /// Gathers the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="maxLevel">The max level.</param>
        /// <returns></returns>
        public static List<ColumnDeclaration> Gather (Expression expression, int maxLevel)
        {
            var gatherer = new ColumnGatherer(maxLevel);
            gatherer.Visit(expression);
            return gatherer.Columns;
        }

        /// <summary>
        /// Gathers the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static List<ColumnDeclaration> Gather(Expression expression)
        {
            var gatherer = new ColumnGatherer();
            gatherer.Visit(expression);
            return gatherer.Columns;
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the columns.
        /// </summary>
        /// <param name="declarations">The declarations.</param>
        private void AddColumns(ReadOnlyCollection<ColumnDeclaration> declarations)
        {
            foundColumns.AddRange(declarations);
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
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
