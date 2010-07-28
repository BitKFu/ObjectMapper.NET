using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    public class QueryProviderFinder : DbExpressionVisitor
    {
        /// <summary> Gets the member access. </summary>
        protected Stack<IRetriever> MemberAccess { get { return memberAccess; } }
        private readonly Stack<IRetriever> memberAccess = new Stack<IRetriever>();

        public ILinqQueryProvider LinqProvider { get; private set; }

        public static ILinqQueryProvider Find(Expression toSearchIn)
        {
            var opf = new QueryProviderFinder();
            opf.Visit(toSearchIn);
            return opf.LinqProvider;
        }

        protected override Expression Visit(Expression exp)
        {
            return LinqProvider != null ? exp : base.Visit(exp);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is ILinqQueryProvider)
                LinqProvider = c.Value as ILinqQueryProvider;

            if (c.Value != null)
            {
                IRetriever retriever;
                object value = c.Value;
                while (MemberAccess.Count > 0)
                {
                    retriever = MemberAccess.Pop();
                    value = retriever.GetValue(value);

                    if (value is ILinqQueryProvider)
                        LinqProvider = value as ILinqQueryProvider;
                }
            }
            return c;
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression me)
        {
            if (me != null && me.Method.ReturnType.ImplementsInterface(typeof(IQueryable)))
            {
                Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(me);

                try
                {
                    Func<object> fn = lambda.Compile();
                    LinqProvider = fn() as ILinqQueryProvider;
                }
                catch(InvalidOperationException)
                {
                    // Can't do a precompile due to missing parameters
                }
            }

            return base.VisitMethodCall(me);
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var retriever = GetRetriever(m);

            if (retriever != null && retriever.Target != typeof(DateTime) && retriever.Target != typeof(string))
                MemberAccess.Push(retriever);

            return base.VisitMemberAccess(m);
        }

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            MemberAccess.Clear();
            return base.VisitParameter(p);
        }
    }
}
