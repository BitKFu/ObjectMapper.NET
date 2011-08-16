using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// The QueryBinder converts the real LINQ Query in a first step to a query that is more readable
    /// </summary>
    public class QueryBinder : DbPackedExpressionVisitor
    {
        private readonly Cache<string, ArrayList> attributeCache = new Cache<string, ArrayList>("Attribute Cache");

        //private readonly Stack<IRetriever> memberAccess = new Stack<IRetriever>();
        private Expression currentGroupElement;
        private Dictionary<Type, AliasedExpression> groupByFrom = new Dictionary<Type, AliasedExpression>();

        private Dictionary<Expression, GroupByInfo> groupByMap = new Dictionary<Expression, GroupByInfo>();

        /// <summary> Gets or sets the method name to define the context in with the properties are bound. </summary>
        private Stack<string> overallMethod = new Stack<string>();

        private readonly Stack<IRetriever> memberAccess = new Stack<IRetriever>();

        /// <summary>
        /// Describes the Load Level, if any has set </summary>
        public int Level { get; private set; }

        private void AddFromClauseMapping(ParameterExpression parameter, Expression exp)
        {
            Backpack.ParameterMapping[parameter] = new MappingStruct(exp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBinder"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="backpack">The backpack.</param>
        private QueryBinder(Expression expression, ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
            Root = expression;
            Level = HierarchyLevel.FlatObject;
        }

        /// <summary> Gets the attribute cache. </summary>
        protected Cache<string, ArrayList> AttributeCache
        {
            get { return attributeCache; }
        }

        /// <summary> Gets or sets the root. </summary>
        public Expression Root { get; private set; }

        /// <summary> Gets or sets the then bys. </summary>
        public List<OrderExpression> ThenBys { get; private set; }

        /// <summary>
        /// Gets the dynamic cache.
        /// </summary>
        /// <value>The dynamic cache.</value>
        protected Cache<Type, ProjectionClass> DynamicCache
        {
            get { return Backpack.ProjectionCache; }
        }

        /// <summary>
        /// Returns true, if distinct is allowed in aggregate functions
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow distinct in aggregates]; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool AllowDistinctInAggregates
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the lambda expression.
        /// </summary>
        protected LambdaExpression GetLambda(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
                e = ((UnaryExpression) e).Operand;

            if (e.NodeType == ExpressionType.Constant)
                return ((ConstantExpression) e).Value as LambdaExpression;

            return e as LambdaExpression;
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="groupings">The groupings.</param>
        /// <param name="backpack">The backpack.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public static Expression Evaluate(Expression expression, out List<PropertyTupel> groupings, ExpressionVisitorBackpack backpack, out int level)
        {
            var binder = new QueryBinder(expression, backpack);
            Expression result = binder.Visit(expression);
            groupings = binder.Groupings;
            level = binder.Level;

            var tableEx = result as TableExpression;
            if (tableEx != null)
            {
                Type projectedType = tableEx.RevealedType;
                ProjectionClass projection = ReflectionHelper.GetProjection(projectedType, backpack.ProjectionCache);
                ReadOnlyCollection<ColumnDeclaration> columns = projection.GetColumns(tableEx.Alias, backpack.ProjectionCache);
                var selectExpression = new SelectExpression(projectedType,  Alias.Generate(AliasType.Select), columns, null, tableEx, null);

                return selectExpression;
            }

            return result;
        }

        /// <summary>
        /// Visits the lambda.
        /// </summary>
        /// <param name="lambda">The lambda.</param>
        /// <returns></returns>
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            var constantBody = lambda.Body as ConstantExpression;

            // Special handling, if it's a constant expression
            if (constantBody != null)
                switch (overallMethod.Peek())
                {
                    case "Where":
                        return Visit(Expression.MakeBinary(ExpressionType.Equal, constantBody, constantBody));

                    default:
                        return base.VisitLambda(lambda);
                }


            // Maybe we have to correct the result of the Lambda Expression in case of !!! Employees.Where(e=>e.Disabled) !!!
            var corrected = CorrectComparisonWithoutOperator(lambda.Body);
            if (lambda.Body != corrected)
                lambda = UpdateLambda(lambda, lambda.Type, corrected, lambda.Parameters);

            // Now do normal lambda processing
            var result = (LambdaExpression)base.VisitLambda(lambda);
            return result;
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            overallMethod.Push(m.Method.Name);

            try
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        if (m.Arguments.Count == 2 && m.Arguments[1] is ConstantExpression && m == Root)
                            return BindTopLevelContains(m.Type, m);
                        break;
                    case "All":
                        if (m.Arguments.Count == 2)
                            return BindAnyAll(m.Type, m.Method, m.Arguments[0], GetLambda(m.Arguments[1]), m == Root);
                        break;
                    case "Any":
                        if (m.Arguments.Count == 1)
                        {
                            return BindAnyAll(m.Type, m.Method, m.Arguments[0], null, m == Root);
                        }
                        else if (m.Arguments.Count == 2)
                        {
                            return BindAnyAll(m.Type, m.Method, m.Arguments[0], GetLambda(m.Arguments[1]), m == Root);
                        }
                        break;
                    case "Where":
                        return BindWhere(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]));
                    case "Select":
                        return BindSelect(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]));
                    case "SelectMany":
                        switch (m.Arguments.Count)
                        {
                            case 2:
                                return BindSelectMany(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), null);
                            case 3:
                                return BindSelectMany(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]),
                                                      GetLambda(m.Arguments[2]));
                        }
                        break;
                    case "Join":
                        return BindJoin(m.Type, m.Arguments[0], m.Arguments[1], GetLambda(m.Arguments[2]),
                                        GetLambda(m.Arguments[3]), GetLambda(m.Arguments[4]));
                    case "Union":
                        return BindUnion(m.Type, m.Arguments[0], m.Arguments[1], false);
                    case "Concat":
                        if (m.Type != typeof(string))
                            return BindUnion(m.Type, m.Arguments[0], m.Arguments[1], true);
                        break;
                    case "GroupJoin":
                        if (m.Arguments.Count == 5)
                        {
                            return BindGroupJoin(m.Type, m.Method, m.Arguments[0], m.Arguments[1],
                                                 GetLambda(m.Arguments[2]), GetLambda(m.Arguments[3]),
                                                 GetLambda(m.Arguments[4]));
                        }
                        break;
                    case "Distinct":
                        if (m.Arguments.Count == 1)
                            return BindDistinct(m.Type, m.Arguments[0]);
                        break;
                    case "Skip":
                        if (m.Arguments.Count == 2)
                            return BindSkip(m.Type, m.Arguments[0], m.Arguments[1]);
                        break;
                    case "Take":
                        if (m.Arguments.Count == 2)
                            return BindTake(m.Type, m.Arguments[0], m.Arguments[1]);
                        break;
                    case "Reverse":
                        return BindReverse(m.Type, m.Arguments[0]);
                    case "First":
                    case "FirstOrDefault":
                    case "Single":
                    case "SingleOrDefault":
                    case "Last":
                    case "LastOrDefault":
                        switch (m.Arguments.Count)
                        {
                            case 1:
                                return BindFirst(m.Type, m.Arguments[0], null, m.Method.Name, m == Root);

                            case 2:
                                return BindFirst(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), m.Method.Name,
                                                 m == Root);
                        }
                        break;

                    case "OrderBy":
                        return BindOrderBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), Ordering.Asc);
                    case "OrderByDescending":
                        return BindOrderBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), Ordering.Desc);
                    case "ThenBy":
                        return BindThenBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), Ordering.Asc);
                    case "ThenByDescending":
                        return BindThenBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), Ordering.Desc);
                    case "GroupBy":
                        if (m.Arguments.Count == 2)
                        {
                            return BindGroupBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]), null, null);
                        }
                        if (m.Arguments.Count == 3)
                        {
                            LambdaExpression lambda1 = GetLambda(m.Arguments[1]);
                            LambdaExpression lambda2 = GetLambda(m.Arguments[2]);
                            if (lambda2.Parameters.Count == 1)
                            {
                                // second lambda is element selector
                                return BindGroupBy(m.Type, m.Arguments[0], lambda1, lambda2, null);
                            }
                            if (lambda2.Parameters.Count == 2)
                            {
                                // second lambda is result selector
                                return BindGroupBy(m.Type, m.Arguments[0], lambda1, null, lambda2);
                            }
                        }
                        else if (m.Arguments.Count == 4)
                        {
                            return BindGroupBy(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]),
                                               GetLambda(m.Arguments[2]), GetLambda(m.Arguments[3]));
                        }
                        break;
                    
                    case "Level":
                        return BindLevel(m.Type, m.Arguments[0], m.Arguments[1]);

                    case "SqlId":
                        return BindSqlId(m.Type, m.Arguments[0], (ConstantExpression)m.Arguments[1]);
                        
                    case "Hint":
                        return BindHint(m.Type, m.Arguments[0], (ConstantExpression)m.Arguments[1]);
                }

                if (IsAggregate(m.Method))
                {
                    switch (m.Arguments.Count)
                    {
                        case 1:
                            return BindAggregate(m.Arguments[0], m.Method, null, m == Root);
                        case 2:
                            return BindAggregate(m.Arguments[0], m.Method, GetLambda(m.Arguments[1]), m == Root);
                    }
                }

                return base.VisitMethodCall(m);
            }
            finally
            {
                overallMethod.Pop();
            }
        }

        /// <summary>
        /// Binds the level.
        /// </summary>
        private Expression BindLevel(Type type, Expression query, Expression level)
        {
            Level = (int) ((ConstantExpression) level).Value;
            return Visit(query);
        }

        /// <summary>
        /// Bind the SQL id
        /// </summary>
        private Expression BindSqlId(Type resultType, Expression source, ConstantExpression sqlId)
        {
            AliasedExpression from = VisitSource(source);
            Alias alias = Alias.Generate(AliasType.Select);

            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression) from).DefaultIfEmpty : null;
            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType, from.Projection, alias, columns, null, from, null, null, null, null, null, false,
                                        false, SelectResultType.Collection, sqlId.Value.ToString(), null, defaultIfEmpty);
        }

        /// <summary>
        /// Bind the Hint
        /// </summary>
        private Expression BindHint(Type resultType, Expression source, ConstantExpression hint)
        {
            AliasedExpression from = VisitSource(source);
            Alias alias = Alias.Generate(AliasType.Select);

            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;
            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType, from.Projection, alias, columns, null, from, null, null, null, null, null, false,
                                        false, SelectResultType.Collection, null, hint.Value.ToString(), defaultIfEmpty);
        }

        /// <summary>
        /// Searches for IEnumerable Types
        /// </summary>
        /// <param name="seqType"></param>
        /// <returns></returns>
        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof (string))
                return null;

            if (seqType.IsArray)
                return typeof (IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof (IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                        return ienum;
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof (object))
                return FindIEnumerable(seqType.BaseType);

            return null;
        }

        /// <summary>
        /// Returns the element Type
        /// </summary>
        /// <param name="seqType"></param>
        /// <returns></returns>
        private static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            return ienum == null
                       ? seqType
                       : ienum.GetGenericArguments().First();
        }

        /// <summary>
        /// Determines whether the specified expression is query.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// 	<c>true</c> if the specified expression is query; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsQuery(Expression expression)
        {
            Type elementType = GetElementType(expression.Type);
            return elementType != null &&
                   typeof (IQueryable<>).MakeGenericType(elementType).IsAssignableFrom(expression.Type);
        }

        /// <summary>
        /// Binds the all expression
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="isRoot">if set to <c>true</c> [is root].</param>
        /// <returns></returns>
        private Expression BindAnyAll(Type type, MethodInfo method, Expression source, LambdaExpression predicate,
                                      bool isRoot)
        {
            bool isAll = method.Name == "All";

            Expression from = Visit(source);

            // Solve Local Collections
            var constSource = from as ValueExpression;
            if (constSource != null && !IsQuery(constSource))
            {
                Debug.Assert(!isRoot);
                Expression where = null;
                foreach (object value in (IEnumerable) constSource.Value)
                {
                    Expression expr = ParameterBinder.BindParameter(predicate, predicate.Parameters[0],
                                                                    Expression.Constant(value));
                    where = where == null
                                ? expr
                                : (isAll
                                       ? where.AndAlso(expr)
                                       : where.OrElse(expr));
                }
                return Visit(where);
            }

            // Solve subselections
            if (isAll)
            {
                predicate = Expression.Lambda(Expression.Not(predicate.Body), predicate.Parameters.ToArray());
            }
            if (predicate != null)
            {
                from = VisitSource(Expression.Call(typeof (Enumerable), "Where", method.GetGenericArguments(), source, predicate));
            }

            var fromTable = from as TableExpression;
            if (fromTable != null)
                from = new SelectExpression(fromTable.Type,  Alias.Generate(AliasType.Select), fromTable.Columns, null,
                                            fromTable, null);

            Expression result = new ExistsExpression((SelectExpression) from);

            if (isAll)
                result = Expression.Not(result);

            if (isRoot)
            {
                result = Visit(Expression.Condition(result, Expression.Constant(true), Expression.Constant(false)));
                var column = new ColumnDeclaration(result, Alias.Generate(AliasType.Column));
                Root = result = new SelectExpression(type, ((SelectExpression)from).Projection, Alias.Generate(AliasType.Select),
                                                     new ReadOnlyCollection<ColumnDeclaration>(
                                                         new List<ColumnDeclaration> {column}),
                                                     result, null, null, null, null, null, null, false, false,
                                                     SelectResultType.SingleAggregate, null, null, null);
            }

            return result;
        }

        /// <summary>
        /// Binds a TopLevel Contains query
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Expression BindTopLevelContains(Type type, MethodCallExpression expression)
        {
            Root = null;
            Expression selector = Visit(Expression.Condition(expression, Expression.Constant(1), Expression.Constant(0)));
            var column = new ColumnDeclaration(selector, Alias.Generate(AliasType.Column));
            Root = new SelectExpression(type, null, Alias.Generate(AliasType.Select),
                                        new ReadOnlyCollection<ColumnDeclaration>(new List<ColumnDeclaration> {column}),
                                        selector, null, null, null, null, null, null, false, false,
                                        SelectResultType.SingleAggregate, null, null, null);
            return Root;
        }

        /// <summary>
        /// Union expression binding
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="unionAll">if set to <c>true</c> [union all].</param>
        /// <returns></returns>
        private Expression BindUnion(Type resultType, Expression first, Expression second, bool unionAll)
        {
            AliasedExpression firstSource = VisitSource(first);
            AliasedExpression secondSource = VisitSource(second);

            var firstTable = firstSource as TableExpression;
            if (firstTable != null)
                firstSource = new SelectExpression(firstTable.Type, Alias.Generate(AliasType.Select), firstTable.Columns,
                                                   null, firstTable, null);

            var secondTable = secondSource as TableExpression;
            if (secondTable != null)
                secondSource = new SelectExpression(secondTable.Type,  Alias.Generate(AliasType.Select),
                                                    secondTable.Columns, null, secondTable, null);

            var alias = Alias.Generate(AliasType.Union);

            ReadOnlyCollection<ColumnDeclaration> columns;
            
            if (resultType.RevealType().IsProjectedType(DynamicCache))
                columns = GetProjection(resultType.RevealType()).GetColumns(alias, DynamicCache);
            else
                columns = new ReadOnlyCollection<ColumnDeclaration>(new List<ColumnDeclaration>());

            return new UnionExpression(resultType, GetProjection(resultType), firstSource, secondSource, alias, columns, unionAll);
        }



        /// <summary>
        /// Returns true, if the given method is an aggregate function
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public virtual bool IsAggregate(MethodInfo method)
        {
            //if (method.DeclaringType == typeof(Queryable)
            //    || method.DeclaringType == typeof(Enumerable))
            //{
            switch (method.Name)
            {
                case "Count":
                case "LongCount":
                case "Sum":
                case "Min":
                case "Max":
                case "Average":
                    return true;
            }
            //}
            return false;
        }

        /// <summary>
        /// Binds the group join.
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="groupJoinMethod">The group join method.</param>
        /// <param name="outerSource">The outer source.</param>
        /// <param name="innerSource">The inner source.</param>
        /// <param name="outerKey">The outer key.</param>
        /// <param name="innerKey">The inner key.</param>
        /// <param name="resultSelector">The result selector.</param>
        /// <returns></returns>
        protected virtual Expression BindGroupJoin(Type resultType, MethodInfo groupJoinMethod, Expression outerSource,
                                                   Expression innerSource, LambdaExpression outerKey,
                                                   LambdaExpression innerKey, LambdaExpression resultSelector)
        {
            Type[] args = groupJoinMethod.GetGenericArguments();

            AliasedExpression outerProjection = VisitSource(outerSource);

            AddFromClauseMapping(outerKey.Parameters[0],outerProjection);
            LambdaExpression predicateLambda = Expression.Lambda(innerKey.Body.Equal(outerKey.Body), innerKey.Parameters[0]);
            MethodCallExpression callToWhere = Expression.Call(typeof (Enumerable), "Where", new[] {args[1]}, innerSource, predicateLambda);
            AliasedExpression group = VisitSource(callToWhere);

            AddFromClauseMapping(resultSelector.Parameters[0], outerProjection);
            AddFromClauseMapping(resultSelector.Parameters[1], group);
            Expression resultExpr = Visit(resultSelector.Body);

            Alias alias = Alias.Generate(AliasType.Select);
            ReadOnlyCollection<ColumnDeclaration> pc = ColumnProjector.Evaluate(resultExpr, DynamicCache);
            return new SelectExpression(resultType, alias, pc, resultExpr, outerProjection, null);
        }

        /// <summary>
        /// Binds the join.
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="outerSource">The outer source.</param>
        /// <param name="innerSource">The inner source.</param>
        /// <param name="outerKey">The outer key.</param>
        /// <param name="innerKey">The inner key.</param>
        /// <param name="resultSelector">The result selector.</param>
        /// <returns></returns>
        protected virtual Expression BindJoin(Type resultType, Expression outerSource, Expression innerSource,
                                              LambdaExpression outerKey, LambdaExpression innerKey,
                                              LambdaExpression resultSelector)
        {
            AliasedExpression outerProjection = VisitSource(outerSource);
            AliasedExpression innerProjection = VisitSource(innerSource);
            AddFromClauseMapping(outerKey.Parameters[0], outerProjection);
            Expression outerKeyExpr = Visit(outerKey.Body);

            AddFromClauseMapping(innerKey.Parameters[0], innerProjection);
            Expression innerKeyExpr = Visit(innerKey.Body);

            AddFromClauseMapping(resultSelector.Parameters[0], outerProjection);
            AddFromClauseMapping(resultSelector.Parameters[1], innerProjection);

            Expression resultExpr = Visit(resultSelector.Body);
            var join = new JoinExpression(resultType, GetProjection(resultType), JoinType.InnerJoin, outerProjection, innerProjection,
                                          Visit(outerKeyExpr.Equal(innerKeyExpr)));
            Alias alias = Alias.Generate(AliasType.Select);
            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(resultExpr, DynamicCache);
                //M, alias, outerProjection.Alias, innerProjection.Alias);
            return new SelectExpression(resultType, alias, columns, resultExpr, join, null);
        }

        /// <summary>
        /// Binds the reverse keyword. That means to create a new Selection that reverts the selection
        /// </summary>
        private Expression BindReverse(Type resultType, Expression source)
        {
            AliasedExpression from = VisitSource(source);
            Alias alias = Alias.Generate(AliasType.Select);
            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;

            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType, from.Projection,  alias, columns, null, from, null, null, null, null, null, false,
                                        true, SelectResultType.Collection, null, null, defaultIfEmpty);
        }

        /// <summary>
        /// This method is used to bind the where expression tree.
        /// </summary>
        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            ParameterExpression fromIdentifier = predicate.Parameters.First();

            AliasedExpression from = VisitSource(source);
            AddFromClauseMapping(fromIdentifier, StripExpression(from));

            Expression where = Visit(predicate);
            Alias alias = Alias.Generate(AliasType.Select);
            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);

            var selector = (from is IDbExpressionWithResult) ? ((IDbExpressionWithResult) from).Selector : null;
            return new SelectExpression(resultType, alias, columns, null, from, where);
        }


        /// <summary>
        /// This method is used to bind the distinct keyword
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private Expression BindDistinct(Type resultType, Expression source)
        {
            AliasedExpression from = VisitSource(source);
            SelectExpression select = from as SelectExpression;
            Alias alias = Alias.Generate(AliasType.Select);
            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;

            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType,from.Projection, alias, columns, select != null ? select.Selector : null, from, null, null, null, null, null, true,
                                        false, SelectResultType.Collection, null, null, defaultIfEmpty);
        }

        /// <summary>
        /// This method is used to bind the Take Row parameter
        /// </summary>
        private Expression BindTake(Type resultType, Expression source, Expression take)
        {
            take = Visit(take);
            AliasedExpression from = VisitSource(source);
            Alias alias = Alias.Generate(AliasType.Select);
            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;

            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType,from.Projection, alias, columns, null, from, null, null, null, null, take, false,
                                        false, SelectResultType.Collection, null, null, defaultIfEmpty);
        }

        /// <summary>
        /// This method is used to bind the Skip Row parameter
        /// </summary>
        private Expression BindSkip(Type resultType, Expression source, Expression skip)
        {
            skip = Visit(skip);
            AliasedExpression from = VisitSource(source);
            Alias alias = Alias.Generate(AliasType.Select);
            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;

            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(from, DynamicCache);
            return new SelectExpression(resultType, from.Projection, alias, columns, null, from, null, null, null, skip, null, false,
                                        false, SelectResultType.Collection, null, null, defaultIfEmpty);
        }

        /// <summary>
        /// Binds the select many. clause
        /// </summary>
        private Expression BindSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector,
                                          LambdaExpression resultSelector)
        {
            ParameterExpression fromIdentifier = collectionSelector.Parameters[0];
            AliasedExpression fromLeft = VisitSource(source);
            AddFromClauseMapping(fromIdentifier, fromLeft);

            // Maybe we only want to ungroup a grouping.
            // That's the case, when the resultSelect is NULL
            if (resultSelector == null)
            {
                // if so, return the FROM Clause of the selection, or the table itself
                var selectLeft = fromLeft as SelectExpression;
                if (selectLeft != null) return selectLeft.From;
                return fromLeft;
            }

            Expression collection = collectionSelector;

            // check for DefaultIfEmpty
            Expression defaultIfEmpty = null;
            var lambda = collection as LambdaExpression;
            MethodCallExpression mcs = lambda != null ? lambda.Body as MethodCallExpression : null;
            if (mcs != null && mcs.Method.Name == "DefaultIfEmpty" && mcs.Arguments.Count == 1 &&
                (mcs.Method.DeclaringType == typeof (Queryable) || mcs.Method.DeclaringType == typeof (Enumerable)))
            {
                MethodCallExpression groupJoin = source as MethodCallExpression;
                NewExpression groupResult = ExpressionTypeFinder.Find(groupJoin.Arguments[4], ExpressionType.New) as NewExpression;
                ParameterExpression leftParameter = groupResult.Arguments[1] as ParameterExpression;
                SelectExpression groupConditionSelect = Backpack.ParameterMapping[leftParameter].Expression as SelectExpression;

                fromLeft = ((SelectExpression) fromLeft).From;
                collection = mcs.Arguments[0];
                MemberExpression memberAccess = (MemberExpression) ExpressionTypeFinder.Find(collection, ExpressionType.MemberAccess);

                Expression condition = groupConditionSelect.Where;

                fromLeft = fromLeft is SelectExpression
                               ? ((SelectExpression) fromLeft).SetDefaultIfEmpty(condition)
                               : fromLeft;
                defaultIfEmpty = condition; 
            }

            AliasedExpression fromRight = VisitSource(collection);
            bool isTable = fromRight is TableExpression;

            JoinType joinType = isTable
                                    ? JoinType.CrossJoin
                                    : defaultIfEmpty != null ? JoinType.OuterApply : JoinType.CrossApply;

            var join = new JoinExpression(resultType, GetProjection(resultType), joinType, fromLeft, fromRight, null);
            AddFromClauseMapping(resultSelector.Parameters[0], fromLeft);
            AddFromClauseMapping(resultSelector.Parameters[1], fromRight);

            Alias alias = Alias.Generate(AliasType.Select);
            //var resultColumns = join; 
            
            // Add the Projection result to the dynamic cache
            ProjectionClass sourceProjection = GetProjection(source.Type.RevealType());
            
            var resultColumns = Visit(resultSelector.Body);
            ProjectionClass projection = new ProjectionClass(resultColumns, sourceProjection.ComplexTypeColumnMapping); //, fromClauseMapping);
            projection.ProjectedType = resultType.RevealType();
            DynamicCache.Insert(resultType.RevealType(), projection);

            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(resultColumns, DynamicCache);
            return new SelectExpression(resultType, alias, columns, resultColumns, join, null, defaultIfEmpty).SetProjection(projection);
        }

        /// <summary>
        /// Aggregates the argument is predicate.
        /// </summary>
        /// <param name="aggregateName">Name of the aggregate.</param>
        /// <returns></returns>
        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        /// <summary>
        /// Binds an aggregate function
        /// </summary>
        /// <param name="source"></param>
        /// <param name="method"></param>
        /// <param name="argument"></param>
        /// <param name="isRoot"></param>
        /// <returns></returns>
        private Expression BindAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot)
        {
            Type returnType = method.ReturnType;
            string aggName = method.Name;
            bool hasPredicateArg = AggregateArgumentIsPredicate(aggName);
            bool isDistinct = false;
            bool argumentWasPredicate = false;
            bool useAlternateArg = false;

            // check for distinct
            MethodCallExpression mcs = source as MethodCallExpression;
            if (mcs != null && !hasPredicateArg && argument == null)
            {
                if (mcs.Method.Name == "Distinct" && mcs.Arguments.Count == 1 &&
                    (mcs.Method.DeclaringType == typeof (Queryable) || mcs.Method.DeclaringType == typeof (Enumerable)))
                    // && this.mapping.Language.AllowDistinctInAggregates)
                {
                    source = mcs.Arguments[0];
                    isDistinct = true;
                }
            }

            var projection = this.VisitSource(source);

            if (argument != null && hasPredicateArg)
            {
                // convert query.Count(predicate) into query.Where(predicate).Count()
                var tableProjection = projection as TableExpression;
                if (tableProjection != null)
                {
                    Backpack.ParameterMapping[argument.Parameters[0]] = new MappingStruct(tableProjection);
                    projection = new SelectExpression(tableProjection.Type, Alias.Generate(AliasType.Select),
                                                      ColumnProjector.Evaluate(tableProjection, DynamicCache), null,
                                                      tableProjection, Visit(argument.Body));
                }
                else
                {
                    var selectProjection = projection as SelectExpression;
                    if (selectProjection != null)
                    {
                        Backpack.ParameterMapping[argument.Parameters[0]] =
                            new MappingStruct((AliasedExpression)selectProjection.From);
                        if (argument.Body is BinaryExpression)
                            projection = selectProjection.Where == null
                                             ? selectProjection.SetWhere(Visit(argument.Body))
                                             : selectProjection.SetWhere(
                                                   selectProjection.Where.AndAlso(Visit(argument.Body)));
                    }
                }
                argument = null;
                argumentWasPredicate = true;
            }

            Expression argExpr = null;
            if (argument != null)
            {
                AddFromClauseMapping(argument.Parameters[0], projection);
                argExpr = this.Visit(argument.Body);
            }
            else if (!hasPredicateArg || useAlternateArg)
            {
                argExpr = projection; //new PropertyExpression(projection, ((IDbExpressionWithResult) projection).Columns.First());
            }

            var alias = Alias.Generate(AliasType.Select);
            var pc = ColumnProjector.Evaluate(projection, DynamicCache);
            Expression aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);
            var columns = new List<ColumnDeclaration>{new ColumnDeclaration(aggExpr, Alias.Generate(AliasType.Column))};
            //SelectExpression select = new SelectExpression(source.Type, alias, new ReadOnlyCollection<ColumnDeclaration> ( columns ), projection, null, null);

            if (isRoot)
            {
                //ParameterExpression p = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(aggExpr.Type), "p");
                //LambdaExpression gator = Expression.Lambda(Expression.Call(typeof(Enumerable), "Single", new Type[] { returnType }, p), p);

                return new ScalarExpression(returnType, Alias.Generate(AliasType.Select),
                                            columns.First(), aggExpr, projection);
            }

            ScalarExpression subquery = new ScalarExpression(returnType, Alias.Generate(AliasType.Select), columns.First(), aggExpr, projection);

            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to 
            // optimize the aggregate expression later using AggregateRewriter
            GroupByInfo info;
            if (!argumentWasPredicate && this.groupByMap.TryGetValue(projection, out info))
            {
                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that 
                // would be legal to add to the columns in the select expression that has the corresponding group-by clause.
                if (argument != null)
                {
                    AddFromClauseMapping(argument.Parameters[0], info.Element);
                    argExpr = this.Visit(argument.Body);
                }
                else if (!hasPredicateArg || useAlternateArg)
                {
                    argExpr = info.Element;
                }
                aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);

                // check for easy to optimize case.  If the projection that our aggregate is based on is really the 'group' argument from
                // the query.GroupBy(xxx, (key, group) => yyy) method then whatever expression we return here will automatically
                // become part of the select expression that has the group-by clause, so just return the simple aggregate expression.
                if (projection == this.currentGroupElement)
                    return aggExpr;

                return new AggregateSubqueryExpression(info.Alias, aggExpr, subquery);
            }

            return subquery;


//            Type returnType = method.ReturnType;
//            string aggName = method.Name;
//            bool hasPredicateArg = AggregateArgumentIsPredicate(aggName);
//            bool isDistinct = false;
//            bool argumentWasPredicate = false;

//            // check for distinct
//            var mcs = source as MethodCallExpression;
//            if (mcs != null && !hasPredicateArg && argument == null)
//            {
//                if (mcs.Method.Name == "Distinct" && mcs.Arguments.Count == 1 &&
//                    (mcs.Method.DeclaringType == typeof (Queryable) || mcs.Method.DeclaringType == typeof (Enumerable)))
////                    && FromClauseMappingping.Language.AllowDistinctInAggregates)
//                {
//                    source = mcs.Arguments[0];
//                    isDistinct = true;
//                }
//            }

//            //AliasedExpression projection = VisitSource(source);

//            //if (argument != null && hasPredicateArg)
//            //{
//            //    // convert query.Count(predicate) into query.Where(predicate).Count()
//            //    var tableProjection = projection as TableExpression;
//            //    if (tableProjection != null)
//            //    {
//            //        FromClauseMapping[argument.Parameters[0]] = new MappingStruct(tableProjection);
//            //        projection = new SelectExpression(tableProjection.Type, Alias.Generate(AliasType.Select),
//            //                                          ColumnProjector.Evaluate(tableProjection, DynamicCache), null,
//            //                                          tableProjection, Visit(argument.Body));
//            //    }
//            //    else
//            //    {
//            //        var selectProjection = projection as SelectExpression;
//            //        if (selectProjection != null)
//            //        {
//            //            FromClauseMapping[argument.Parameters[0]] =
//            //                new MappingStruct((AliasedExpression) selectProjection.From);
//            //            if (argument.Body is BinaryExpression)
//            //                projection = selectProjection.Where == null
//            //                                 ? selectProjection.SetWhere(Visit(argument.Body))
//            //                                 : selectProjection.SetWhere(
//            //                                       selectProjection.Where.AndAlso(Visit(argument.Body)));
//            //        }
//            //    }
//            //    argument = null;
//            //    argumentWasPredicate = true;
//            //}

//            //if (groupByFrom.ContainsKey(projection.Type))
//            //    groupByFrom.TryGetValue(projection.Type, out projection);

//            if (argument != null && hasPredicateArg)
//            {
//                // convert query.Count(predicate) into query.Where(predicate).Count()
//                source = Expression.Call(typeof(Queryable), "Where", new[] { source.Type.RevealType() }, source, argument);
//                argument = null;
//                argumentWasPredicate = true;
//            }

//            AliasedExpression projection = this.VisitSource(source);

//            Expression argExpr = null;
//            if (argument != null)
//            {
//                AddFromClauseMapping(argument.Parameters[0], projection);
//                argExpr = Visit(argument.Body);
//            }
//            else if (!hasPredicateArg)
//            {
//                var selectProjection = projection as SelectExpression;
//                if (selectProjection != null)
//                {
//                    ColumnDeclaration column = selectProjection.Columns.FirstOrDefault();
//                    if (column != null)
//                        argExpr = new PropertyExpression(selectProjection, column);
//                    else
//                        argExpr = selectProjection.Selector;
//                }
//                else
//                    argExpr = projection;
//            }

//            Alias alias = Alias.Generate(AliasType.Select);
//            Expression aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);
            
//            //var select = new SelectExpression(returnType, alias,
//            //        new ReadOnlyCollection<ColumnDeclaration>(new[] {
//            //            new ColumnDeclaration(aggExpr,Alias.Generate(AliasType.Column))}),
//            //            null, projection, null, null, null, null, null, false, false,
//            //            SelectResultType.SingleAggregate, null, null);

//            var select = new ScalarExpression(returnType, alias, new ColumnDeclaration(aggExpr, Alias.Generate(AliasType.Column)), argExpr, projection);

//            if (isRoot)
//                return select;


//            AliasedExpression subquery = select;

//            // if we can find the corresponding group-info we can build a special AggregateSubquery node that will enable us to 
//            // optimize the aggregate expression later using AggregateRewriter
//            GroupByInfo info;
//            if (!argumentWasPredicate && groupByMap.TryGetValue(projection, out info))
//            {
//                // use the element expression from the group-by info to rebind the argument so the resulting expression is one that 
//                // would be legal to add to the columns in the select expression that has the corresponding group-by clause.
//                if (argument != null)
//                {
//                    var aliasedInfoElement = info.Element as AliasedExpression;
//                    if (aliasedInfoElement != null)
//                    {
//                        AddFromClauseMapping(argument.Parameters[0], info.Element);
//                        argExpr = Visit(argument.Body);
//                    }
//                }
//                else if (!hasPredicateArg)
//                {
//                    argExpr = info.Element;
//                }
//                aggExpr = new AggregateExpression(returnType, aggName, argExpr, isDistinct);

//                // check for easy to optimize case.  If the projection that our aggregate is based on is really the 'group' argument from
//                // the query.GroupBy(xxx, (key, group) => yyy) method then whatever expression we return here will automatically
//                // become part of the select expression that has the group-by clause, so just return the simple aggregate expression.
//                if (projection == currentGroupElement)
//                    return aggExpr;

//                //aggExpr = RebindToSelection.Rebind(outerProjection, info.Element);
//                return new AggregateSubqueryExpression(info.Alias, aggExpr, (SelectExpression) subquery);
//            }

//            return subquery;
        }

        /// <summary>
        /// Visits the new.
        /// </summary>
        /// <param name="nex">The nex.</param>
        /// <returns></returns>
        protected override NewExpression VisitNew(NewExpression nex)
        {
            overallMethod.Push("New");
            try
            {
                Type key = nex.Type;
                if (key == typeof (DateTime))
                    return base.VisitNew(nex);

                //ProjectionClass cachedProjection = DynamicCache.Get(key);
                //if (cachedProjection != null)
                //    return base.VisitNew(nex);

                // Evaluate New Expression
                nex = base.VisitNew(nex);

                var projection = new ProjectionClass(nex); //, fromClauseMapping);
                DynamicCache.Insert(key, projection);

                return nex;
            }
            finally
            {
                overallMethod.Pop();
            }
        }

        /// <summary>
        /// Visits the member init.
        /// </summary>
        /// <param name="init">The init.</param>
        /// <returns></returns>
        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            Expression result = base.VisitMemberInit(init);

            Type key = init.Type;
            ProjectionClass cachedProjection = DynamicCache.Get(key);

            if (cachedProjection != null)
                cachedProjection.Expression = init;
            else
            {
                var projection = new ProjectionClass(init); //, fromClauseMapping);
                DynamicCache.Insert(key, projection);
            }

            return result;
        }

        /// <summary>
        /// Binds members assignments to a projection class
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            assignment = base.VisitMemberAssignment(assignment);

            if (assignment.BindingType != MemberBindingType.Assignment)
                return assignment;

            // Try to get the projection and the property info
            ProjectionClass cachedProjection = DynamicCache.Get(assignment.Member.ReflectedType);
            PropertyInfo propertyInfo = assignment.Member as PropertyInfo;
            if (propertyInfo == null)
                return assignment;

            Property property = Property.GetPropertyInstance(propertyInfo);
            if (cachedProjection != null && !cachedProjection.MemberBindings.ContainsKey(property))
                cachedProjection.MemberBindings.Add(property, assignment.Expression);
            return assignment;
        }

        /// <summary> 
        /// Binds the first expression. 
        /// </summary>
        private Expression BindFirst(Type resultType, Expression source, LambdaExpression predicate, string kind,
                                     bool isRoot)
        {
            ParameterExpression fromIdentifier = predicate != null ? predicate.Parameters.First() : null;
            AliasedExpression from = VisitSource(source);

            Expression where = null;
            if (predicate != null)
            {
                AddFromClauseMapping(fromIdentifier, from);
                where = Visit(predicate);
            }

            bool isFirst = kind.StartsWith("First") || kind.StartsWith("Single");
            bool isLast = kind.StartsWith("Last");
            SelectResultType selectResultType = kind.EndsWith("OrDefault")
                                                    ? SelectResultType.SingleObjectOrDefault
                                                    : SelectResultType.SingleObject;

            Expression take = (isFirst || isLast) ? Expression.Constant(1) : null;

            ReadOnlyCollection<ColumnDeclaration> pc = ColumnProjector.Evaluate(from, DynamicCache);

            var defaultIfEmpty = from is SelectExpression ? ((SelectExpression)from).DefaultIfEmpty : null;
            Alias alias = Alias.Generate(AliasType.Select);
            if (take != null || where != null)
                return new SelectExpression(resultType,from.Projection, alias, pc, null, from, where, null, null, null, take, false,
                                            isLast, selectResultType, null, null, defaultIfEmpty);

            return isRoot
                       ? new SelectExpression(resultType, from.Projection, alias, pc, null, from, where, null, null, null, null, false,
                                              false, selectResultType, null, null, defaultIfEmpty)
                       : null;
        }

        /// <summary> 
        /// Binds the order by. 
        /// </summary>
        protected virtual Expression BindOrderBy(Type resultType, Expression source, LambdaExpression selector,
                                                 Ordering ordering)
        {
            ParameterExpression fromIdentifier = selector.Parameters.First();

            List<OrderExpression> myThenBys = ThenBys;
            ThenBys = null;

            AliasedExpression from = VisitSource(source);
            AddFromClauseMapping(fromIdentifier, StripExpression(from));

            LambdaExpression visitedSelector = (LambdaExpression) Visit(selector);
            var orderings = new List<OrderExpression> { new OrderExpression(ordering, visitedSelector.Body) };

            if (myThenBys != null)
            {
                for (int i = myThenBys.Count - 1; i >= 0; i--)
                {
                    OrderExpression tb = myThenBys[i];
                    var lambda = (LambdaExpression) tb.Expression;
                    fromIdentifier = lambda.Parameters[0];
                    AddFromClauseMapping(fromIdentifier, from);
                    orderings.Add(new OrderExpression(tb.Ordering, Visit(lambda.Body)));
                }
            }

            ReadOnlyCollection<ColumnDeclaration> pc = ColumnProjector.Evaluate(from, DynamicCache);

            var resultSelector = (from is IDbExpressionWithResult) ? ((IDbExpressionWithResult)from).Selector : null;
            Alias alias = Alias.Generate(AliasType.Select);
            return new SelectExpression(resultType, alias, pc, resultSelector, from, null, orderings.AsReadOnly(), null);
        }

        /// <summary>
        /// Bind following orderings
        /// </summary>
        protected virtual Expression BindThenBy(Type resultType, Expression source, LambdaExpression orderSelector,
                                                Ordering orderType)
        {
            if (ThenBys == null)
                ThenBys = new List<OrderExpression>();

            ThenBys.Add(new OrderExpression(orderType, orderSelector));
            return Visit(source);
        }

        /// <summary>
        /// Bind the grouping expression
        /// </summary>
        protected virtual Expression BindGroupBy(Type resultType, Expression source, LambdaExpression keySelector,
                                                 LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            // Visit the Source
            AliasedExpression projection = VisitSource(source);
            AddFromClauseMapping(keySelector.Parameters[0], projection);

            // Place the projection into the FROM Mapping
            Expression keyExpr = Visit(keySelector.Body);

            Groupings.AddRange(GroupingBinder.Evaluate(resultType, keyExpr));

            Expression elemExpr = projection;
            if (elementSelector != null)
            {
                AddFromClauseMapping(elementSelector.Parameters[0], projection);
                elemExpr = Visit(elementSelector.Body);
            }

            // Use ProjectColumns to get group-by expressions from key expression
            //ReadOnlyCollection<ColumnDeclaration> keyProjection = ColumnProjector.Evaluate(keyExpr, DynamicCache);
                //, projection.Select.Alias, projection.Select.Alias);
            List<Expression> groupExprs = new List<Expression>{keyExpr}; // keyProjection.Select(c => c.Expression).ToList();}

            // make duplicate of source query as basis of element subquery by visiting the source again
            AliasedExpression subqueryBasis = VisitSource(source);

            // recompute key columns for group expressions relative to subquery (need these for doing the correlation predicate)
            AddFromClauseMapping(keySelector.Parameters[0], subqueryBasis);
            Expression subqueryKey = Visit(keySelector.Body);

            //// Turn it into a select expression
            //Alias alias = Alias.Generate(AliasType.Select);
            //ReadOnlyCollection<ColumnDeclaration> pc = ColumnProjector.Evaluate(keyExpr, DynamicCache);
            //var result = new SelectExpression(resultType, alias, pc, null, projection, null, null,
            //                                  new ReadOnlyCollection<Expression>(groupExprs));

            //// ReBind the Grouping properties to the surrounding collection in order to make them available to the element subquery
            //groupExprs = groupExprs.Select(c => RebindToSelection.Rebind(result, c)).ToList();

            // use same projection trick to get group-by expressions based on subquery
            //ReadOnlyCollection<ColumnDeclaration> subqueryKeyPc = ColumnProjector.Evaluate(subqueryKey, DynamicCache);
            //List<Expression> subqueryGroupExprs = new List<Expression> {subqueryKey}; //subqueryKeyPc.Select(c => c.Expression).ToList());}

            //Expression subqueryCorrelation = BuildPredicateWithNullsEqual(subqueryGroupExprs, groupExprs);

            // compute element based on duplicated subquery
            Expression subqueryElemExpr = subqueryBasis;
            if (elementSelector != null)
            {
                AddFromClauseMapping(elementSelector.Parameters[0], subqueryBasis);
                subqueryElemExpr = Visit(elementSelector.Body);
            }

            // build subquery that projects the desired element
            Alias elementAlias = Alias.Generate(AliasType.Select);
            ReadOnlyCollection<ColumnDeclaration> elementPc = ColumnProjector.Evaluate(subqueryElemExpr, DynamicCache);
            var elementSubquery = new SelectExpression(subqueryElemExpr.Type, elementAlias, elementPc, subqueryElemExpr,
                                                       subqueryBasis, null /*subqueryCorrelation*/);

            // Turn it into a select expression
            var alias = Alias.Generate(AliasType.Select);

            // make it possible to tie aggregates back to this group-by
            var info = new GroupByInfo(alias, elemExpr);
            groupByMap.Add(elementSubquery, info);

            Expression resultExpr;
            if (resultSelector != null)
            {
                Expression saveGroupElement = currentGroupElement;
                currentGroupElement = elementSubquery;
                // compute result expression based on key & element-subquery
                AddFromClauseMapping(resultSelector.Parameters[0], keyExpr as AliasedExpression);
                AddFromClauseMapping(resultSelector.Parameters[1], projection);
                resultExpr = Visit(resultSelector.Body);
                currentGroupElement = saveGroupElement;

                //resultExpr = RebindToSelection.Rebind(projection, resultExpr);

            }
            else
            {
                // result must be IGrouping<K,E>
                resultExpr =
                    Expression.New(
                        typeof (Grouping<,>).MakeGenericType(keyExpr.Type, subqueryElemExpr.Type).GetConstructors()[0],
                        new[]
                            {
                                keyExpr,
                                elementSubquery.SetType(typeof (IEnumerable<>).MakeGenericType(subqueryElemExpr.Type))
                            }
                        );

                resultExpr = Expression.Convert(resultExpr,
                                                typeof (IGrouping<,>).MakeGenericType(keyExpr.Type,
                                                                                      subqueryElemExpr.Type));
            }

            //keyExpr = RebindToSelection.Rebind(projection, keyExpr);
            //pc = ColumnProjector.Evaluate(keyExpr, DynamicCache);

            var pc = ColumnProjector.Evaluate(resultExpr, DynamicCache);

            // make it possible to tie aggregates back to this group-by
            NewExpression newResult = GetNewExpression(resultExpr);
            if (newResult != null && newResult.Type.IsGenericType &&
                newResult.Type.GetGenericTypeDefinition() == typeof (Grouping<,>))
            {
                var projectedElementSubquery = (AliasedExpression) newResult.Arguments[1];
                groupByMap.Add(projectedElementSubquery, info);
                groupByFrom.Add(resultType, projectedElementSubquery);
            }

            return new SelectExpression(resultType, alias, pc, resultExpr, elementSubquery, null, null, new ReadOnlyCollection<Expression>(groupExprs));
        }

        private NewExpression GetNewExpression(Expression expression)
        {
            // ignore converions 
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression) expression).Operand;
            }
            return expression as NewExpression;
        }

        private static Expression BuildPredicateWithNullsEqual(IEnumerable<Expression> source1,
                                                               IEnumerable<Expression> source2)
        {
            IEnumerator<Expression> en1 = source1.GetEnumerator();
            IEnumerator<Expression> en2 = source2.GetEnumerator();
            Expression result = null;
            while (en1.MoveNext() && en2.MoveNext())
            {
                Expression compare =
                    Expression.OrElse(
                        Expression.AndAlso(
                            en1.Current.Equal(new ValueExpression(en1.Current.Type, null)),
                            en2.Current.Equal(new ValueExpression(en2.Current.Type, null))),
//                        new IsNullExpression(en1.Current).And(new IsNullExpression(en2.Current)),
                        en1.Current.Equal(en2.Current)
                        );

                result = (result == null) ? compare : result.AndAlso(compare);
            }
            return result;
        }

        /// <summary>
        /// Visits the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            IRetriever retriever = GetRetriever(m);
            if (retriever != null && retriever.Target != typeof(DateTime) && retriever.Target != typeof(string))
                memberAccess.Push(retriever);

            try
            {
                var exp = Visit(m.Expression);

                var aliasedExpression = exp as AliasedExpression;
                if (aliasedExpression != null)
                    return aliasedExpression.SetType(m.Type);

                if (exp is SqlParameterExpression || exp is ValueExpression)
                    return exp;

                return UpdateMemberAccess(m, exp, m.Member);
            }
            finally
            {
                memberAccess.Clear();
            }
        }

        /// <summary>
        /// This method is used to bind the where expression tree.
        /// </summary>
        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
        {
            ParameterExpression fromIdentifier = selector.Parameters.First();
            AliasedExpression from = VisitSource(source);

            AddFromClauseMapping(fromIdentifier, from);

            var where = (Expression) null;
            Alias alias = Alias.Generate(AliasType.Select);
            Expression selector1 = Visit(selector.Body);
            ReadOnlyCollection<ColumnDeclaration> columns = ColumnProjector.Evaluate(selector1, DynamicCache);
            ProjectionClass projection = ReflectionHelper.GetProjection(resultType.RevealType(), DynamicCache);

            return new SelectExpression(resultType,  projection, alias, columns, selector1, from, where, null, null, null, null, false, false, SelectResultType.Collection, null, null, null);
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            var provider = c.Value as ILinqQueryProvider;
            if (provider != null && Level == 0)
                Level = provider.HierarchyLevel;

            if (provider != null && !(provider.Expression is ConstantExpression))
                return Visit(provider.Expression);

            // That means that the constant expression is the root of an expression, called TableExpression
            if (c.Value is IQueryProvider)
            {
                var projection = ReflectionHelper.GetProjection(c.Type.RevealType(), provider != null ? provider.DynamicCache : null);

                Alias tableAlias = Alias.Generate(AliasType.Table);
                var tableExpression = new TableExpression(c.Type, projection, tableAlias);
                return tableExpression;
            }

            // If no prior member Access occured, it's a real constant value, which has to be treated as a const
            if (memberAccess.Count == 0)
            {
                return new ValueExpression(c.Type, c.Value);
            }

            // Ok. we have to query an variable - that's why we use a Parameter Expression
            if (c.Value != null)
            {
                // Develope the concrete value
                object value = c.Value;
                string parameterName = null;

                IRetriever retriever = null;
                while (memberAccess.Count > 0)
                {
                    retriever = memberAccess.Pop();
                    parameterName = retriever.Source.Name;
                    value = retriever.GetValue(value);
                }

                // A NULL can never be assigned by a parameter, that's why we convert it to a const value
                if (value == null || Backpack.TypeMapper.IsDbNull(value))
                    return new ValueExpression(retriever.SourceType, null);

                // check, what type of parameter we have
                var linqCondition = value as IQueryable;
                var enumerable = value as IEnumerable;
                var valueObject = value as IValueObject;

                if (linqCondition != null)
                {
                    //Type projectedType = value.GetType(); //.GetGenericArguments().First();
                    //var tableExpression = new TableExpression(projectedType, Alias.Generate(AliasType.Table));

                    //return tableExpression;
                    return Visit(linqCondition.Expression);
                }

                // Is it an array?
                if (enumerable != null && value.GetType() != typeof (string))
                {
                    return new ValueExpression(value.GetType(), value);
                }

                // If it's a value object, take the Id
                if (valueObject != null)
                {
                    //var parameterType = typeof (Query<>).MakeGenericType(valueObject.GetType());
                    return new SqlParameterExpression(valueObject.GetType(), valueObject.Id, parameterName);
                }

                // It's a member access ...
                return new SqlParameterExpression(retriever.SourceType, value, parameterName);
            }

            return base.VisitConstant(c);
        }

        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private ProjectionClass GetProjection(Type type)
        {
            return ReflectionHelper.GetProjection(type.RevealType(), DynamicCache);
        }

        /// <summary>
        /// Visits the comparison.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="queryOperator">The query operator.</param>
        /// <returns></returns>
        protected override Expression VisitComparison(BinaryExpression b, QueryOperator queryOperator)
        {
            // Do we have a expression tupel as a child?
            var leftNewExpression = b.Left as NewExpression;
            var rightNewExpression = b.Right as NewExpression;

            bool tupelCondition = leftNewExpression != null && rightNewExpression != null;

            if (tupelCondition)
            {
                // Now - that's an expression list, we have to devide it into single AND Expressions
                int arguments = leftNewExpression.Arguments.Count;
                var binaries = new Stack<Expression>();
                for (int x = 0; x < arguments; x++)
                    binaries.Push(Expression.MakeBinary(b.NodeType, leftNewExpression.Arguments[x],
                                                        rightNewExpression.Arguments[x]));

                Expression result = binaries.Pop();
                while (binaries.Count > 0)
                    result = Expression.AndAlso(result, binaries.Pop());

                return Visit(result);
            }

            return base.VisitComparison(b, queryOperator);
        }

        /// <summary>
        /// Visits the binary.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {

            // Perhaps we have on the left or right side, a single boolean expression
            // If that's the case we implicit have to add a EQUAL Compare
            Expression newLeft = CorrectComparisonWithoutOperator(b.Left);
            Expression newRight = CorrectComparisonWithoutOperator(b.Right);

            if (newLeft != b.Left || newRight != b.Right)
            {
                b = Expression.MakeBinary(b.NodeType, newLeft, newRight);
                return Visit(b);
            }

            // if everything is normal, than continue the binary way
            return base.VisitBinary(b);
        }

        /// <summary>
        /// This method corrects a comparison, if that is necessary
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static Expression CorrectComparisonWithoutOperator(Expression ex)
        {
            var unaryLeft = ex as UnaryExpression;
            var newLeft = ex;
            if ((ex is MemberExpression || (unaryLeft != null && unaryLeft.Operand is MemberExpression)) && ex.Type == typeof(bool))
                newLeft = Expression.MakeBinary(ExpressionType.Equal, ex, Expression.Constant(true));
            return newLeft;
        }

        /// <summary>
        /// Try to find the source of an expression
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        protected override AliasedExpression VisitSource(Expression source)
        {
            Expression exp = Visit(source);

            do
            {
                var unaryExpression = exp as UnaryExpression;
                if (unaryExpression != null)
                    exp = ((UnaryExpression) exp).Operand;
            } while (exp is UnaryExpression);

            //if (exp is MemberExpression)
            //    exp = ColumnProjector.FindAliasedExpression(exp);

            //var selection = exp as SelectExpression;
            //if (selection != null && selection.RevealedType.IsGenericType && selection.RevealedType.GetGenericTypeDefinition() == typeof(IGrouping<,>))
            //{
            //    var nex = GetNewExpression(selection.Selector);
            //    return (AliasedExpression) nex.Arguments[1];
            //}

            var aliased = exp as AliasedExpression;
            if (aliased != null) return aliased;

            var pe = source as ParameterExpression;
            if (pe != null && Backpack.ParameterMapping[pe].Expression is AliasedExpression)
                return Backpack.ParameterMapping[pe].Expression as AliasedExpression;

            var la = exp as LambdaExpression;
            if (la != null && la.Body is AliasedExpression)
                return la.Body as AliasedExpression;

            return new LateBindingExpression(exp);
        }

        #region Nested type: GroupByInfo

        /// <summary>
        /// 
        /// </summary>
        protected class GroupByInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GroupByInfo"/> class.
            /// </summary>
            /// <param name="alias">The alias.</param>
            /// <param name="element">The element.</param>
            internal GroupByInfo(Alias alias, Expression element)
            {
                Alias = alias;
                Element = element;
            }

            /// <summary>
            /// Gets or sets the alias.
            /// </summary>
            /// <value>The alias.</value>
            internal Alias Alias { get; set; }

            /// <summary>
            /// Gets or sets the element.
            /// </summary>
            /// <value>The element.</value>
            internal Expression Element { get; set; }
        }

        #endregion

        /// <summary>
        /// Strips the expression.
        /// </summary>
        /// <param name="bound">The bound.</param>
        /// <returns></returns>
        private Expression StripExpression (Expression bound)
        {
            // If it's a select expression, only take the selector
            var select = bound as SelectExpression;

            // Maybe, it's only one column, than use that
            if (select != null && select.Selector != null && select.Columns.Count == 1)
                return new PropertyExpression(select, select.Columns.First());

            // If it's a lambda expression, only take the body
            var lambda = bound as LambdaExpression;
            bound = lambda != null ? lambda.Body ?? bound : bound;

            return bound;
        }
    }

    #region Nested type: MappingStruct

    /// <summary>
    /// 
    /// </summary>
    public class MappingStruct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingStruct"/> class.
        /// </summary>
        /// <param name="bound">The bound.</param>
        internal MappingStruct(Expression bound)
        {
            Expression = bound;
        }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        internal Expression Expression { get; private set; }
    }

    #endregion

}