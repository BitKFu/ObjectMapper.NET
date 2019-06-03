using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// Tries to bind the members
    /// </summary>
    public class MemberBinder : DbPackedExpressionVisitor
    {

        private AliasedExpression currentFrom;


        private readonly Dictionary<ParameterExpression, Expression> visitedParameterMappings = new Dictionary<ParameterExpression, Expression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberBinder"/> class.
        /// </summary>
        private MemberBinder(ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
#if TRACE
            Console.WriteLine("\nMemberBinder:");
#endif
        }

        /// <summary>
        /// Evaluates the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="backpack">The backpack.</param>
        /// <returns></returns>
        public static Expression Evaluate(Expression exp, ExpressionVisitorBackpack backpack)
        {
            MemberBinder binder = new MemberBinder(backpack);
            return binder.Visit(exp);
        }

        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private ProjectionClass GetProjection(Type type)
        {
            return ReflectionHelper.GetProjection(type.RevealType(), Backpack.ProjectionCache);
        }

        /// <summary>
        /// Memberses the match.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static bool MembersMatch(MemberInfo a, string propertyName)
        {
            return a.Name == propertyName;
        }

        private Expression previousExpression;
        private Stack<Expression> callStack = new Stack<Expression>();

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            previousExpression = callStack.Count>0 ?callStack.Peek() : null;
            callStack.Push(exp);
            try
            {
                return base.Visit(exp);
            }
            finally
            {
                callStack.Pop();
                previousExpression = callStack.Count > 0 ? callStack.Peek() : null;
            }

        }

        /// <summary>
        /// Visits the order by.
        /// </summary>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        protected override ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions == null)
                return null;

            var newOrders = new List<OrderExpression>();

            for (int index = 0; index < expressions.Count; index++)
            {
                OrderExpression expr = expressions[index];
                var e = Visit(expr) as OrderExpression;

                // Maybe we have to sort an result set
                var resultSet = e != null ? e.Expression as IDbExpressionWithResult : null;
                if (resultSet != null)
                {
                    newOrders.AddRange(resultSet.Columns.Select(cd => new OrderExpression(expr.Ordering, FindSourceColumn(currentFrom, cd).Expression)));
                    continue;
                }

                newOrders.Add(e);
            }

            return new ReadOnlyCollection<OrderExpression>(newOrders);
        }

        /// <summary>
        /// Visits the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            // Solve DateTime.Now, DateTime.Today
            if (m.Expression == null && m.Type == typeof(DateTime) && m.Member.Name == "Today")
                m = Expression.MakeMemberAccess(new SysDateExpression(), typeof(SysDateExpression).GetProperty("Today"));

            if (m.Expression == null && m.Type == typeof(DateTime) && m.Member.Name == "Now")
                m = Expression.MakeMemberAccess(new SysTimeExpression(), typeof(SysTimeExpression).GetProperty("Now"));

            Expression exp = Visit(m.Expression);

            IRetriever member = GetRetriever(m);
            var columnName = Property.GetPropertyInstance((PropertyInfo)member.Source).MetaInfo.ColumnName;
            var propertyName = member.Source.Name;
            var linkTarget = m.Expression != null ? m.Expression.Type.RevealType() : null;

            NewExpression nex = exp as NewExpression;
            if (nex != null)
            {
                if (nex.Members != null)
                {
                    for (int i = 0, n = nex.Members.Count; i < n; i++)
                        if (MembersMatch(nex.Members[i], propertyName))
                        {
                            exp = nex.Arguments[i];
                            if (exp is AliasedExpression)
                                return MapPropertyToCurrentFromClause(m, currentFrom, exp, m.Type);

                            var result = new PropertyExpression(currentFrom, new ColumnDeclaration(exp, Alias.Generate(columnName), propertyName));
                            return MapPropertyToCurrentFromClause(m, currentFrom, result, m.Type);
                        }
                }

                if (nex.Type.IsGenericType && nex.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                {
                    if (propertyName == "Key")
                    {
                        exp = nex.Arguments[0];
                        if (exp is AliasedExpression)
                            return MapPropertyToCurrentFromClause(m, currentFrom, exp, m.Type);

                        var result = new PropertyExpression(currentFrom, new ColumnDeclaration(exp, Alias.Generate(columnName), propertyName));
                        return MapPropertyToCurrentFromClause(m, currentFrom, result, m.Type);
                    }
                }
            }

            PropertyExpression property = exp as PropertyExpression;
            if (property != null && linkTarget.IsValueObjectType())
            {
                // now, find the property expression in the current From Expression
                var sourceColumn = FindSourceColumn(currentFrom, property);
                exp = CreateInnerJoin(linkTarget, sourceColumn);
            }

            bool again = false;
            do
            {
                if (exp.Type == m.Type)
                    return MapPropertyToCurrentFromClause(m, currentFrom, exp, m.Type);

                TableExpression table = exp as TableExpression;
                if (table != null)
                {
                    // First check, if the property is matching
                    var column =
                        table.Columns.Where(x => x.OriginalProperty != null && x.OriginalProperty.PropertyName == propertyName).
                            FirstOrDefault();
                    if (column != null)
                        return MapPropertyToCurrentFromClause(m, currentFrom, column.Expression, m.Type);

                    // Second check, if the alias is matching
                    column = table.Columns.Where(x => x.Alias.Name == columnName).FirstOrDefault();
                    if (column != null)
                        return MapPropertyToCurrentFromClause(m, currentFrom, column.Expression, m.Type);
                }

                IDbExpressionWithResult select = exp as IDbExpressionWithResult;
                if (select != null)
                {
                    // Ok. First check, if we have to create a join or something that way
                    var targetProjection = GetProjection(m.Expression.Type);
                    if (targetProjection.NewExpression != null)
                    {
                        var subParameter = targetProjection.NewExpression.Arguments
                                           .Where(arg =>
                                               {
                                                   var pe = arg as ParameterExpression;
                                                   if (pe != null) return pe.Name == propertyName;

                                                   var me = arg as MemberExpression;
                                                   if (me != null) return me.Member.Name == propertyName;

                                                   return false;
                                               }).FirstOrDefault();

                        if (subParameter != null)
                        {
                            var from = Visit(subParameter); //fromClauseMapping[subParameter].Expression;
                            from = from is SelectExpression
                                       ? ((SelectExpression)from).SetDefaultIfEmpty(select.DefaultIfEmpty)
                                       : from;
                            return MapPropertyToCurrentFromClause(m, currentFrom, from, m.Type);
                        }
                    }

                    // First check, if the property is matching
                    var column =
                        select.Columns.Where(
                            x =>
                            x.OriginalProperty != null && x.OriginalProperty.PropertyName == propertyName &&
                            x.OriginalProperty.ParentType == member.Target).FirstOrDefault();
                    if (column != null)
                    {
                        if (member.SourceType == m.Type && !member.SourceType.IsProjectedType(Backpack.ProjectionCache) && !member.SourceType.IsValueObjectType() ) // Only if the type is mapping
                            return MapPropertyToCurrentFromClause(m, currentFrom, new PropertyExpression((AliasedExpression)select, column), m.Type);


                        // Otherwise we have to create a join
                        var target = Property.GetPropertyInstance((PropertyInfo)member.Source).MetaInfo.LinkTarget;
                        exp = CreateInnerJoin(target, column);
                        again = true;
                        continue;
                    }

                    // Second check, if the alias is matching
                    column = select.Columns.Where(x => x.Alias.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (column != null)
                    {
                        if (member.SourceType == m.Type && !member.SourceType.IsProjectedType(Backpack.ProjectionCache) && !member.SourceType.IsValueObjectType()) // Only if the type is mapping
                            return MapPropertyToCurrentFromClause(m, currentFrom, new PropertyExpression((AliasedExpression)select, column), m.Type);

                        // Otherwise we have to create a join
                        var target = Property.GetPropertyInstance((PropertyInfo)member.Source).MetaInfo.LinkTarget;
                        exp = CreateInnerJoin(target, column);
                        again = true;
                        continue;
                    }
                }
            } while (again);

            // search for the property inside the expression
            return UpdateMemberAccess(m, exp, m.Member);
        }

        /// <summary>
        /// Creates the inner join.
        /// </summary>
        /// <param name="linkTarget">The link target.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private Expression CreateInnerJoin(Type linkTarget, ColumnDeclaration column)
        {
            var primaryKey = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(linkTarget);
            Alias newAlias = Alias.Generate(AliasType.Table);
            var targetProperty = new PropertyExpression(GetProjection(linkTarget), newAlias, primaryKey);

            Expression exp = new TableExpression(linkTarget, GetProjection(linkTarget), newAlias);
            var sourceProperty = FindSourceColumn(currentFrom, column).Expression as PropertyExpression;
            var where = Expression.MakeBinary(ExpressionType.Equal,
                                              sourceProperty.SetType(primaryKey.PropertyType), //new PropertyExpression(linkTarget, currentFrom, column), 
                                              targetProperty.SetType(primaryKey.PropertyType));
            currentFrom = new JoinExpression(linkTarget, GetProjection(linkTarget), JoinType.InnerJoin, currentFrom, exp, where);
            return exp;
        }


        /// <summary>
        /// Finds the source column.
        /// </summary>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private ColumnDeclaration FindSourceColumn(Expression currentFrom, Expression property)
        {
            if (currentFrom == null)
                return null;

            SelectExpression select = currentFrom as SelectExpression;
            PropertyExpression propertyExpression = property as PropertyExpression;

            // If It's a new Expression
            NewExpression nex = select != null ? select.Selector as NewExpression : null;
            if (nex != null && propertyExpression != null)
                for (int i = 0, n = nex.Members.Count; i < n; i++)
                    if (MembersMatch(nex.Members[i], propertyExpression.PropertyName))
                        return new ColumnDeclaration(nex.Arguments[i], Alias.Generate(propertyExpression.Name), propertyExpression.PropertyName);

            // Perhaps the complete object is gonne compared
            if (nex != null && DbExpressionComparer.AreEqual(property, nex))
                return new ColumnDeclaration(nex.Arguments.Single(), select.Columns.Single());

            if (propertyExpression != null)
                property = OriginPropertyFinder.Find(propertyExpression);
            
            var columns = ((IDbExpressionWithResult)currentFrom).Columns;
            return columns.Where(x =>
                x.Expression is PropertyExpression ?
                x.OriginalProperty.Equals(property) : DbExpressionComparer.AreEqual(property, x.Expression)
                ).FirstOrDefault();
        }

        /// <summary>
        /// Maps the property to current from clause.
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="currentFrom">The current from.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        protected Expression MapPropertyToCurrentFromClause(MemberExpression me, AliasedExpression currentFrom, Expression exp, Type resultType)
        {
            // Maybe we have to compare to the primary key
            var resultExp = exp as IDbExpressionWithResult;

            if (resultExp != null)
            {
                if (previousExpression is MemberExpression)
                    return AdjustType(exp, resultType);

                var pk = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(resultExp.RevealedType);
                var info = Internal.Property.GetPropertyInstance(pk);
                var col = resultExp.Columns.Where(x => x.PropertyName == info.MetaInfo.PropertyName).First();

                exp = col.Expression;
            }

            // Now setup the property expression in the current context
            PropertyExpression property = exp as PropertyExpression;

            if (!(exp is ValueExpression))
            {
                if (property != null)
                {
                    var source = FindSourceColumn(currentFrom, property);
                    if (source != null)
                        property = (exp = FindSourceColumn(currentFrom, source).Expression) as PropertyExpression;

                    if (property != null)
                        exp = property.SetType(resultType);
                }
                else
                {
                    // It's not a property, so we have to compare the complete tree inside ;(
                    var source = FindSourceColumn(currentFrom, exp);
                    if (source != null)
                        exp = new PropertyExpression(currentFrom, source);
                }
            }

            // Maybe we have to attach the MemberExpression again
            if (me != null && me.Member.DeclaringType.FullName.StartsWith("System."))
                exp = Expression.MakeMemberAccess(exp, me.Member);

            return exp;
        }

        /// <summary>
        /// Visits the comparison.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="queryOperator">The query operator.</param>
        /// <returns></returns>
        protected override Expression VisitComparison(BinaryExpression b, AdFactum.Data.Queries.QueryOperator queryOperator)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);

            if (left is IDbExpressionWithResult)
                left = MapPropertyToCurrentFromClause(null, currentFrom, left, left.Type);

            if (right is IDbExpressionWithResult)
                right = MapPropertyToCurrentFromClause(null, currentFrom, right, right.Type);

            Expression conversion = Visit(b.Conversion);
            return UpdateBinary(b, left, right, conversion, b.IsLiftedToNull, b.Method);
        }

        ///// <summary>
        ///// Visits the comparison.
        ///// </summary>
        ///// <param name="b">The b.</param>
        ///// <param name="queryOperator">The query operator.</param>
        ///// <returns></returns>
        //protected override Expression VisitComparison(BinaryExpression b, Queries.QueryOperator queryOperator)
        //{
        //    Expression left = Visit(b.Left);
        //    Expression right = Visit(b.Right);

        //    // Maybe we have to compare to the primary key
        //    var leftResultExp = left as IDbExpressionWithResult;
        //    var rightResultExp = right as IDbExpressionWithResult;

        //    if (leftResultExp != null)
        //    {
        //        var pk = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(leftResultExp.RevealedType);
        //        var info = Internal.Property.GetPropertyInstance(pk);
        //        var col = leftResultExp.Columns.Where(x => x.PropertyName == info.MetaInfo.PropertyName).First();

        //        left = col.Expression;
        //    }

        //    if (rightResultExp != null)
        //    {
        //        var pk = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(rightResultExp.RevealedType);
        //        var info = Internal.Property.GetPropertyInstance(pk);
        //        var col = rightResultExp.Columns.Where(x=>x.PropertyName == info.MetaInfo.PropertyName).First();

        //        right = col.Expression;
        //    }

        //    // Now setup the property expression in the current context
        //    PropertyExpression leftProperty = left as PropertyExpression;
        //    PropertyExpression rightProperty = right as PropertyExpression;

        //    if (!(left is ValueExpression))
        //    {
        //        if (leftProperty != null)
        //        {
        //            var source = FindSourceColumn(leftProperty);
        //            if (source != null)
        //                leftProperty = (left = FindSourceColumn(currentFrom, source, dynamicCache).Expression) as PropertyExpression;

        //            if (leftProperty != null)
        //                left = leftProperty.SetType(b.Left.Type);
        //        }
        //        else
        //        {
        //            // It's not a property, so we have to compare the complete tree inside ;(
        //            var source = FindSourceColumn(left);
        //            if (source != null)
        //                left = new PropertyExpression(currentFrom, source);
        //        }
        //    }

        //    if (!(right is ValueExpression))
        //    {
        //        if (rightProperty != null)
        //        {
        //            var source = FindSourceColumn(rightProperty);
        //            if (source != null)
        //                rightProperty = (right = FindSourceColumn(currentFrom, source, dynamicCache).Expression) as PropertyExpression;

        //            if (rightProperty != null)
        //                right = rightProperty.SetType(b.Right.Type);
        //        }
        //        else
        //        {
        //            // It's not a property, so we have to compare the complete tree inside ;(
        //            var source = FindSourceColumn(right);
        //            if (source != null)
        //                right = new PropertyExpression(currentFrom, source);
        //        }
        //    }

        //    Expression conversion = Visit(b.Conversion);
        //    return UpdateBinary(b, left, right, conversion, b.IsLiftedToNull, b.Method);
        //}

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            // Perhaps, we already visited the parameter
            Expression visit;
            if (visitedParameterMappings.TryGetValue(p, out visit))
                return AdjustType(visit, p.Type);
            

            // Get the corresponding table
            MappingStruct table;
            if (Backpack.ParameterMapping.TryGetValue(p, out table))
            {
                visit = Visit(table.Expression);
                visitedParameterMappings.Add(p, visit);
                Backpack.ParameterMapping[p] = new MappingStruct(visit);
                return AdjustType(visit, p.Type);
            }

            return new SqlParameterExpression(p.Type, null, p.Name);
        }

        /// <summary>
        /// Adjusts the type.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        protected Expression AdjustType(Expression exp, Type resultType)
        {
            AliasedExpression aliasedExpression = exp as AliasedExpression;
            if (aliasedExpression != null && aliasedExpression.Type != resultType)
                return aliasedExpression.SetType(resultType);

            return exp;
        }

        /// <summary>
        /// Visits the aggregate expression
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            var arg = Visit(aggregate.Argument);

            var select = arg as SelectExpression;
            if (select != null)
                arg = new PropertyExpression(currentFrom, ((IDbExpressionWithResult)currentFrom).Columns.First());

            var prop = arg as PropertyExpression;
            if (prop != null && currentFrom != null && prop.Alias != currentFrom.Alias)
                arg = RebindToSelection.Rebind(currentFrom, currentFrom, prop, Backpack);

            return UpdateAggregate(aggregate, aggregate.Type, aggregate.AggregateName, arg, aggregate.IsDistinct);
        }

        /// <summary>
        /// Visits the scalar expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitScalarExpression(ScalarExpression expression)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                currentFrom = VisitSource(expression.From);
                var columns = VisitColumnDeclarations(expression.Columns);

                return UpdateScalarExpression(expression, columns.First(), currentFrom);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            return base.VisitColumn(expression);
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                var left = VisitSource(join.Left);
                currentFrom = left;

                var right = VisitSource(join.Right);
                var condition = Visit(join.Condition);
                return UpdateJoin(join, join.Join, left, right, condition);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            var saveCurrentFrom = currentFrom;
            try
            {
                return base.VisitUnionExpression(union);
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }


        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var saveCurrentFrom = currentFrom;
            currentFrom = VisitSource(select.From);
            try
            {
                var where = Visit(select.Where);
                var orderBy = VisitOrderBy(select.OrderBy);
                var groupBy = VisitExpressionList(select.GroupBy);
                var skip = Visit(select.Skip);
                var take = Visit(select.Take);

                if (select.Selector != null)
                {
                    LambdaExpression lambda = select.Selector as LambdaExpression;
                    ParameterExpression p = lambda != null
                                                ? lambda.Parameters.FirstOrDefault() as ParameterExpression
                                                : null;
                    if (p != null)
                    {
                        if (visitedParameterMappings.ContainsKey(p))
                            visitedParameterMappings.Remove(p);
                        visitedParameterMappings.Add(p, currentFrom);
                    }
                }

                var selector = Visit(select.Selector);

                var defaultIfEmpty = Visit(select.DefaultIfEmpty);
                ProjectionClass projection;

                if (selector != null)
                {
                    projection = new ProjectionClass(selector);
                }
                else
                {
                    projection = select.Projection;

                    // If the new projection equals the old projection, than take the old one, because it is enriched with more infos
                    if (projection == null || projection.ProjectedType == currentFrom.Projection.ProjectedType)
                        projection = currentFrom.Projection;
                }

                if (currentFrom != select.From
                    || where != select.Where
                    || orderBy != select.OrderBy
                    || groupBy != select.GroupBy
                    || take != select.Take
                    || skip != select.Skip
                    || selector != select.Selector
                    || defaultIfEmpty != select.DefaultIfEmpty
                    )
                {
                    List<ColumnDeclaration> columns = GetColumns(currentFrom, select.Columns, selector, projection);
                    var readOnlyCollection = new ReadOnlyCollection<ColumnDeclaration>(columns);

                    select = new SelectExpression(select.Type, projection, select.Alias, readOnlyCollection, selector, currentFrom, where, orderBy, groupBy,
                                                skip, take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, defaultIfEmpty);
                }
                return select;
            }
            finally
            {
#if DEBUG
                // Check, if ater the SelectExpression the columns are valid
                ReferingColumnChecker.Validate(select);
#endif

                currentFrom = saveCurrentFrom;
            }
        }

        private HashSet<MemberInitExpression> memberInitExpressions = new HashSet<MemberInitExpression>();
    }
}
