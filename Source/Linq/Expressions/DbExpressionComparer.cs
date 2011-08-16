using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Linq.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// DbExpressionComparer
    /// </summary>
    public class DbExpressionComparer : ExpressionComparer
    {
        ScopedDictionary<Alias, Alias> aliasScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbExpressionComparer"/> class.
        /// </summary>
        /// <param name="parameterScope">The parameter scope.</param>
        /// <param name="aliasScope">The alias scope.</param>
        /// <param name="exactMatch">if set to <c>true</c> [exact match].</param>
        protected DbExpressionComparer(ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope, ScopedDictionary<Alias, Alias> aliasScope, bool exactMatch)
            : base(parameterScope, exactMatch)
        {
            this.aliasScope = aliasScope;
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public new static bool AreEqual(Expression a, Expression b)
        {
            return AreEqual(null, null, a, b, true);
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="exactMatch">if set to <c>true</c> [exact match].</param>
        /// <returns></returns>
        public new static bool AreEqual(Expression a, Expression b, bool exactMatch)
        {
            return AreEqual(null, null, a, b, exactMatch);
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="parameterScope">The parameter scope.</param>
        /// <param name="aliasScope">The alias scope.</param>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static bool AreEqual(ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope, ScopedDictionary<Alias, Alias> aliasScope, Expression a, Expression b)
        {
            return new DbExpressionComparer(parameterScope, aliasScope, true).Compare(a, b);
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="parameterScope">The parameter scope.</param>
        /// <param name="aliasScope">The alias scope.</param>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="exactMatch">if set to <c>true</c> [exact match].</param>
        /// <returns></returns>
        public static bool AreEqual(ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope, ScopedDictionary<Alias, Alias> aliasScope, Expression a, Expression b, bool exactMatch)
        {
            return new DbExpressionComparer(parameterScope, aliasScope, exactMatch).Compare(a, b);
        }

        //protected override bool CompareMemberAccess(MemberExpression a, MemberExpression b)
        //{
        //    var propertyA = OriginPropertyFinder.Find(a);
        //    var propertyB = OriginPropertyFinder.Find(b);

        //    if (propertyA != null && propertyB != null)
        //        return Compare(propertyA, propertyB);

        //    return base.CompareMemberAccess(a, b);
        //}

        /// <summary>
        /// Compares the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected override bool Compare(Expression a, Expression b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.NodeType != b.NodeType)
                return false;

            if (ExactMatch && a.Type != b.Type)
                return false;

            switch ((DbExpressionType)a.NodeType)
            {
                case DbExpressionType.PropertyExpression:
                    return CompareProperty((PropertyExpression)a, (PropertyExpression)b);

                case DbExpressionType.ValueExpression:
                    return CompareValue((ValueExpression) a, (ValueExpression) b);

                case DbExpressionType.TableExpression:
                    return CompareTable((TableExpression) a, (TableExpression) b);

                case DbExpressionType.SelectExpression:
                    return CompareSelect((SelectExpression) a, (SelectExpression) b);

                case DbExpressionType.SqlParameterExpression:
                    return CompareSqlParameter((SqlParameterExpression) a, (SqlParameterExpression) b);

                case DbExpressionType.Join:
                    return CompareJoin((JoinExpression) a, (JoinExpression) b);

                case DbExpressionType.Aggregate:
                    return CompareAggregate((AggregateExpression) a, (AggregateExpression) b);

                case DbExpressionType.RowCount:
                    return CompareRowCount((RowNumberExpression) a, (RowNumberExpression) b);

                case DbExpressionType.Between:
                    return CompareBetween((BetweenExpression) a, (BetweenExpression) b);

                case DbExpressionType.Union:
                    return CompareUnion((UnionExpression) a, (UnionExpression) b);

                case DbExpressionType.RowNum:
                    return CompareRowNum((RowNumExpression) a, (RowNumExpression) b);

                case DbExpressionType.Exists:
                    return CompareExists((ExistsExpression) a, (ExistsExpression) b);
                
                case DbExpressionType.Cast:
                    return CompareCast((CastExpression) a, (CastExpression) b);

                case DbExpressionType.SelectFunction:
                    return CompareSelectFunction((SelectFunctionExpression) a, (SelectFunctionExpression) b);

                case DbExpressionType.AggregateSubquery:
                    return CompareAggregateSubQuery((AggregateSubqueryExpression) a, (AggregateSubqueryExpression) b);

                case DbExpressionType.Ordering:
                    return CompareOrdering((OrderExpression) a, (OrderExpression) b);

                default:
                    return base.Compare(a, b);
            }
        }

        /// <summary>
        /// Compares the ordering.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareOrdering(OrderExpression a, OrderExpression b)
        {
            return a.Ordering == b.Ordering && Compare(a.Expression, b.Expression);
        }

        /// <summary>
        /// Compares the aggregate sub query.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareAggregateSubQuery(AggregateSubqueryExpression a, AggregateSubqueryExpression b)
        {
            return this.Compare(a.AggregateAsSubquery, b.AggregateAsSubquery)
                && this.Compare(a.AggregateInGroupSelect, b.AggregateInGroupSelect)
                && a.Alias == b.Alias;
        }

        /// <summary>
        /// Compares the select function.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareSelectFunction(SelectFunctionExpression a, SelectFunctionExpression b)
        {
            return a.Function == b.Function;
        }

        /// <summary>
        /// Compares the cast.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareCast(CastExpression a, CastExpression b)
        {
            return a.TargetType == b.TargetType && Compare(a.Expression, b.Expression);
        }

        /// <summary>
        /// Compares the exists.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareExists(ExistsExpression a, ExistsExpression b)
        {
            return Compare(a.Selection, b.Selection);
        }

        /// <summary>
        /// Compares the row num.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareRowNum(RowNumExpression a, RowNumExpression b)
        {
            return a.RevealedType == b.RevealedType;
        }

        /// <summary>
        /// Compares the union.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareUnion(UnionExpression a, UnionExpression b)
        {
            return (Compare(a.First, b.First) && Compare(a.Second, b.Second));
        }

        /// <summary>
        /// Compares the between.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareBetween(BetweenExpression a, BetweenExpression b)
        {
            return this.Compare(a.Expression, b.Expression)
                && this.Compare(a.Lower, b.Lower)
                && this.Compare(a.Upper, b.Upper);
        }

        /// <summary>
        /// Compares the row count.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareRowCount(RowNumberExpression a, RowNumberExpression b)
        {
            return this.CompareOrderList(a.OrderBy, b.OrderBy);
        }

        /// <summary>
        /// Compares the aggregate.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareAggregate(AggregateExpression a, AggregateExpression b)
        {
            return a.AggregateName == b.AggregateName && this.Compare(a.Argument, b.Argument);
        }

        /// <summary>
        /// Compares the join.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareJoin(JoinExpression a, JoinExpression b)
        {
            if (a.Join != b.Join || !this.Compare(a.Left, b.Left))
                return false;

            if (a.Join == JoinType.CrossApply || a.Join == JoinType.OuterApply)
            {
                var save = this.aliasScope;
                try
                {
                    this.aliasScope = new ScopedDictionary<Alias, Alias>(this.aliasScope);
                    this.MapAliases(a.Left, b.Left);

                    return this.Compare(a.Right, b.Right)
                        && this.Compare(a.Condition, b.Condition);
                }
                finally
                {
                    this.aliasScope = save;
                }
            }
            else
            {
                return this.Compare(a.Right, b.Right)
                    && this.Compare(a.Condition, b.Condition);
            }
        }

        /// <summary>
        /// Compares the SQL parameter.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareSqlParameter(SqlParameterExpression a, SqlParameterExpression b)
        {
            return a.ContentType == b.ContentType && a.Value == b.Value;
        }

        /// <summary>
        /// Compares the select.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareSelect(SelectExpression a, SelectExpression b)
        {
            var save = this.aliasScope;
            try
            {
                if (!this.Compare(a.From, b.From))
                    return false;

                this.aliasScope = new ScopedDictionary<Alias, Alias>(save);
                this.MapAliases(a.From, b.From);

                return this.Compare(a.Where, b.Where)
                    && this.CompareOrderList(a.OrderBy, b.OrderBy)
                    && this.CompareExpressionList(a.GroupBy, b.GroupBy)
                    && this.Compare(a.Skip, b.Skip)
                    && this.Compare(a.Take, b.Take)
                    && a.IsDistinct == b.IsDistinct
                    && a.IsReverse == b.IsReverse
                    && this.CompareColumnDeclarations(a.Columns, b.Columns);
            }
            finally
            {
                this.aliasScope = save;
            }
        }

        /// <summary>
        /// Compares the table.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareTable(TableExpression a, TableExpression b)
        {
            return a.RevealedType == b.RevealedType;
        }

        /// <summary>
        /// Compares the value.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareValue(ValueExpression a, ValueExpression b)
        {
            return a.Value == b.Value;
        }

        /// <summary>
        /// Compares the property.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private bool CompareProperty(PropertyExpression a, PropertyExpression b)
        {
            return this.CompareAlias(a.Alias, b.Alias) && a.Name == b.Name;
        }

        /// <summary>
        /// Compares the alias.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual bool CompareAlias(Alias a, Alias b)
        {
            if (this.aliasScope != null)
            {
                Alias mapped;
                if (this.aliasScope.TryGetValue(a, out mapped))
                    return mapped == b;
            }
            return a == b;
        }

        /// <summary>
        /// Maps the aliases.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        private void MapAliases(Expression a, Expression b)
        {
            Alias[] prodA = DeclaredAliasGatherer.Gather(a).ToArray();
            Alias[] prodB = DeclaredAliasGatherer.Gather(b).ToArray();
            for (int i = 0, n = prodA.Length; i < n; i++)
            {
                this.aliasScope.Add(prodA[i], prodB[i]);
            }
        }

        /// <summary>
        /// Compares the order list.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual bool CompareOrderList(ReadOnlyCollection<OrderExpression> a, ReadOnlyCollection<OrderExpression> b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (a[i].Ordering != b[i].Ordering ||
                    !this.Compare(a[i].Expression, b[i].Expression))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the column declarations.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual bool CompareColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> a, ReadOnlyCollection<ColumnDeclaration> b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (!this.CompareColumnDeclaration(a[i], b[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the column declaration.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual bool CompareColumnDeclaration(ColumnDeclaration a, ColumnDeclaration b)
        {
            return a.PropertyName == b.PropertyName && Compare(a.Expression, b.Expression);
        }
    }
}
