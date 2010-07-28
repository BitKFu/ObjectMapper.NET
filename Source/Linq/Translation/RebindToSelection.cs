using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class RebindToSelection : DbExpressionVisitor
    {
        AliasedExpression Selection { get; set; }
        HashSet<Alias> AliasesToReplace { get; set; }
        AliasedExpression currentFrom;

        /// <summary>
        /// Initializes an instance of the RebindToSelection class
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="newSelection">The new selection.</param>
        public RebindToSelection(AliasedExpression currentFrom, AliasedExpression newSelection)
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
        private RebindToSelection(AliasedExpression currentFrom, AliasedExpression newSelection, HashSet<Alias> aliasesToReplace)
            : this(currentFrom, newSelection)
        {
            AliasesToReplace = aliasesToReplace;
        }

        /// <summary>
        /// Rebinds all properties within the expression to a new selection
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static Expression Rebind(AliasedExpression currentFrom, AliasedExpression selection, Expression exp)
        {
            var rebinder = new RebindToSelection(currentFrom, selection);
            return rebinder.Visit(exp);
        }

        public static Expression Rebind(AliasedExpression currentFrom, AliasedExpression selection, Expression exp, HashSet<Alias> aliasesToReplace)
        {
            var rebinder = new RebindToSelection(currentFrom, selection, aliasesToReplace);
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
                var accordingFromClause = FromExpressionFinder.Find(currentFrom, expression);
                return new PropertyExpression(Selection, FindSourceColumn(accordingFromClause, expression)).SetType(expression.Type);
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
