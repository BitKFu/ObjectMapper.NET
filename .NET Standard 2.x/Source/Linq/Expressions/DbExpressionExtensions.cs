using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// DbExpressionExtensions
    /// </summary>
    public static class DbExpressionExtensions
    {
        /// <summary>
        /// Sets the columns.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public static SelectExpression SetColumns(this SelectExpression select, IList<ColumnDeclaration> columns)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias,
                                        new ReadOnlyCollection<ColumnDeclaration>(columns), select.Selector,
                                        select.From, select.Where, select.OrderBy, select.GroupBy, select.Skip,
                                        select.Take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static SelectExpression AddColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns);
            columns.Add(column);
            return select.SetColumns(columns);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static SelectExpression RemoveColumn(this SelectExpression select, ColumnDeclaration column)
        {
            var columns = new List<ColumnDeclaration>(select.Columns);
            columns.Remove(column);
            return select.SetColumns(columns);
        }

        /// <summary>
        /// Sets the alias.
        /// </summary>
        /// <param name="decl">The decl.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public static ColumnDeclaration SetAlias(this ColumnDeclaration decl, Alias alias)
        {
            if (decl.Alias.Equals(alias))
                return decl;

            return new ColumnDeclaration(decl.Expression, alias, decl.PropertyName);
        }

        /// <summary>
        /// Sets the alias.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public static AliasedExpression SetAlias(this AliasedExpression exp, Alias alias)
        {
            if (exp.Alias == alias)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(table.Type, table.Projection, alias, table.Columns);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(exp.Type, exp.Projection, alias, prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(exp.Type, select.Projection, alias, select.Columns, select.Selector,
                                            select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            var union = exp as UnionExpression;
            if (union != null)
                return new UnionExpression(union.Type, union.Projection, union.First, union.Second, alias, union.Columns, union.UnionAll);

            var join = exp as JoinExpression;
            if (join != null)
                return new JoinExpression(join.Type, join.Projection, join.Join, join.Left, join.Right, join.Condition, alias.Name);

            var scalar = exp as ScalarExpression;
            if (scalar != null)
                return new ScalarExpression(scalar.Type, alias, scalar.Columns.FirstOrDefault(), scalar.Selector, scalar.From as AliasedExpression);

            return exp;
        }

        /// <summary>
        /// Sets the alias.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public static AliasedExpression SetAlias(this AliasedExpression exp, string alias)
        {
            if (exp.Alias.Name == alias)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(table.Type, table.Projection, Alias.Generate(alias), table.Columns);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(exp.Type, exp.Projection, Alias.Generate(alias), prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(exp.Type, select.Projection, Alias.Generate(alias), select.Columns, select.Selector,
                                            select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            var union = exp as UnionExpression;
            if (union != null)
                return new UnionExpression(union.Type, union.Projection, union.First, union.Second, Alias.Generate(alias), union.Columns, union.UnionAll);

            var join = exp as JoinExpression;
            if (join != null)
                return new JoinExpression(join.Type, join.Projection, join.Join, join.Left, join.Right, join.Condition, alias);

            var scalar = exp as ScalarExpression;
            if (scalar != null)
                return new ScalarExpression(scalar.Type, Alias.Generate(alias), scalar.Columns.FirstOrDefault(), scalar.Selector, scalar.From as AliasedExpression);

            return exp;
        }

        /// <summary>
        /// Sets the default if empty.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="defaultIfEmpty">The default if empty.</param>
        /// <returns></returns>
        public static SelectExpression SetDefaultIfEmpty(this SelectExpression exp, Expression defaultIfEmpty)
        {
            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, defaultIfEmpty);

            return exp;
        }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static AliasedExpression SetType(this AliasedExpression exp, Type type)
        {
            if (exp.Type == type)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(type, table.Projection, table.Alias, table.Columns);

            var prop = exp as PropertyExpression;
            if (prop != null)
                return new PropertyExpression(type, prop);

            var select = exp as SelectExpression;
            if (select != null)
                return new SelectExpression(type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                            select.Where, select.OrderBy, select.GroupBy, select.Skip, select.Take,
                                            select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);

            var union = exp as UnionExpression;
            if (union != null)
                return new UnionExpression(type, union.Projection, union.First, union.Second, union.Alias, union.Columns,union.UnionAll);

            var join = exp as JoinExpression;
            if (join != null)
                return new JoinExpression(type, join.Projection, join.Join, join.Left, join.Right, join.Condition, join.Alias.Name);

            var scalar = exp as ScalarExpression;
            if (scalar != null)
                return new ScalarExpression(type, scalar.Alias, scalar.Columns.FirstOrDefault(), scalar.Selector, scalar.From as AliasedExpression);

            return exp;
        }

        /// <summary>
        /// Sets the projection.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="projection">The projection.</param>
        /// <returns></returns>
        public static AliasedExpression SetProjection(this AliasedExpression exp, ProjectionClass projection)
        {
            if (exp.Projection == projection)
                return exp;

            var table = exp as TableExpression;
            if (table != null)
                return new TableExpression(table.Type, projection, table.Alias, table.Columns);

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

        /// <summary>
        /// Sets the distinct.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="isDistinct">if set to <c>true</c> [is distinct].</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the reverse.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="isReverse">if set to <c>true</c> [is reverse].</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the where.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="where">The where.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the order by.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="orderBy">The order by.</param>
        /// <returns></returns>
        public static SelectExpression SetOrderBy(this SelectExpression select, IList<OrderExpression> orderBy)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, new ReadOnlyCollection<OrderExpression>(orderBy), select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the type of the result.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        public static SelectExpression SetResultType(this SelectExpression select, SelectResultType resultType)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        resultType, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the SQL id.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="sqlId">The SQL id.</param>
        /// <returns></returns>
        public static SelectExpression SetSqlId(this SelectExpression select, string sqlId)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, sqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the hint.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="hint">The hint.</param>
        /// <returns></returns>
        public static SelectExpression SetHint(this SelectExpression select, string hint)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the group by.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="groupBy">The group by.</param>
        /// <returns></returns>
        public static SelectExpression SetGroupBy(this SelectExpression select, IList<Expression> groupBy)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, select.Selector, select.From,
                                        select.Where, select.OrderBy, new ReadOnlyCollection<Expression>(groupBy),
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the selector.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public static SelectExpression SetSelector(this SelectExpression select, Expression selector)
        {
            return new SelectExpression(select.Type, select.Projection, select.Alias, select.Columns, selector, select.From,
                                        select.Where, select.OrderBy, select.GroupBy,
                                        select.Skip, select.Take, select.IsDistinct, select.IsReverse,
                                        select.SelectResult, select.SqlId, select.Hint, select.DefaultIfEmpty);
        }

        /// <summary>
        /// Sets the skip.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="skip">The skip.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the take.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the redundant select.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="newAlias">The new alias.</param>
        /// <returns></returns>
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

    }
}