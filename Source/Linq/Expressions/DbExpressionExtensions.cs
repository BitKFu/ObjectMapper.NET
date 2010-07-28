using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    public static class DbExpressionExtensions
    {
        public static SelectExpression SetColumns(this SelectExpression select, IList<ColumnDeclaration> columns)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias,
                                        new ReadOnlyCollection<ColumnDeclaration>(columns), select.Selector,
                                        select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip,
                                        select.Take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        public static SelectExpression AddColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns);
            columns.Add(column);
            return select.SetColumns(columns);
        }

        public static SelectExpression RemoveColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns);
            columns.Remove(column);
            return select.SetColumns(columns);
        }

        public static ColumnDeclaration SetAlias(this ColumnDeclaration decl, Alias alias)
        {
            if (decl.Alias == alias)
                return decl;

            return new ColumnDeclaration(decl.Expression, alias, decl.PropertyName);
        }

        public static AliasedExpression SetAlias(this AliasedExpression exp, Alias alias)
        {
            if (exp.Alias == alias)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(exp, alias);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(exp.Type, exp.Projection, alias, prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(exp.Type, select.Projection, alias, select.Columns, select.Selector,
                                            select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            return exp;
        }

        public static AliasedExpression SetAlias(this AliasedExpression exp, string alias)
        {
            if (exp.Alias.Name == alias)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(exp, Alias.Generate(alias));

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(exp.Type, exp.Projection, Alias.Generate(alias), prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(exp.Type, select.Projection, Alias.Generate(alias), select.Columns, select.Selector,
                                            select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            return exp;
        }

        public static SelectExpression SetDefaultIfEmpty(this SelectExpression exp, Expression defaultIfEmpty)
        {
            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, defaultIfEmpty);

            return exp;
        }

        public static AliasedExpression SetType(this AliasedExpression exp, Type type)
        {
            if (exp.Type == type)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(type, exp.Projection, exp.Alias);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(type, prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            return exp;
        }

        public static AliasedExpression SetProjection(this AliasedExpression exp, ProjectionClass projection)
        {
            if (exp.Projection == projection)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(exp.Type, projection, exp.Alias);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(exp.Type, projection, prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(select.Type, projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            return exp;
        }

        public static SelectExpression SetDistinct(this SelectExpression select, bool isDistinct)
        {
            if (select.IsDistinct != isDistinct)
            {
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            isDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        public static SelectExpression SetReverse(this SelectExpression select, bool isReverse)
        {
            if (select.IsReverse != isReverse)
            {
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, isReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        public static SelectExpression SetWhere(this SelectExpression select, Expression where)
        {
            if (where != select.Where)
            {
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        public static SelectExpression SetOrderBy(this SelectExpression select, IList<OrderExpression> orderBy)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, new ReadOnlyCollection<OrderExpression>(orderBy), select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        public static SelectExpression SetResultType(this SelectExpression select, SelectResultType resultType)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        resultType, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        public static SelectExpression SetSqlId(this SelectExpression select, string sqlId)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, sqlId, select.Hint, select.DefaultIfEmpty);
        }

        public static SelectExpression SetHint(this SelectExpression select, string hint)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, hint, select.DefaultIfEmpty);
        }

        //public static SelectExpression AddOrderExpression(this SelectExpression select, OrderExpression ordering)
        //{
        //    var orderby = new List<OrderExpression>();
        //    if (select.OrderBy != null)
        //        orderby.AddRange(select.OrderBy);
        //    orderby.Add(ordering);
        //    return select.SetOrderBy(orderby);
        //}

        //public static SelectExpression RemoveOrderExpression(this SelectExpression select, OrderExpression ordering)
        //{
        //    if (select.OrderBy != null && select.OrderBy.Count > 0)
        //    {
        //        var orderby = new List<OrderExpression>(select.OrderBy);
        //        orderby.Remove(ordering);
        //        return select.SetOrderBy(orderby);
        //    }
        //    return select;
        //}

        public static SelectExpression SetGroupBy(this SelectExpression select, IList<Expression> groupBy)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, new ReadOnlyCollection<Expression>(groupBy),
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        public static SelectExpression SetSelector(this SelectExpression select, Expression selector)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        //public static SelectExpression AddGroupExpression(this SelectExpression select, Expression expression)
        //{
        //    var groupby = new List<Expression>();
        //    if (select.GroupBy != null)
        //        groupby.AddRange(select.GroupBy);
        //    groupby.Add(expression);
        //    return select.SetGroupBy(groupby);
        //}

        //public static SelectExpression RemoveGroupExpression(this SelectExpression select, Expression expression)
        //{
        //    if (select.GroupBy != null && select.GroupBy.Count > 0)
        //    {
        //        var groupby = new List<Expression>(select.GroupBy);
        //        groupby.Remove(expression);
        //        return select.SetGroupBy(groupby);
        //    }
        //    return select;
        //}

        public static SelectExpression SetSkip(this SelectExpression select, Expression skip)
        {
            if (skip != select.Skip)
            {
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        public static SelectExpression SetTake(this SelectExpression select, Expression take)
        {
            if (take != select.Take)
            {
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            return select;
        }

        public static SelectExpression AddRedundantSelect(this SelectExpression select, Alias newAlias)
        {
            IEnumerable<ColumnDeclaration> newColumns =
                select.Columns.Select(
                    d =>
                    new ColumnDeclaration(new PropertyExpression(select.Projection, newAlias, d),
                                          Alias.Generate(AliasType.Column), d.PropertyName));

            var newFrom = new SelectExpression(select.Type, select.Projection, newAlias, select.Columns, select.Selector, select.From,
                                               select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                               select.IsDistinct, select.IsReverse, select.SelectResult, null, null, select.DefaultIfEmpty);
            return new SelectExpression(select.Type, select.Projection, select.Alias,
                                        new ReadOnlyCollection<ColumnDeclaration>(new List<ColumnDeclaration>(newColumns)),
                                        null, newFrom, null, null, null, null, null, false, false, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        //public static SelectExpression RemoveRedundantFrom(this SelectExpression select)
        //{
        //    var fromSelect = select.From as SelectExpression;
        //    if (fromSelect != null)
        //    {
        //        return SubqueryRemover.Remove(select, fromSelect);
        //    }
        //    return select;
        //}

        //public static SelectExpression SetFrom(this SelectExpression select, Expression from)
        //{
        //    if (select.From != from)
        //    {
        //        return new SelectExpression(select.Type, select.Alias, select.Columns, select.Selector, from,
        //                                    select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
        //                                    select.IsDistinct, select.IsReverse, select.SelectResult);
        //    }
        //    return select;
        //}
    }
}