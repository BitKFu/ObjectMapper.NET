using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq
{
    /// <summary>
    /// Linq Extensions used by the ObjectMapper .NET
    /// </summary>
    public static class LinqExtensions
    {
        #region 'First' Aggregation

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static decimal First<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static double First<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int First<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static long First<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Single First<TSource>(this IEnumerable<TSource> source, Func<TSource, Single> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static string First<TSource>(this IEnumerable<TSource> source, Func<TSource, string> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Guid First<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid> selector)
        {
            return source.Select(selector).First();
        }

        /*
         * Nullable types
         */

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static decimal? First<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static double? First<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int? First<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static long? First<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Single? First<TSource>(this IEnumerable<TSource> source, Func<TSource, Single?> selector)
        {
            return source.Select(selector).First();
        }

        /// <summary>
        /// Select the first value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Guid? First<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid?> selector)
        {
            return source.Select(selector).First();
        }
        #endregion

        #region 'Last' Aggregation

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static decimal Last<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static double Last<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Last<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static long Last<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Single Last<TSource>(this IEnumerable<TSource> source, Func<TSource, Single> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static string Last<TSource>(this IEnumerable<TSource> source, Func<TSource, string> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Guid Last<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid> selector)
        {
            return source.Select(selector).Last();
        }

        /*
         * Nullable types
         */

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static decimal? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static double? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static long? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Single? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, Single?> selector)
        {
            return source.Select(selector).Last();
        }

        /// <summary>
        /// Select the Last value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static Guid? Last<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid?> selector)
        {
            return source.Select(selector).Last();
        }
        #endregion

        #region 'Count' Aggregation

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, Single> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, string> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid> selector)
        {
            return source.Select(selector).Count();
        }

        /*
         * Nullable types
         */

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, Single?> selector)
        {
            return source.Select(selector).Count();
        }

        /// <summary>
        /// Select the Count value.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, Guid?> selector)
        {
            return source.Select(selector).Count();
        }
        #endregion

        #region 'Level' Aggregation

        /// <summary>
        /// Levels the specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public static IQueryable<TSource> Level<TSource>(this IQueryable<TSource> source, int level)
        {
            var query = source as Query<TSource>;
            if (query == null) return source;

            MethodInfo method = query.GetType().GetMethod("Level");
            
            var call = Expression.Call(Expression.Constant(query), method, new[]{query.Expression, Expression.Constant(level)});
            return query.CreateQuery<TSource>(call); 
        }

        #endregion  

        #region Linq Extensions


        /// <summary>
        /// Adds a SqlId to the select
        /// </summary>
        public static IQueryable<TSource> SqlId<TSource>(this IQueryable<TSource> source, string sqlId)
        {
            var query = source as Query<TSource>;
            if (query == null) return source;

            MethodInfo method = query.GetType().GetMethod("SqlId", BindingFlags.NonPublic | BindingFlags.Instance);

            var call = Expression.Call(Expression.Constant(query), method, new[] { query.Expression, Expression.Constant(sqlId) });
            return query.CreateQuery<TSource>(call);
        }

        /// <summary>
        /// Adds a Hint to the select
        /// </summary>
        public static IQueryable<TSource> Hint<TSource>(this IQueryable<TSource> source, string hint)
        {
            var query = source as Query<TSource>;
            if (query == null) return source;

            MethodInfo method = query.GetType().GetMethod("Hint", BindingFlags.NonPublic | BindingFlags.Instance);

            var call = Expression.Call(Expression.Constant(query), method, new[] { query.Expression, Expression.Constant(hint) });
            return query.CreateQuery<TSource>(call);
        }

        /// <summary> Used to execute a explicit like condition, without adding "%" to begin and end "%" </summary>
        public static bool Like(this string searchIn, string searchFor)
        {
            return searchIn == searchFor;
        }

        /// <summary>
        /// Explicits the specified explicit parameter.
        /// </summary>
        public static T Explicit<T>(this T explicitParameter)
        {
            return explicitParameter;
        }

        #endregion

        #region Type Queries

        /// <summary>
        /// Determines whether [is grouping type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is grouping type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGroupingType (this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IGrouping<,>));
        }

        /// <summary>
        /// Determines whether [is anonymous type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is anonymous type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAnonymousType(this Type type)
        {
            return type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length>0;
        }

        /// <summary>
        /// Determines whether [is read only type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is read only type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsReadOnlyType (this Type type)
        {
            if (type.IsInterface)
                return false;

            var defaultConstructor = type.GetConstructor(new Type[] { });
            return (defaultConstructor == null);
        }

        #endregion
    }
}
