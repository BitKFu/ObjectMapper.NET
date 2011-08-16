using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// RebindToSelection
    /// </summary>
    public class RebindToSelection : DbPackedExpressionVisitor
    {
        AliasedExpression Selection { get; set; }
        HashSet<Alias> AliasesToReplace { get; set; }
        AliasedExpression currentFrom;

        /// <summary>
        /// Initializes an instance of the RebindToSelection class
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="newSelection">The new selection.</param>
        /// <param name="backpack">The backpack.</param>
        public RebindToSelection(AliasedExpression currentFrom, AliasedExpression newSelection, ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
            Selection = newSelection;
            this.currentFrom = currentFrom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RebindToSelection"/> class.
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="newSelection">The new selection.</param>
        /// <param name="aliasesToReplace">The aliases to replace.</param>
        /// <param name="backpack">The backpack.</param>
        private RebindToSelection(AliasedExpression currentFrom, AliasedExpression newSelection, HashSet<Alias> aliasesToReplace, ExpressionVisitorBackpack backpack)
            : this(currentFrom, newSelection, backpack)
        {
            AliasesToReplace = aliasesToReplace;
        }

        /// <summary>
        /// Rebinds all properties within the expression to a new selection
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="selection">The selection.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns></returns>
        public static Expression Rebind(AliasedExpression currentFrom, AliasedExpression selection, Expression exp, ExpressionVisitorBackpack backpack)
        {
            var rebinder = new RebindToSelection(currentFrom, selection, backpack);
            return rebinder.Visit(exp);
        }

        /// <summary>
        /// Rebinds the specified current from.
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="selection">The selection.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="aliasesToReplace">The aliases to replace.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns></returns>
        public static Expression Rebind(AliasedExpression currentFrom, AliasedExpression selection, Expression exp, HashSet<Alias> aliasesToReplace, ExpressionVisitorBackpack backpack)
        {
            var rebinder = new RebindToSelection(currentFrom, selection, aliasesToReplace, backpack);
            return rebinder.Visit(exp);
        }

        /// <summary>
        /// Rebinds the column to the new selection
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            if (AliasesToReplace == null || AliasesToReplace.Contains(expression.Alias))
            {
                var expWithResult = FromExpressionFinder.Find(currentFrom, expression) as IDbExpressionWithResult;
                if (expWithResult == null)
                    return expression;

                var sourceColumn = expWithResult.Columns.Where(c => c.Expression.Equals(expression)).First();

                return string.IsNullOrEmpty(Selection.Alias.Name)
                           ? new PropertyExpression(expWithResult as AliasedExpression, sourceColumn).SetType(expression.Type)
                           : new PropertyExpression(Selection, sourceColumn).SetType(expression.Type);
            }
            else
                return expression;
        }


        /// <summary>
        /// Visits the scalar expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitScalarExpression(ScalarExpression expression)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                currentFrom = VisitSource(expression.From);
                var columns = VisitColumnDeclarations(expression.Columns);

                return UpdateScalarExpression(expression, columns.First(), currentFrom);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                var left = VisitSource(join.Left);
                currentFrom = left;

                var right = VisitSource(join.Right);
                var condition = Visit(join.Condition);
                return UpdateJoin(join, join.Join, left, right, condition);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                return base.VisitUnionExpression(union);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                return base.VisitSelectExpression(select);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

    }
}
