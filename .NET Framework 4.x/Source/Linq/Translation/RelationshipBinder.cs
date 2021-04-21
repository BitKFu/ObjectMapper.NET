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
    /// Translates accesses to relationship members into projections or joins
    /// </summary>
    public class RelationshipBinder : DbExpressionVisitor
    {
        private AliasedExpression currentFrom;

        /// <summary> Gets the member access. </summary>
        protected List<IRetriever> MemberAccess { get { return memberAccess; } }
        private readonly List<IRetriever> memberAccess = new List<IRetriever>();

        private readonly Cache<Type, ProjectionClass> dynamicCache;
        protected Cache<Type, ProjectionClass> DynamicCache { get { return dynamicCache; } }

        private RelationshipBinder(Cache<Type, ProjectionClass> cache) 
        {
            dynamicCache = cache;

#if TRACE
            Console.WriteLine("\nRelationshipBinder:");
#endif
        }

        /// Binds all implicit relationships e.g. o.customer.id = xxx
        public static Expression Bind(Expression expression, Cache<Type, ProjectionClass> cache)
        {
            return new RelationshipBinder(cache).Visit(expression);
        }

        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            var left = VisitSource(join.Left);
            var right = VisitSource(join.Right);
            var condition = join.Condition;
            return UpdateJoin(join, join.Join, left, right, condition);
        }

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
                
                if (currentFrom != select.From
                    || where != select.Where
                    || orderBy != select.OrderBy
                    || groupBy != select.GroupBy
                    || take != select.Take
                    || skip != select.Skip
                    || selector != select.Selector
                    )
                {
                    var columns = ColumnProjector.Evaluate(selector ?? currentFrom, DynamicCache).ToList();

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

                    return new SelectExpression(select.Type, select.Alias, new ReadOnlyCollection<ColumnDeclaration>(columns), selector, currentFrom, where, orderBy, groupBy,
                                                skip, take, select.IsDistinct, select.IsReverse, select.SelectResult, select.SqlId, select.Hint);
                }
                return select;
            }
            finally
            {
                currentFrom = saveCurrentFrom;
            }
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var retriever = GetRetriever(m);

            if (retriever != null && retriever.Target != typeof(DateTime) && retriever.Target != typeof(string))
                MemberAccess.Add(retriever);

            return base.VisitMemberAccess(m);
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            Type key = nex.Type;
            if (key == typeof(DateTime))
                return base.VisitNew(nex);

            if (nex.Members == null) 
                return nex;

            for (int x = 0; x < nex.Arguments.Count; x++)
            {
                var aliasedExpression = ColumnProjector.FindAliasedExpression(nex.Arguments[x]);

                // Maybe we have to join a subselect, nested within an argument
                var selectExpression = aliasedExpression as SelectExpression;
                //if (selectExpression != null && selectExpression.Alias != currentFrom.Alias && selectExpression.SelectResult != SelectResultType.SingleAggregate)
                //{
                //    var where = selectExpression.Where;
                //    selectExpression = selectExpression.SetWhere(null);
                //    currentFrom = new JoinExpression(JoinType.LeftOuter, currentFrom, selectExpression, where, nex.Members[x].Name.Substring(4));

                //    continue;
                //}

                // Maybe we have to find a property expression, nested within a member expression
                var propertyExpression = aliasedExpression as PropertyExpression;
                if (propertyExpression != null && nex.Arguments[x].Type.IsValueObjectType())
                {
                    var linkTarget = nex.Arguments[x].Type;
                    var linkProjection = ReflectionHelper.GetProjection(linkTarget, dynamicCache);
                    var primaryKey = linkProjection.GetPrimaryKeyDescription();
                    var foundColumn = TableExpressionFinder.FindForType(currentFrom, linkTarget, primaryKey.Name);

                    // No fitting column could be found. In that case an assosiative join is missing
                    if (foundColumn == null)
                    {
                        var tableExpression = new TableExpression(linkTarget, Alias.Generate(AliasType.Table), dynamicCache.Get(linkTarget));
                        var columns = tableExpression.Columns;
                         selectExpression = new SelectExpression(linkTarget, Alias.Generate(AliasType.Select), columns,
                                                                null, tableExpression, null);

                        var propertyInfo = propertyExpression.ParentType.GetPropertyInfo(propertyExpression.PropertyName);
                        if (propertyInfo != null)
                        {
                            var field = ReflectionHelper.GetStaticFieldTemplate(propertyInfo);
                            var where = Expression.MakeBinary(ExpressionType.Equal,
                                                              new PropertyExpression(linkTarget, currentFrom.Alias, field),
                                                              new PropertyExpression(linkTarget, selectExpression.Alias,
                                                                                     primaryKey));

                            currentFrom = new JoinExpression(nex.Type, JoinType.InnerJoin, currentFrom, selectExpression, where);
                        }
                    }

                    continue;
                }
            }

            return nex;
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <param name="aliasedExpression">The aliased expression.</param>
        /// <returns></returns>
        protected Dictionary<string, ColumnDeclaration> GetColumns(AliasedExpression aliasedExpression)
        {
            return ColumnGatherer.Gather(aliasedExpression, 1);    
        }

        protected override Expression  VisitColumn(PropertyExpression expression)
        {
            if (MemberAccess.Count == 0)
                return expression;

            // If a direct member will be accessed, we have to check, if the member is of a value object type
            // That's the case when querying e.g. c.Order where Order is an object of the class Order
            if (MemberAccess.Count == 1)
            {
                var retriever = MemberAccess[0];
                if (retriever.SourceType.IsValueObjectType())
                {
                    var linkTarget = retriever.SourceType;
                    var foundColumn = TableExpressionFinder.FindForType(currentFrom, linkTarget, expression.Name);

                    // No fitting column could be found. In that case an assosiative join is missing
                    if (foundColumn == null)
                    {
                        var tableExpression = new TableExpression(linkTarget, Alias.Generate(AliasType.Table), dynamicCache.Get(linkTarget));
                        var columns = tableExpression.Columns;
                        var selectExpression = new SelectExpression(linkTarget, Alias.Generate(AliasType.Select), columns,
                                                                    null, tableExpression, null);

                        var sourceProjection = ReflectionHelper.GetProjection(retriever.SourceType, dynamicCache);

                        var join1 = ReflectionHelper.GetStaticFieldTemplate(retriever.Target, retriever.Source.Name);
                        var join2 = sourceProjection.GetPrimaryKeyDescription();

                        var where = Expression.MakeBinary(ExpressionType.Equal,
                                                          new PropertyExpression(linkTarget, currentFrom.Alias, join1),
                                                          new PropertyExpression(linkTarget, selectExpression.Alias, join2));

                        currentFrom = new JoinExpression(typeof(void), JoinType.InnerJoin, currentFrom, selectExpression, where);
                    }
                }

                MemberAccess.Clear();
                return expression;
            }

            // Indirect Members must be joined together
            // That's the case when querying e.g. c.Order.Id where the property id, is an member of the type Order
            for (int counter = 0; counter <= MemberAccess.Count - 2; counter++)
            {
                var member = MemberAccess[counter];
                var joinMember = MemberAccess[counter + 1];

                // Do we have a type mismatch, this indicates, that a nested member is accessed and we need to rebind or add new join
                if (!member.Target.UnpackType().All(type => expression.Type.UnpackType().Contains(type)))
                {
                    var linkTarget = member.Target.UnpackType().First();
                    var foundColumn = TableExpressionFinder.FindForType(currentFrom, linkTarget, expression.Name);

                    // No fitting column could be found. In that case an assosiative join is missing
                    if (foundColumn == null)
                    {
                        var tableExpression = new TableExpression(linkTarget, Alias.Generate(AliasType.Table), dynamicCache.Get(linkTarget));
                        var columns = tableExpression.Columns;
                        var selectExpression = new SelectExpression(linkTarget, Alias.Generate(AliasType.Select), columns,
                                                                    null, tableExpression, null);

                        var propertyInfo = joinMember.Source as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            var targetProjection = ReflectionHelper.GetProjection(linkTarget, dynamicCache);
                            var primaryKey = targetProjection.GetPrimaryKeyDescription();
                            var field = ReflectionHelper.GetStaticFieldTemplate(propertyInfo);
                            var where = Expression.MakeBinary(ExpressionType.Equal,
                                                              //new PropertyExpression(linkTarget, currentFrom.Alias, field),
                                                              //new PropertyExpression(linkTarget, selectExpression.Alias,primaryKey));
                                                              new PropertyExpression(linkTarget, currentFrom.Alias, GetColumns(currentFrom)[field.Name]),
                                                              new PropertyExpression(linkTarget, selectExpression.Alias, GetColumns(selectExpression)[primaryKey.Name]));
//                                                                  ))

                            currentFrom = new JoinExpression(typeof(void), JoinType.InnerJoin, currentFrom, selectExpression, where);
                        }

                        MemberAccess.Clear();
                        return RebindToSelection.Rebind(selectExpression, expression);
                    }

                    MemberAccess.Clear();
                    return RebindToSelection.Rebind(foundColumn.Mapping, expression);
                }
            }

            MemberAccess.Clear();
            return expression;
        }
    }
}