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

        /// <summary>
        /// Initializes a new instance of the <see cref="SubqueryRemover"/> class.
        /// </summary>
        /// <param name="redundant">The redundant.</param>
        private SubqueryRemover(List<SelectExpression> redundant)
            :base(ReferenceDirection.Referrer)
        {
            Redundant = redundant;
        }

        /// <summary>
        /// Removes the specified select.
        /// </summary>
        public static SelectExpression Remove(SelectExpression select, Cache<Type, ProjectionClass> dynamicCache, List<SelectExpression> redundant)
        {
            var result= (SelectExpression) new SubqueryRemover(redundant).Visit(select);
            return result;
        }   

        /// <summary>
        /// Removes the specified select.
        /// </summary>
        public static SelectExpression Remove(SelectExpression select, Cache<Type, ProjectionClass> dynamicCache, params SelectExpression[] redundant)
        {
            var result = (SelectExpression) new SubqueryRemover(redundant.ToList()).Visit(select);
            return result;
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
                RedundantSelect.Add(select.Alias, (IDbExpressionWithResult)result);
                return result;
            }

            var resultEx = (AliasedExpression)base.VisitSelectExpression(select);
            return resultEx;
        }

        protected override Expression VisitColumn(PropertyExpression expression)
        {
            var result= base.VisitColumn(expression);
            return result;
        }
    }
}
