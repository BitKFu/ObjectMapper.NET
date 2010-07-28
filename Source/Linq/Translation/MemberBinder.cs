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
    public class MemberBinder : DbExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, MappingStruct> fromClauseMapping =
            new Dictionary<ParameterExpression, MappingStruct>();

        private readonly Cache<Type, ProjectionClass> dynamicCache;

        private AliasedExpression currentFrom;

        /// <summary> Gets the Type Mapper</summary>
        protected ITypeMapper TypeMapper { get; private set; }

        private readonly Dictionary<ParameterExpression, Expression> visitedParameterMappings = new Dictionary<ParameterExpression, Expression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberBinder"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="typeMapper">The type mapper.</param>
        /// <param name="mapping">The mapping.</param>
        private MemberBinder(Cache<Type, ProjectionClass> cache, ITypeMapper typeMapper, Dictionary<ParameterExpression, MappingStruct> mapping)
        {
            TypeMapper = typeMapper;
            dynamicCache = cache;
            fromClauseMapping = mapping;

#if TRACE
            Console.WriteLine("\nMemberBinder:");
#endif
        }

        /// <summary>
        /// Evaluates the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns></returns>
        public static Expression Evaluate(Expression exp, Cache<Type, ProjectionClass> cache, ITypeMapper mapper, Dictionary<ParameterExpression, MappingStruct> mapping)
        {
            MemberBinder binder = new MemberBinder(cache, mapper, mapping);
            return binder.Visit(exp);
        }

        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private ProjectionClass GetProjection(Type type)
        {
            return ReflectionHelper.GetProjection(type.RevealType(), dynamicCache);
        }

        private static bool MembersMatch(MemberInfo a, string propertyName)
        {
            return a.Name.Substring(4) == propertyName;
        }

        private Expression previousExpression;
        private Stack<Expression> callStack = new Stack<Expression>();

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
        /// Visits the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            // Solve DateTime.Now, DateTime.Today
            if (m.Expression == null && m.Type == typeof(DateTime) && m.Member.Name == "Today")
                m = Expression.MakeMemberAccess(new SysDateExpression(), m.Member);

            if (m.Expression == null && m.Type == typeof(DateTime) && m.Member.Name == "Now")
                m = Expression.MakeMemberAccess(new SysTimeExpression(), m.Member);

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
                                return MapPropertyToCurrentFromClause(currentFrom, exp, m.Type);

                            var result = new PropertyExpression(currentFrom, new ColumnDeclaration(exp, Alias.Generate(columnName), propertyName));
                            return MapPropertyToCurrentFromClause(currentFrom, result, m.Type);
                        }
                }

                if (nex.Type.IsGenericType && nex.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                {
                    if (propertyName == "Key")
                    {
                        exp = nex.Arguments[0];
                        if (exp is AliasedExpression)
                            return MapPropertyToCurrentFromClause(currentFrom, exp, m.Type);

                        var result = new PropertyExpression(currentFrom, new ColumnDeclaration(exp, Alias.Generate(columnName), propertyName));
                        return MapPropertyToCurrentFromClause(currentFrom, result, m.Type);
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
                    return MapPropertyToCurrentFromClause(currentFrom, exp, m.Type);

                TableExpression table = exp as TableExpression;
                if (table != null)
                {
                    // First check, if the property is matching
                    var column =
                        table.Columns.Where(x => x.OriginalProperty != null && x.OriginalProperty.PropertyName == propertyName).
                            FirstOrDefault();
                    if (column != null)
                        return MapPropertyToCurrentFromClause(currentFrom, column.Expression, m.Type);

                    // Second check, if the alias is matching
                    column = table.Columns.Where(x => x.Alias.Name == columnName).FirstOrDefault();
                    if (column != null)
                        return MapPropertyToCurrentFromClause(currentFrom, column.Expression, m.Type);
                }

                IDbExpressionWithResult select = exp as IDbExpressionWithResult;
                if (select != null)
                {
                    // Ok. First check, if we have to create a join or something that way
                    var targetProjection = GetProjection(m.Expression.Type);
                    if (targetProjection.NewExpression != null)
                    {
                        var subParameter = (ParameterExpression)targetProjection.NewExpression.Arguments
                                                                     .Where(
                                                                     arg =>
                                                                     arg is ParameterExpression &&
                                                                     ((ParameterExpression)arg).Name == propertyName).
                                                                     FirstOrDefault();

                        if (subParameter != null)
                        {
                            var from = fromClauseMapping[subParameter].Expression;
                            from = from is SelectExpression
                                       ? ((SelectExpression)from).SetDefaultIfEmpty(select.DefaultIfEmpty)
                                       : from;
                            return MapPropertyToCurrentFromClause(currentFrom, from, m.Type);
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
                        if (member.SourceType == m.Type && !member.SourceType.IsProjectedType(dynamicCache)) // Only if the type is mapping
                            return MapPropertyToCurrentFromClause(currentFrom, new PropertyExpression((AliasedExpression)select, column), m.Type);

                        // Otherwise we have to create a join
                        exp = CreateInnerJoin(m.Type, column);
                        again = true;
                        continue;
                    }

                    // Second check, if the alias is matching
                    column = select.Columns.Where(x => x.Alias.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (column != null)
                    {
                        if (member.SourceType == m.Type && !member.SourceType.IsProjectedType(dynamicCache)) // Only if the type is mapping
                            return MapPropertyToCurrentFromClause(currentFrom, new PropertyExpression((AliasedExpression)select, column), m.Type);

                        // Otherwise we have to create a join
                        exp = CreateInnerJoin(m.Type, column);
                        again = true;
                        continue;
                    }
                }
            } while (again);

            // search for the property inside the expression
            return UpdateMemberAccess(m, exp, m.Member);
        }

        private Expression CreateInnerJoin(Type linkTarget, ColumnDeclaration column)
        {
            var primaryKey = ReflectionHelper.GetPrimaryKeyPropertyInfoNonRecursive(linkTarget);
            Alias newAlias = Alias.Generate(AliasType.Table);
            var targetProperty = new PropertyExpression(GetProjection(linkTarget), newAlias, primaryKey);

            Expression exp = new TableExpression(linkTarget, GetProjection(linkTarget), newAlias);
            var sourceProperty = FindSourceColumn(currentFrom, column, dynamicCache).Expression as PropertyExpression;
            var where = Expression.MakeBinary(ExpressionType.Equal,
                                              sourceProperty.SetType(primaryKey.PropertyType), //new PropertyExpression(linkTarget, currentFrom, column), 
                                              targetProperty.SetType(primaryKey.PropertyType));
            currentFrom = new JoinExpression(linkTarget, GetProjection(linkTarget), JoinType.InnerJoin, currentFrom, exp, where);
            return exp;
        }


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

        protected Expression MapPropertyToCurrentFromClause(AliasedExpression currentFrom, Expression exp, Type resultType)
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
                        property = (exp = FindSourceColumn(currentFrom, source, dynamicCache).Expression) as PropertyExpression;

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

            return exp;
        }

        protected override Expression VisitComparison(BinaryExpression b, AdFactum.Data.Queries.QueryOperator queryOperator)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);

            if (left is IDbExpressionWithResult)
                left = MapPropertyToCurrentFromClause(currentFrom, left, left.Type);

            if (right is IDbExpressionWithResult)
                right = MapPropertyToCurrentFromClause(currentFrom, right, right.Type);

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
            if (fromClauseMapping.TryGetValue(p, out table))
            {
                visit = Visit(table.Expression);
                visitedParameterMappings.Add(p, visit);
                fromClauseMapping[p] = new MappingStruct(visit);
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

        ///// <summary>
        ///// Visits the order expression.
        ///// </summary>
        ///// <param name="orderExpression">The order expression.</param>
        ///// <returns></returns>
        //protected override Expression VisitOrderExpression(OrderExpression orderExpression)
        //{
        //    OrderExpression exp = (OrderExpression) base.VisitOrderExpression(orderExpression);
        //    PropertyExpression propertyExpression = exp.Expression as PropertyExpression;
        //    if (propertyExpression != null)
        //        return new OrderExpression(orderExpression.Ordering,
        //                                   new PropertyExpression(currentFrom, propertyExpression));

        //    return exp;
        //}


        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            var arg = Visit(aggregate.Argument);

            PropertyExpression prop = arg as PropertyExpression;
            if (prop != null && currentFrom != null && prop.Alias != currentFrom.Alias)
                arg = RebindToSelection.Rebind(currentFrom, currentFrom, prop);

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
                var selector = Visit(select.Selector);
                var defaultIfEmpty = Visit(select.DefaultIfEmpty);

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
                    List<ColumnDeclaration> columns = GetColumns(currentFrom, select.Columns, selector, dynamicCache);

                    return new SelectExpression(select.Type, select.Projection, select.Alias, new ReadOnlyCollection<ColumnDeclaration>(columns), selector, currentFrom, where, orderBy, groupBy,
                                                skip, take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint, defaultIfEmpty);
                }
                return select;
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        public static List<ColumnDeclaration> GetColumns(AliasedExpression currentFrom, ReadOnlyCollection<ColumnDeclaration> existingColumns, Expression selector, Cache<Type, ProjectionClass> dynamicCache)
        {
            List<ColumnDeclaration> columns;

            if (selector == null)
                columns = ColumnProjector.Evaluate(currentFrom, dynamicCache).ToList();
            else
            {
                columns = new List<ColumnDeclaration>();
                var selectorColumns = ColumnProjector.Evaluate(selector, dynamicCache);

                for (int i = 0; i < selectorColumns.Count; i++)
                {
                    ColumnDeclaration cd = selectorColumns[i];
                    var declaration = FindSourceColumn(currentFrom, cd, dynamicCache);
                    if (declaration == null)
                        continue;
                    //   throw new AmbiguousMatchException("Column " + cd + " could not be found in the current result set.\nThat is mostly because a variable has be used ambiguously, e.g. in a Union - two different subselects share the same variables.");

                    // If the alias has been generated, than use the pre-existing one
                    if (declaration.Alias.Generated && selectorColumns.Count == existingColumns.Count)
                        declaration.Alias = existingColumns[i].Alias;

                    columns.Add(declaration);
                }
            }

            /*
             * Expand columns, if tried to access dependend objects
             */
            var toExpand = columns.Where(c => c.OriginalProperty != null && c.OriginalProperty.Expandable).ToList();
            foreach (var expand in toExpand)
            {
                Expression fromClause = FromExpressionFinder.Find(currentFrom, expand.OriginalProperty.ContentType);
                SelectExpression selectFrom = fromClause as SelectExpression;
                TableExpression tableFrom = fromClause as TableExpression;
                if (selectFrom != null)
                {
                    columns.Remove(expand);
                    columns.AddRange(selectFrom.Columns);
                }

                if (tableFrom != null)
                {
                    columns.Remove(expand);
                    columns.AddRange(tableFrom.Columns);
                }
            }
            return columns;
        }

        private HashSet<MemberInitExpression> memberInitExpressions = new HashSet<MemberInitExpression>();
    }
}
