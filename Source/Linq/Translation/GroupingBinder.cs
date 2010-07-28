using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AdFactum.Data.Linq.Expressions;

namespace AdFactum.Data.Linq.Translation
{
    public class GroupingBinder : DbExpressionVisitor
    {
        private Type resultType;

        private GroupingBinder(Type resultType)
        {
#if TRACE
            Console.WriteLine("\nGroupingBinder:");
#endif
            this.resultType = resultType;
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static List<PropertyTupel> Evaluate(Type resultType, Expression expression)
        {
            var writer = new GroupingBinder(resultType);
            writer.Visit(expression);
            return writer.Groupings;
        }


        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected override Expression VisitMemberAccess(MemberExpression expr)
        {
            var propertyInfo = expr.Member as PropertyInfo;

            if (propertyInfo != null)
            {
                var tupel = new PropertyTupel(expr.Member.ReflectedType, expr.Member as PropertyInfo);
                Groupings.Add(tupel);
            }

            return base.VisitMemberAccess(expr);
        }
    }
}
