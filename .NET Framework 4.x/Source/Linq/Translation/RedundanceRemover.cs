using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This class is used to remove redundancies. That means, it removes Cleared Aliases from the Column 
    /// </summary>
    public abstract class RedundanceRemover : DbPackedExpressionVisitor
    {
        private readonly Dictionary<Alias, IDbExpressionWithResult> redundantSelect = new Dictionary<Alias, IDbExpressionWithResult>();

        /// <summary>
        /// 
        /// </summary>
        protected enum ReferenceDirection
        {
            /// <summary>
            /// 
            /// </summary>
            Forward,
            /// <summary>
            /// 
            /// </summary>
            Referrer
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        protected ReferenceDirection Direction { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RedundanceRemover(ReferenceDirection referenceDirection, ExpressionVisitorBackpack backpack)
            :base (backpack)
        {
            Direction = referenceDirection;
        }

        /// <summary>
        /// Accessor
        /// </summary>
        public Dictionary<Alias, IDbExpressionWithResult> RedundantSelect
        {
            get { return redundantSelect; }
        }

        /// <summary>
        /// Check the property expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            expression = (PropertyExpression) base.VisitColumn(expression);
            if (expression.ReferringColumn == null)
                return expression;

            // Maybe the expression itself is wrong.
            IDbExpressionWithResult newFromSelection;
            if (Direction == ReferenceDirection.Referrer && redundantSelect.TryGetValue(expression.Alias, out newFromSelection))
            {
                // Maybe we have to search for a referrer
                var referer =
                    newFromSelection.Columns.FirstOrDefault(
                        x => DbExpressionComparer.AreEqual(x.Expression, expression.ReferringColumn.Expression))
                ??
                    newFromSelection.Columns.FirstOrDefault(
                        x => x.OriginalProperty != null && expression.ReferringColumn.OriginalProperty != null &&
                            DbExpressionComparer.AreEqual(x.OriginalProperty, expression.ReferringColumn.OriginalProperty))
                ??
                    newFromSelection.Columns.FirstOrDefault(
                        x => expression.Name == x.Alias.Name);

                if (referer != null && string.IsNullOrEmpty(newFromSelection.Alias.Name))
                {
                    // Select the From Clause in which the column is contained
                    newFromSelection = newFromSelection.FromExpression.Where(from => from.Columns.Contains(referer)).First();
                }

                if (referer != null && !string.IsNullOrEmpty(newFromSelection.Alias.Name)
                    && !DbExpressionComparer.AreEqual(expression.ReferringColumn.Expression, referer.Expression)
                    )
                {
                    expression.ReferringColumn = referer;
                    var shortCut = expression.SetAlias(newFromSelection.Alias);
                    return Visit(shortCut);
                }
                else if (referer != null)
                {
                    var shortCut = referer.Expression;
                    var shortCutAliased = shortCut as AliasedExpression;
                    return Visit(shortCutAliased != null
                        ? shortCutAliased.SetType(expression.Type)
                        : referer.Expression);
                }
            }

            if (Direction == ReferenceDirection.Forward && redundantSelect.TryGetValue(expression.Alias, out newFromSelection))
            {
                // Now shortcut the ReferringColumn
                expression.ReferringColumn = FindSourceColumn(newFromSelection as AliasedExpression, expression.ReferringColumn);

                var refColumn = expression.ReferringColumn;
                if (refColumn == null)
                    return base.VisitColumn(expression);

                var refProperty = refColumn.Expression as PropertyExpression;
                if (refProperty == null)
                    return base.VisitColumn(expression);

                if (redundantSelect.TryGetValue(refProperty.Alias, out newFromSelection))
                {
                    var referer =
                        newFromSelection.Columns.FirstOrDefault(
                            x => DbExpressionComparer.AreEqual(x.Expression, refProperty.ReferringColumn.Expression))
                    ??
                        newFromSelection.Columns.FirstOrDefault(
                            x => x.OriginalProperty != null && expression.ReferringColumn.OriginalProperty != null &&
                                DbExpressionComparer.AreEqual(x.OriginalProperty, expression.ReferringColumn.OriginalProperty))
                    ??
                        newFromSelection.Columns.FirstOrDefault(
                            x => expression.Name == x.Alias.Name);

                    // Now shortcut the ReferringColumn
                    if (referer != null)
                        expression.ReferringColumn = referer;
                }

                return base.VisitColumn(expression);
            }

            return expression;
        }

    }
}
