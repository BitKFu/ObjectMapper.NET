using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
    /// </summary>
    public class PartialEvaluator : DbExpressionVisitor
    {
        /// <summary>
        /// Evals the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static Expression Eval(Expression expression)
        {
            var partial = new PartialEvaluator();
            return partial.Visit(expression);
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
             Type type = m.Type;

             if (!(type.ImplementsInterface(typeof(IQueryable))))
                 return base.VisitMethodCall(m);

             if (m.Method.ReflectedType == typeof(Queryable) 
              || m.Method.ReflectedType == typeof(LinqExtensions))
                 return base.VisitMethodCall(m);

             Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(m);

             Func<object> fn = lambda.Compile();
             return Expression.Constant(fn(), type);        
        }
    }

//    /// <summary>
//    /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
//    /// </summary>
//    public static class PartialEvaluator
//    {
//        /// <summary>
//        /// Performs evaluation & replacement of independent sub-trees
//        /// </summary>
//        /// <param name="expression">The root of the expression tree.</param>
//        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
//        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
//        public static Expression Eval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
//        {
//            return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression), expression);
//        }

//        /// <summary>
//        /// Performs evaluation & replacement of independent sub-trees
//        /// </summary>
//        /// <param name="expression">The root of the expression tree.</param>
//        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
//        public static Expression Eval(Expression expression)
//        {
//            return Eval(expression, CanBeEvaluatedLocally);
//        }

//       private static bool CanBeEvaluatedLocally(Expression expression)
//       {
//           return expression is ConstantExpression || expression is MemberExpression ||
//                  expression is MethodCallExpression;
//       }

//        #region Nested type: Nominator

//        /// <summary>
//        /// Performs bottom-up analysis to determine which nodes can possibly
//        /// be part of an evaluated sub-tree.
//        /// </summary>
//        private class Nominator : DbExpressionVisitor
//        {
//            private readonly HashSet<Expression> candidates;
//            private bool cannotBeEvaluated;
//            private readonly Func<Expression, bool> fnCanBeEvaluated;

//            private Nominator(Func<Expression, bool> fnCanBeEvaluated)
//            {
//                candidates = new HashSet<Expression>();
//                this.fnCanBeEvaluated = fnCanBeEvaluated;
//            }

//            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression)
//            {
//                var nominator = new Nominator(fnCanBeEvaluated);
//                nominator.Visit(expression);
//                return nominator.candidates;
//            }


//            protected override Expression Visit(Expression expression)
//            {
//                if (expression != null)
//                {
//                    bool saveCannotBeEvaluated = cannotBeEvaluated;
//                    cannotBeEvaluated = false;
//                    base.Visit(expression);
//                    if (!cannotBeEvaluated)
//                    {
//                        if (fnCanBeEvaluated(expression))
//                            candidates.Add(expression);
//                        else
//                            cannotBeEvaluated = true;
//                    }
//                    cannotBeEvaluated |= saveCannotBeEvaluated;
//                }
//                return expression;
//            }
//        }

//        #endregion

//        #region Nested type: SubtreeEvaluator

//        /// <summary>
//        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
//        /// </summary>
//        private class SubtreeEvaluator : DbExpressionVisitor
//        {
//            private readonly HashSet<Expression> candidates;

//            private SubtreeEvaluator(HashSet<Expression> candidates)
//            {
//                this.candidates = candidates;
//            }

//            internal static Expression Eval(HashSet<Expression> candidates, Expression exp)
//            {
//                return new SubtreeEvaluator(candidates).Visit(exp);
//            }

//            protected override Expression Visit(Expression exp)
//            {
//                return exp == null ? null : (candidates.Contains(exp) ? Evaluate(exp) : base.Visit(exp));
//            }

//            private static Expression Evaluate(Expression e)
//            {
//                Type type = e.Type;

//                if (!(type.ImplementsInterface(typeof(IQueryable))))
//                    return e;

//                var me = e as MethodCallExpression;
//                if (me == null || me.Method.ReflectedType == typeof(Queryable) || me.Method.ReflectedType == typeof(LinqExtensions))
//                    return e;

//                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);

//                Func<object> fn = lambda.Compile();
//                return Expression.Constant(fn(), type);
//            }
//        }
//        #endregion
//    }
}