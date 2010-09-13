using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// This class Removes a subquery from the expression tree
    /// </summary>
    public class SubqueryRemover : RedundanceRemover
    {
        private List<SelectExpression> Redundant { get; set; }
        //private readonly Dictionary<Alias, IDbExpressionWithResult> redundantSelect = new Dictionary<Alias, IDbExpressionWithResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubqueryRemover"/> class.
        /// </summary>
        /// <param name="redundant">The redundant.</param>
        private SubqueryRemover(List<SelectExpression> redundant)
            :base(ReferenceDirection.Backward)
        {
            Redundant = redundant;
        }

        /// <summary>
        /// Removes the specified select.
        /// </summary>
        public static SelectExpression Remove(SelectExpression select, Cache<Type, ProjectionClass> dynamicCache, List<SelectExpression> redundant)
        {
            return (SelectExpression) new SubqueryRemover(redundant).Visit(select);
        }

        /// <summary>
        /// Removes the specified select.
        /// </summary>
        public static SelectExpression Remove(SelectExpression select, Cache<Type, ProjectionClass> dynamicCache, params SelectExpression[] redundant)
        {
            return (SelectExpression) new SubqueryRemover(redundant.ToList()).Visit(select);
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (Redundant.Contains(select))
            {
                var result = Visit(select.From);
                RedundantSelect.Add(select.Alias, (IDbExpressionWithResult) result);
                return result;
            }

            return base.VisitSelectExpression(select);
        }

        ///// <summary>
        ///// Check the property expressions
        ///// </summary>
        ///// <param name="expression"></param>
        ///// <returns></returns>
        //protected override Expression VisitColumn(PropertyExpression expression)
        //{
        //    IDbExpressionWithResult newFromSelection;
        //    if (redundantSelect.TryGetValue(expression.Alias, out newFromSelection))
        //    {
        //        // Shortcut the expression.
        //        var shortCut = expression.ReferringColumn.Expression;
        //        if (shortCut.Type != expression.Type)
        //        {
        //            // Maybe we have to adjust the type
        //            var aliasedExpression = shortCut as AliasedExpression;
        //            if (aliasedExpression != null)
        //                shortCut = aliasedExpression.SetType(expression.Type);
        //        }

        //        return Visit(shortCut);
        //    }

        //    return base.VisitColumn(expression);
        //}

    }
}
