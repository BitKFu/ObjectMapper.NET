using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq
{
    /// <summary>Used to take lambda expressions with zero parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TResult>(TMapper mapper);

    /// <summary>Used to take lambda expressions with one parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TResult>(TMapper mapper, TArg1 arg);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TResult>(TMapper mapper, TArg1 arg, TArg2 arg2);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TArg4, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3, TArg4 arg4);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7);

    /// <summary>Used to take lambda expressions with two parameter</summary>
    public delegate TResult OrmDelegate<TMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
        TMapper mapper, TArg1 arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8);

    /// <summary>
    ///  This class is used to compile the queries
    /// </summary>
    public class QueryCompiler
    {
        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TResult> Compile<TResult>(
            Expression<Func<TResult>> query)
        {
            return new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate<TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TResult> Compile<TArg1, TResult>(
            Expression<OrmDelegate<TArg1, TResult>> query)
        {
            return new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate<TArg1, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TResult> Compile<TArg1, TArg2, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TResult>> query)
        {
            return new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate<TArg1, TArg2, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TResult> Compile<TArg1, TArg2, TArg3, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TResult>> query)
        {
            return new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate<TArg1, TArg2, TArg3, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TArg4, TResult> Compile
            <TArg1, TArg2, TArg3, TArg4, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TArg4, TResult>> query)
        {
            return new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate<TArg1, TArg2, TArg3, TArg4, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Compile
            <TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> query)
        {
            return
                new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate
                    <TArg1, TArg2, TArg3, TArg4, TArg5, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Compile
            <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> query)
        {
            return
                new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate
                    <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TArg7">The type of the arg7.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Compile
            <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> query)
        {
            return
                new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate
                    <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TArg7">The type of the arg7.</typeparam>
        /// <typeparam name="TArg8">The type of the arg8.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static OrmDelegate<ObjectMapper, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Compile
            <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
            Expression<OrmDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> query)
        {
            return
                new CompiledQuery(query).PrepareQuery<TResult>().CreateDelegate
                    <TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>;
        }
    }


    /// <summary>
    /// This object is used to store a compiled query
    /// </summary>
    public class CompiledQuery
    {
        private readonly LambdaExpression query;
        private ILinqQueryProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledQuery"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public CompiledQuery(LambdaExpression expression)
        {
            query = expression;
        }

        /// <summary>
        /// Executes the query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private TResult Execute<TResult>(ObjectMapper mapper, params object[] args)
        {
            // Query must be accessed uncompiled, because the result is of the type IQueryable
            if (typeof (TResult).IsGenericType && typeof (TResult).GetGenericTypeDefinition() == typeof (IQueryable<>))
            {
                Type contentType = typeof (TResult).GetGenericArguments()[0];
                Type queryType = typeof (Query<>).MakeGenericType(contentType);

                var replaced = (LambdaExpression) ReplaceQueryArguments(args);
                provider = (ILinqQueryProvider) Activator.CreateInstance(queryType, mapper);

                return (TResult) provider.CreateQuery(replaced.Body);
            }

            // Try to solve provider
            var localProvider = (Query<TResult>) provider;

            IDbCommand command;
            localProvider = localProvider.RebindStatement(mapper, args, out command);
            var result = localProvider.ExecuteCommand<TResult>(command);

            return result;
        }

        /// <summary>
        /// Prepares the query
        /// </summary>
        public CompiledQuery PrepareQuery<TResult>()
        {
            provider = QueryProviderFinder.Find(query);
            if (provider == null)
                throw new InvalidOperationException("Can't precompile due to missing LinqProvider.");

            provider = new Query<TResult>(provider.Mapper);
            provider.PreCompile(query);
            provider.MarkAsTemplate();
            return this;
        }

        /// <summary>
        /// Returns the stored sql command of a compiled query
        /// </summary>
        public string StoredSqlCommand
        {
            get
            {
                return provider != null ? provider.StoredSqlCommand : string.Empty;
            }
        }

        /// <summary>
        /// Returns the stored sql command id
        /// </summary>
        /// <value>The stored SQL id.</value>
        public string StoredSqlId
        {
            get
            {
                return provider != null ? provider.StoredSqlId : null;
            }
        }


        /// <summary>
        /// This method is used to replace the query arguments within the expression tree
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private Expression ReplaceQueryArguments(object[] args)
        {
            // Only replace single parameters
            return ExpressionReplacer.ReplaceAll(
                query,
                query.Parameters.ToArray(),
                args.Select((a, i) =>
                                {
                                    if (a.GetType().IsListType())
                                    {
                                        Type listType = a.GetType().UnpackType()[1];
                                        var parameters = new List<SqlParameterExpression>();
                                        int counter = 0;
                                        foreach (object argument in (IEnumerable) a)
                                        {
                                            parameters.Add(new SqlParameterExpression(
                                                               listType, argument,
                                                               string.Concat(query.Parameters[i].Name, counter)));
                                            counter++;
                                        }

                                        if (a.GetType().IsArray)
                                            return Expression.NewArrayInit(listType, parameters.ToArray());

                                        return Expression.ListInit(Expression.New(a.GetType()), parameters.ToArray());
                                    }

                                    return (Expression)
                                        new SqlParameterExpression(query.Parameters[i].Type, a, query.Parameters[i].Name);
                                }
                    ).ToArray()
                );
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TResult>(ObjectMapper mapper)
        {
            return Execute<TResult>(mapper, null);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TResult>(ObjectMapper mapper, TArg1 arg1)
        {
            return Execute<TResult>(mapper, arg1);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TResult>(ObjectMapper mapper, TArg1 arg1, TArg2 arg2)
        {
            return Execute<TResult>(mapper, arg1, arg2);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TResult>(ObjectMapper mapper, TArg1 arg1, TArg2 arg2,
                                                                    TArg3 arg3)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TArg4, TResult>(ObjectMapper mapper, TArg1 arg1, TArg2 arg2,
                                                                           TArg3 arg3, TArg4 arg4)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        /// <param name="arg5">The arg5.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(ObjectMapper mapper, TArg1 arg1,
                                                                                  TArg2 arg2, TArg3 arg3, TArg4 arg4,
                                                                                  TArg5 arg5)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        /// <param name="arg5">The arg5.</param>
        /// <param name="arg6">The arg6.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(ObjectMapper mapper, TArg1 arg1,
                                                                                         TArg2 arg2, TArg3 arg3,
                                                                                         TArg4 arg4, TArg5 arg5,
                                                                                         TArg6 arg6)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TArg7">The type of the arg7.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        /// <param name="arg5">The arg5.</param>
        /// <param name="arg6">The arg6.</param>
        /// <param name="arg7">The arg7.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(ObjectMapper mapper,
                                                                                                TArg1 arg1, TArg2 arg2,
                                                                                                TArg3 arg3, TArg4 arg4,
                                                                                                TArg5 arg5, TArg6 arg6,
                                                                                                TArg7 arg7)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TArg5">The type of the arg5.</typeparam>
        /// <typeparam name="TArg6">The type of the arg6.</typeparam>
        /// <typeparam name="TArg7">The type of the arg7.</typeparam>
        /// <typeparam name="TArg8">The type of the arg8.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapper">The mapper.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        /// <param name="arg5">The arg5.</param>
        /// <param name="arg6">The arg6.</param>
        /// <param name="arg7">The arg7.</param>
        /// <param name="arg8">The arg8.</param>
        /// <returns></returns>
        public TResult CreateDelegate<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
            ObjectMapper mapper, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7,
            TArg8 arg8)
        {
            return Execute<TResult>(mapper, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
    }
}