using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Removes one or more SelectExpression's by rewriting the expression tree to not include them, promoting
    /// their from clause expressions and rewriting any column expressions that may have referenced them to now
    /// reference the underlying data directly.
    /// </summary>
    public class SubqueryRemover : DbExpressionVisitor
    {
        readonly HashSet<SelectExpression> selectsToRemove;
        //private Dictionary<Alias, Dictionary<string, ColumnDeclaration>> map;
        private readonly Dictionary<Alias, ReadOnlyCollection<ColumnDeclaration>> list;
        private Cache<Type, ProjectionClass> dynamicCache;
        private readonly Dictionary<Alias, SelectExpression> replaceWith = new Dictionary<Alias, SelectExpression>();

        private SubqueryRemover(IEnumerable<SelectExpression> selects, Cache<Type, ProjectionClass> dynamicCache)
        {
#if TRACE
            Console.WriteLine("\nSubqueryRemover:");
#endif
            this.dynamicCache = dynamicCache;
            selectsToRemove = new HashSet<SelectExpression>(selects);
            list = selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns);
            replaceWith = selectsToRemove.ToDictionary(d => d.Alias, d => d);

            //map = selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Alias.Name, d2 => d2));
        }

        public static SelectExpression Remove(SelectExpression outerSelect, Cache<Type, ProjectionClass> dynamicCache, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove, dynamicCache);
        }

        public static SelectExpression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove, Cache<Type, ProjectionClass> dynamicCache)
        {
            return (SelectExpression)new SubqueryRemover(selectsToRemove, dynamicCache).Visit(outerSelect);
        }

        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (selectsToRemove.Contains(select))
                return Visit(select.From);

            AliasedExpression from = select.From;
            Alias alias = from.Alias;

            ReadOnlyCollection<ColumnDeclaration> nameMap;
            if (list.TryGetValue(alias, out nameMap))
            {
                //var newColumns = ((IDbExpressionWithResult) select.From).Columns; 
                var newColumns = ((IDbExpressionWithResult) from).FromExpression.Columns;

                list.Remove(alias);
                list.Add(alias, newColumns);
            }

            return base.VisitSelectExpression(select);
        }

        /// <summary>
        /// Extracts all referenced columns.
        /// </summary>
        /// <param name="decl">The decl.</param>
        /// <returns></returns>
        IEnumerable<ColumnDeclaration> ExtractAllReferencedColumns(PropertyExpression property)
        {
            if (property == null || property.ReferringColumn == null)
                yield break;

            if (property.ReferringColumn.Expression is PropertyExpression)
                foreach (var column in ExtractAllReferencedColumns((PropertyExpression)property.ReferringColumn.Expression))
                    yield return column;

            yield return property.ReferringColumn;
        }

        IEnumerable<ColumnDeclaration> ExtractAllReferencedColumns(ColumnDeclaration column)
        {
            if (column == null)
                yield break;

            if (column.Expression is PropertyExpression)
                foreach (var i in ExtractAllReferencedColumns(((PropertyExpression)column.Expression).ReferringColumn))
                    yield return i;

            yield return column;
        }

        protected override Expression VisitColumn(PropertyExpression property)
        {
            ReadOnlyCollection<ColumnDeclaration> nameMap;
            if (list.TryGetValue(property.Alias, out nameMap))
            {
                List<ColumnDeclaration> columns = ExtractAllReferencedColumns(property).ToList();

                var result = nameMap.Where(x => ExtractAllReferencedColumns(x).Intersect(columns).Count()>0).FirstOrDefault();
                //var result = nameMap.Intersect(columns).FirstOrDefault();
                if (result == null)
//                    return property;
                     throw new Exception("This is really weired. Wait until I'm back from holiday.");

                AliasedExpression from = null;
                while (from == null)
                {
                    from = replaceWith[property.Alias].From;
                    from = FromExpressionFinder.Find(from, result.Expression);

                    if (from == null)
                    {
                        PropertyExpression ex = result.Expression as PropertyExpression;
                        if (ex == null)
                            break;
                        result = ex.ReferringColumn;

                        //from = replaceWith[property.Alias].From;
                        //if (ex != null)
                        //    from = FromExpressionFinder.Find(from, ex.ReferringColumn.Expression);
                    }

                    //break;
                }

                if (from == null)
                    throw new Exception("This is really weired. Wait until I'm back from holiday.");
                PropertyExpression aliased = result.Expression as PropertyExpression;
                return aliased != null 
                    ? new PropertyExpression(from, aliased).SetType(property.Type) 
                    : new PropertyExpression(from, new ColumnDeclaration(result.Expression, result));
            }

            return property;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = Visit(m.Expression);

            var prop = exp as PropertyExpression;
            if (prop != null)
                if (prop.Type == m.Expression.Type)    // Only make a member access, if both types equals
                    return Expression.MakeMemberAccess(new PropertyExpression(prop), m.Member);
                else
                    return prop;

            if (exp is SqlParameterExpression || exp is TableExpression || exp is ValueExpression || exp is SelectExpression)
                return exp;

            return UpdateMemberAccess(m, exp, m.Member);
        }

    }
}