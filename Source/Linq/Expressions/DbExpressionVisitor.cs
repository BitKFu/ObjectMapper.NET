using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// Defines method in order to execute the new DbExpressions
    /// </summary>
    public class DbExpressionVisitor : ExpressionVisitor
    {
        /// <summary> Gets or sets the groupings. </summary>
        protected List<PropertyTupel> Groupings { get { return groupings; } }
        private readonly List<PropertyTupel> groupings = new List<PropertyTupel>();

        // FIX!!!! MUST BE CHANGED; BECAUSE IT'S UNSAFE!!!!
        private static Dictionary<ColumnDeclaration,ColumnDeclaration> exchangeCd = new Dictionary<ColumnDeclaration, ColumnDeclaration>();

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (exp == null) return null;

#if TRACE
            if ((DbExpressionType)exp.NodeType > DbExpressionType.DbExpression)
            {
                Console.WriteLine(new string(' ', Deep*2) + (DbExpressionType) exp.NodeType+ ": " +exp);
                Deep++;
            }
#endif

            try
            {
                switch ((DbExpressionType) exp.NodeType)
                {
                    case DbExpressionType.PropertyExpression:
                        return VisitColumn((PropertyExpression) exp);
                    case DbExpressionType.ValueExpression:
                        return VisitValueExpression((ValueExpression) exp);
                    case DbExpressionType.TableExpression:
                        return VisitTableExpression((TableExpression) exp);
                    case DbExpressionType.SelectExpression:
                        return VisitSelectExpression((SelectExpression) exp);
                    case DbExpressionType.SqlParameterExpression:
                        return VisitSqlParameterExpression((SqlParameterExpression) exp);
                    case DbExpressionType.Join:
                        return VisitJoinExpression((JoinExpression) exp);
                    case DbExpressionType.Aggregate:
                        return VisitAggregateExpression((AggregateExpression) exp);
                    case DbExpressionType.RowCount:
                        return VisitRowNumberExpression((RowNumberExpression) exp);
                    case DbExpressionType.Between:
                        return VisitBetweenExpression((BetweenExpression) exp);
                    case DbExpressionType.Union:
                        return VisitUnionExpression((UnionExpression) exp);
                    case DbExpressionType.RowNum:
                        return VisitRowNumExpression((RowNumExpression)exp);
                    case DbExpressionType.Exists:
                        return VisitExistsExpression((ExistsExpression) exp);
                    case DbExpressionType.Cast:
                        return VisitCastExpression((CastExpression) exp);
                    case DbExpressionType.SelectFunction:
                        return VisitSelectFunctionExpression((SelectFunctionExpression) exp);
                    case DbExpressionType.AggregateSubquery:
                        return VisitAggregateSubquery((AggregateSubqueryExpression)exp);
                    case DbExpressionType.Ordering:
                        return VisitOrderExpression((OrderExpression) exp);
                    case DbExpressionType.SysDate:
                        return VisitSysDateExpression((SysDateExpression) exp);
                    case DbExpressionType.SysTime:
                        return VisitSysTimeExpression((SysTimeExpression) exp);
                    case DbExpressionType.ScalarExpression:
                        return VisitScalarExpression((ScalarExpression) exp);

                    case DbExpressionType.LateBinding:
                        return VisitLateBindingExpression((LateBindingExpression) exp);
                    default:
                        return base.Visit(exp);
                }
            }
            finally
            {
#if TRACE
                if ((DbExpressionType)exp.NodeType > DbExpressionType.DbExpression)
                    Deep--;
#endif
            }
        }

        /// <summary>
        /// Visits the late binding expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitLateBindingExpression(LateBindingExpression expression)
        {
            var lateBinding = Visit(expression.Binding);
            var alias = lateBinding as AliasedExpression;
            if (alias != null)
                return alias;

            return UpdateLateBindingExpression(expression, lateBinding);
        }

        /// <summary>
        /// Updates the late binding expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="binding">The binding.</param>
        /// <returns></returns>
        protected Expression UpdateLateBindingExpression(LateBindingExpression expression, Expression binding)
        {
            if (expression.Binding != binding)
                return new LateBindingExpression(binding);
        
            return expression;
        }

        protected virtual Expression VisitScalarExpression(ScalarExpression expression)
        {
            var from = VisitSource(expression.From);
            var columns = VisitColumnDeclarations(expression.Columns);

            return UpdateScalarExpression(expression, columns.FirstOrDefault(), from);
        }

        protected Expression UpdateScalarExpression(ScalarExpression expression, ColumnDeclaration column, AliasedExpression from)
        {
            if (expression.Columns.FirstOrDefault() != column ||
                expression.From != from)
                return new ScalarExpression(expression.Type, expression.Alias, column, expression.Selector, from);

            return expression;
        }

        protected virtual Expression VisitSysTimeExpression(SysTimeExpression expression)
        {
            return expression;
        }

        protected virtual Expression VisitSysDateExpression(SysDateExpression expression)
        {
            return expression;
        }

        protected virtual Expression VisitOrderExpression(OrderExpression orderExpression)
        {
            var orderBy = Visit(orderExpression.Expression);
            return UpdateOrderExpression(orderExpression, orderBy);
        }

        protected Expression UpdateOrderExpression(OrderExpression orderExpression, Expression orderBy)
        {
            if (orderBy != orderExpression.Expression)
                return new OrderExpression(orderExpression.Ordering, orderBy);
            return orderExpression;
        }
            
        protected virtual Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            var subquery = (ScalarExpression)Visit(aggregate.AggregateAsSubquery);
            return UpdateAggregateSubquery(aggregate, subquery);
        }

        protected AggregateSubqueryExpression UpdateAggregateSubquery(AggregateSubqueryExpression aggregate, ScalarExpression subquery)
        {
            if (subquery != aggregate.AggregateAsSubquery)
                return new AggregateSubqueryExpression(aggregate.Alias, aggregate.AggregateInGroupSelect, subquery);
            
            return aggregate;
        }

        /// <summary>
        /// Visits the Select Function Expresion
        /// </summary>
        /// <param name="sfe"></param>
        /// <returns></returns>
        protected virtual Expression VisitSelectFunctionExpression(SelectFunctionExpression sfe)
        {
            return sfe;
        }

        /// <summary>
        /// Visits an cast expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitCastExpression(CastExpression expression)
        {
            var subExpression = Visit(expression.Expression);
            return UpdateCast(expression, subExpression);
        }

        protected Expression UpdateCast(CastExpression exists, Expression subExpression)
        {
            if (exists.Expression != subExpression)
                return new CastExpression(subExpression, exists.TargetType);

            return exists;
        }

        /// <summary>
        /// Visits the Exists expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitExistsExpression(ExistsExpression expression)
        {
            var subSelect = (SelectExpression) Visit(expression.Selection);
            return UpdateExists(expression, subSelect);
        }

        protected Expression UpdateExists(ExistsExpression exists, SelectExpression subSelect)
        {
            if (exists.Selection != subSelect)
                return new ExistsExpression(subSelect);

            return exists;
        }

        /// <summary>
        /// Visits the RowNum Expression
        /// </summary>
        protected virtual Expression VisitRowNumExpression(RowNumExpression rowNumExpression)
        {
            return rowNumExpression;
        }

        protected virtual Expression VisitUnionExpression(UnionExpression union)
        {
            var first = Visit(union.First);
            var second = Visit(union.Second);
            return UpdateUnion(union, first, second);
        }

        protected Expression UpdateUnion(UnionExpression union, Expression first, Expression second)
        {
            if (union.First != first || union.Second != second)
                return new UnionExpression(union.Type, union.Projection, first, second, union.Alias, union.Columns, union.UnionAll);
            return union;
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <returns></returns>
        protected virtual Expression VisitJoinExpression(JoinExpression join)
        {
            var left = VisitSource(join.Left);
            var right = VisitSource(join.Right);
            var condition = Visit(join.Condition);
            return UpdateJoin(join, join.Join, left, right, condition);
        }

        /// <summary>
        /// Updates the join.
        /// </summary>
        protected JoinExpression UpdateJoin(JoinExpression join, JoinType joinType, Expression left, Expression right, Expression condition)
        {
            if (joinType != join.Join || left != join.Left || right != join.Right || condition != join.Condition)
                return new JoinExpression(join.Type, join.Projection, joinType, left, right, condition);
            return join;
        }

        /// <summary>
        /// Visits the SQL parameter expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitSqlParameterExpression(SqlParameterExpression expression)
        {
            return expression;
        }

        /// <summary>
        /// Visits the source.
        /// </summary>
        protected virtual AliasedExpression VisitSource(Expression source)
        {
            return Visit(source) as AliasedExpression;
        }

        /// <summary> Visits the column declarations. </summary>
        protected virtual ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                var column = columns[i];
                var e = Visit(column.Expression);
                if (e == null)
                    continue;

                if (alternate == null && e != column.Expression)
                    alternate = columns.Take(i).ToList();

                if (alternate != null)
                {
                    var newCd = new ColumnDeclaration(e, column);
                    if (exchangeCd.ContainsKey(column))
                        exchangeCd.Remove(column);
                    exchangeCd.Add(column, newCd);
                    alternate.Add(newCd);
                }
            }
            
            return alternate != null ? alternate.AsReadOnly() : columns;
        }

        protected virtual ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions == null) return expressions;

            List<OrderExpression> alternate = null;
            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                OrderExpression expr = expressions[i];
                OrderExpression e = Visit(expr) as OrderExpression;

                if (alternate == null && e != expr)
                    alternate = expressions.Take(i).ToList();

                if (alternate != null)
                    alternate.Add(e);
            }

            return alternate != null ? alternate.AsReadOnly() : expressions;
        }

        /// <summary> Visits the select expression. </summary>
        protected virtual Expression VisitSelectExpression(SelectExpression select)
        {
            var from = VisitSource(select.From);
            var where = Visit(select.Where);
            var orderBy = VisitOrderBy(select.OrderBy);
            var groupBy = VisitExpressionList(select.GroupBy);
            var skip = Visit(select.Skip);
            var take = Visit(select.Take);
            var columns = VisitColumnDeclarations(select.Columns);

            // If the selector equals the from, than take this one
            Expression selector;
            if (DbExpressionComparer.AreEqual(select.From, select.Selector))
                selector = from;
            else
                selector = Visit(select.Selector);


            var defaultIfEmpty = Visit(select.DefaultIfEmpty);

            var projection = selector != null
                // If we have a new selector, than create the projection based on that selector
                                 ? new ProjectionClass(selector)
                // If we have no selector, than visit the current projection and see, if something has to be amendet.   
                                 : VisitProjection(select.Projection);  

            var result = UpdateSelect(select, projection, selector, from, where, orderBy, groupBy, skip, take, select.IsDistinct, select.IsReverse, columns, select.SqlId, select.Hint, defaultIfEmpty);

#if DEBUG
            // Check, if ater the SelectExpression the columns are valid
            if (!(this is ReferingColumnChecker))
                ReferingColumnChecker.Validate(result); 
#endif
            return result;
        }

        /// <summary>
        /// Visits the projection and update the complex column mappings
        /// </summary>
        /// <param name="projection"></param>
        /// <returns></returns>
        private ProjectionClass VisitProjection(ProjectionClass projection)
        {
            if (projection == null)
                return null;

            if (projection.ComplexTypeColumnMapping != null)
                for (int x = 0; x < projection.ComplexTypeColumnMapping.Length; x++)
                    projection.ComplexTypeColumnMapping[x] = projection.ComplexTypeColumnMapping[x] != null
                        ? VisitColumnDeclarations(projection.ComplexTypeColumnMapping[x])
                        : null;

            return projection;
        }

        protected SelectExpression UpdateSelect(SelectExpression select, ProjectionClass projection, Expression selector, 
            AliasedExpression from, Expression where, ReadOnlyCollection<OrderExpression> orderBy, 
            ReadOnlyCollection<Expression> groupBy, Expression skip, Expression take, bool isDistinct, 
            bool isReverse, ReadOnlyCollection<ColumnDeclaration> columns, string sqlId, string hint, Expression defaultIfEmpty)
        {
            return from != select.From || projection != select.Projection || where != select.Where || orderBy != select.OrderBy || selector != select.Selector
                   || groupBy != select.GroupBy || take != select.Take || skip != select.Skip
                   || isDistinct != select.IsDistinct || columns != select.Columns || isReverse != select.IsReverse
                   || sqlId != select.SqlId || hint != select.Hint || defaultIfEmpty != select.DefaultIfEmpty
                       
                       ? new SelectExpression(
                             select.Type, projection, select.Alias, columns, selector, from, where, orderBy,
                             groupBy, skip, take, isDistinct, isReverse, select.SelectResult, sqlId, hint, defaultIfEmpty)

                       : select;
        }

        /// <summary> Visits the table expression. </summary>
        protected virtual Expression VisitTableExpression(TableExpression expression)
        {
            return expression;
        }

        /// <summary> Visits the value expression. </summary>
        protected virtual Expression VisitValueExpression(ValueExpression expression)
        {
            return expression;
        }

        /// <summary> Visits the column expression </summary>
        protected virtual Expression VisitColumn(PropertyExpression expression)
        {
            // Maybe we must exchange a referring column
            if (expression.ReferringColumn != null)
            {
                ColumnDeclaration exchangedCd;
                if (exchangeCd.TryGetValue(expression.ReferringColumn, out exchangedCd))
                    expression.ReferringColumn = exchangedCd;
            }

            //// Perhaps we have a referring column that needs to be vistited
            //if (expression.ReferringColumn != null)
            //    expression.ReferringColumn.Expression = Visit(expression.ReferringColumn.Expression);

            return expression;
        }

        /// <summary>
        /// Resolves the grouping.
        /// </summary>
        protected PropertyInfo ResolveGrouping(PropertyInfo propertyInfo)
        {
            // Solve grouping 
            if (propertyInfo.ReflectedType.IsGenericType)
            {
                Type genericType = propertyInfo.ReflectedType;

                // Simple Single Groupings ?? Complex Multiple Grouping
                PropertyInfo info = propertyInfo;
                var foundTupel = Groupings.Find(tupel => tupel.Target == genericType && tupel.Source.Name == info.Name) ??
                                 Groupings.Find(tupel => tupel.CoveredType == genericType && tupel.Source.Name == info.Name);

                if (foundTupel != null)
                    propertyInfo = foundTupel.Source;
            }
            return propertyInfo;
        }

        /// <summary>
        /// Gets the retriever.
        /// </summary>
        protected IRetriever GetRetriever(MemberExpression expr)
        {
            IRetriever tupel = null;
            var member = expr.Member as PropertyInfo;
            if (member != null)
            {
                member = ResolveGrouping(member);
                if (expr.Expression != null)
                    tupel = new PropertyTupel(expr.Expression.Type, member);
            }

            var field = expr.Member as FieldInfo;
            if (field != null && expr.Expression != null)
            {
                tupel = new FieldTupel(expr.Expression.Type, field);
            }
            return tupel;
        }

        /// <summary>
        /// Visits the aggregate expression
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected virtual Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            var arg = Visit(aggregate.Argument);
            return UpdateAggregate(aggregate, aggregate.Type, aggregate.AggregateName, arg, aggregate.IsDistinct);
        }

        /// <summary>
        /// Updates the aggregate expression after visiting it
        /// </summary>
        /// <param name="aggregate"></param>
        /// <param name="type"></param>
        /// <param name="aggType"></param>
        /// <param name="arg"></param>
        /// <param name="isDistinct"></param>
        /// <returns></returns>
        protected AggregateExpression UpdateAggregate(AggregateExpression aggregate, Type type, string aggType, Expression arg, bool isDistinct)
        {
            if (type != aggregate.Type || aggType != aggregate.AggregateName || arg != aggregate.Argument || isDistinct != aggregate.IsDistinct)
            {
                return new AggregateExpression(type, aggType, arg, isDistinct);
            }
            return aggregate;
        }

        /// <summary>
        /// Visits the Betwen expression
        /// </summary>
        /// <param name="between"></param>
        /// <returns></returns>
        protected virtual Expression VisitBetweenExpression(BetweenExpression between)
        {
            var expr = Visit(between.Expression);
            var lower = Visit(between.Lower);
            var upper = Visit(between.Upper);
            return UpdateBetween(between, expr, lower, upper);
        }

        /// <summary>
        /// Updates the between expression after visiting it.
        /// </summary>
        /// <param name="between"></param>
        /// <param name="expression"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        protected BetweenExpression UpdateBetween(BetweenExpression between, Expression expression, Expression lower, Expression upper)
        {
            if (expression != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                return new BetweenExpression(expression, lower, upper);
            }
            return between;
        }

        /// <summary>
        /// Visit the rownumber expression
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        protected virtual Expression VisitRowNumberExpression(RowNumberExpression rowNumber)
        {
            var orderby = VisitOrderBy(rowNumber.OrderBy);
            return UpdateRowNumber(rowNumber, orderby);
        }

        /// <summary>
        /// Updates the rownumber expression after visiting it.
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        protected RowNumberExpression UpdateRowNumber(RowNumberExpression rowNumber, IList<OrderExpression> orderBy)
        {
            return orderBy != rowNumber.OrderBy ? new RowNumberExpression(orderBy) : rowNumber;
        }

        /// <summary>
        /// Finds the source column within the current Selection, based on a nested property
        /// </summary>
        protected static ColumnDeclaration FindSourceColumn(AliasedExpression from, PropertyExpression property)
        {
            property = OriginPropertyFinder.Find(property) ?? property;
            var columns = ColumnProjector.Evaluate(from, from.Projection);
            return columns.Where(x => property.Equals(x.OriginalProperty)).FirstOrDefault();
        }

        protected static ColumnDeclaration FindSourceColumn(AliasedExpression from, ColumnDeclaration declaration)
        {
            AliasedExpression aliased = declaration.Expression as AliasedExpression;
            if (from == null || (aliased != null && aliased.Alias == from.Alias))
                return declaration;

            PropertyExpression pe = declaration.Expression as PropertyExpression;
            if (pe == null)
                return declaration;

            var column = FindSourceColumn(from, pe);
            if (column == null)
                return column;

            return column.SetAlias(declaration.Alias);
        }

        protected static ColumnDeclaration FindSourceColumn(AliasedExpression currentFrom, Expression property)
        {
            SelectExpression select = currentFrom as SelectExpression;
            PropertyExpression propertyExpression = property as PropertyExpression;

            // If It's a new Expression
            NewExpression nex = select != null ? select.Selector as NewExpression : null;
            if (nex != null && propertyExpression != null && nex.Members != null)
                for (int i = 0, n = nex.Members.Count; i < n; i++)
                    if (MembersMatch(nex.Members[i], propertyExpression.PropertyName))
                        return new ColumnDeclaration(nex.Arguments[i], Alias.Generate(propertyExpression.Name), propertyExpression.PropertyName);

            var columns = ((IDbExpressionWithResult)currentFrom).Columns;

            Expression expression1 = property;
            var found = columns.Where(x =>
                x.Expression is PropertyExpression ?
                expression1.Equals(x.Expression) : DbExpressionComparer.AreEqual(expression1, x.Expression)
                ).FirstOrDefault();

            if (found != null)
                return found;

            Expression expression = OriginPropertyFinder.Find(property);
            found = columns.Where(x =>
                x.Expression is PropertyExpression ?
                expression.Equals(x.OriginalProperty) : DbExpressionComparer.AreEqual(property, x.Expression)
                ).FirstOrDefault();

            return found;
        }

        private static bool MembersMatch(MemberInfo a, string propertyName)
        {
            return a.Name.Substring(4) == propertyName;
        }
    }
}