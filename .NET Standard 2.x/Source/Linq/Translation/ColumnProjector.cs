using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// The column Projector visits all expressions and creates column declarations out of it
    /// </summary>
    public class ColumnProjector : DbExpressionVisitor
    {
        struct JoinStruct
        {
            public string name;
            public JoinType joinType;
        }

        private readonly Collection<ColumnDeclaration> columns = new Collection<ColumnDeclaration>();

        private int Level { get; set; }

        private readonly Cache<Type, ProjectionClass> dynamicCache;
        private Stack<JoinStruct> inJoinCondition = new Stack<JoinStruct>();

        /// <summary>
        /// Gets the dynamic cache.
        /// </summary>
        /// <value>The dynamic cache.</value>
        protected Cache<Type, ProjectionClass> DynamicCache { get { return dynamicCache; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnProjector"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        private ColumnProjector(Cache<Type, ProjectionClass> cache)
        {
            Level = 1;
            dynamicCache = cache;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            try
            {
                Level++;
                return base.Visit(exp);
            }
            finally
            {
                Level--;
            }
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        public static ReadOnlyCollection<ColumnDeclaration> Evaluate(Expression expression, Cache<Type, ProjectionClass> cache)
        {
            var projector = new ColumnProjector(cache);

            projector.Visit(expression);
            return new ReadOnlyCollection<ColumnDeclaration>(projector.columns);
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="projection">The projection.</param>
        /// <returns></returns>
        public static ReadOnlyCollection<ColumnDeclaration> Evaluate(Expression expression, ProjectionClass projection)
        {
            var cache = new Cache<Type, ProjectionClass>("ColumnProjector");
            if (projection != null)
                cache.Insert(projection.ProjectedType, projection);

            var projector = new ColumnProjector(cache);

            projector.Visit(expression);
            return new ReadOnlyCollection<ColumnDeclaration>(projector.columns);
        }

        /// <summary>
        /// Visits the aggregate subquery.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <returns></returns>
        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate)
        {
            columns.Add(new ColumnDeclaration(aggregate.AggregateAsSubquery, Alias.Generate(AliasType.Column)));
            return aggregate;
        }

        /// <summary>
        /// Visits the aggregate expression
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            columns.Add(new ColumnDeclaration(aggregate, Alias.Generate(AliasType.Column)));
            return aggregate;
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            //var asSelect = union.First as SelectExpression ?? union.Second as SelectExpression;
            //if (asSelect == null)
            //    return base.VisitUnionExpression(union);

            //// Only visit the first expression, because the columns are the same in the second expression.
            //AddSelectColumns(union, asSelect.Columns);

            AddSelectColumns(union, union.Columns);
            return union;
        }

        /// <summary>
        /// Visits the binary.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (inJoinCondition.Count == 0)
                columns.Add(new ColumnDeclaration(b, Alias.Generate(AliasType.Column)));
            return b;
        }

        /// <summary>
        /// Visits the parameter expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSqlParameterExpression(SqlParameterExpression expression)
        {
            columns.Add(new ColumnDeclaration(expression, Alias.Generate(AliasType.Column)));
            return expression;
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            columns.Add(new ColumnDeclaration(expression, Alias.Generate(expression.Name))); //.Generate(AliasType.Column)));
            return expression;
        }

        /// <summary>
        /// Visits the value expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitValueExpression(ValueExpression expression)
        {
            if (Level < 2)     // Level 3 is the first and only top-level where a value expression can be a result column
                return expression;

            columns.Add(new ColumnDeclaration(expression, Alias.Generate(AliasType.Column)));
            return expression;
        }

        /// <summary>
        /// Visits the conditional.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression c)
        {
            columns.Add(new ColumnDeclaration(c, Alias.Generate(AliasType.Column)));
            return c;
        }

        /// <summary>
        /// A Comparison is the result of a selection
        /// </summary>
        protected override Expression VisitComparison(BinaryExpression expression, Queries.QueryOperator queryOperator)
        {
            ConditionalExpression condition = CreateConditionalExpression(expression);

            if (inJoinCondition.Count > 0)
            {
                JoinStruct js = inJoinCondition.Peek();

                // only add additional columns for outer apply and left outer joins
                if (js.joinType == JoinType.LeftOuter || js.joinType == JoinType.OuterApply)
                    columns.Add(new ColumnDeclaration(condition, Alias.Generate(js.name)));
            }
            else
                columns.Add(new ColumnDeclaration(condition, Alias.Generate(AliasType.Column)));

            return expression;
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            var left = VisitSource(join.Left);
            var right = VisitSource(join.Right);
            var condition = VisitJoinCondition(join.Condition, join.Join, join.Alias.Name);
            return UpdateJoin(join, join.Join, left, right, condition);
        }

        /// <summary>
        /// Visits the join condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="joinType">Type of the join.</param>
        /// <param name="joinName">Name of the join.</param>
        /// <returns></returns>
        private Expression VisitJoinCondition(Expression expression, JoinType joinType, string joinName)
        {
            inJoinCondition.Push(new JoinStruct() { joinType = joinType, name = joinName });
            try
            {
                return Visit(expression);
            }
            finally
            {
                inJoinCondition.Pop();
            }
        }

        /// <summary>
        /// Creates the conditional expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private ConditionalExpression CreateConditionalExpression(BinaryExpression expression)
        {
            return expression != null
                ? Expression.Condition(expression, new ValueExpression(typeof(bool), true), new ValueExpression(typeof(bool), false))
                : null;
        }

        /// <summary>
        /// Adds from expresion.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        private bool AddFromExpresion(Expression expression, string columnName)
        {
            // Maybe we have to find a property expression, nested within a member expression
            var aliasedExpression = FindAliasedExpression(expression);
            var propertyExpression = aliasedExpression as PropertyExpression;

            if (propertyExpression != null)
            {
                if (expression.Type.IsValueObjectType())
                    foreach (
                        var col in
                            ReflectionHelper.GetProjection(expression.Type, DynamicCache).GetColumns(
                                propertyExpression.Alias, DynamicCache))
                        columns.Add(col);
                else
                    columns.Add(new ColumnDeclaration(expression, Alias.Generate(columnName)));

                return true;
            }

            // Perhaps it's a select expression
            var selectExpression = aliasedExpression as SelectExpression;
            if (selectExpression != null)
            {
                if (selectExpression.SelectResult == SelectResultType.SingleAggregate
                    || selectExpression.SelectResult == SelectResultType.SingleObject
                    || selectExpression.SelectResult == SelectResultType.SingleObjectOrDefault)
                {
                    columns.Add(new ColumnDeclaration(selectExpression.SetAlias(string.Empty),
                                                      Alias.Generate(columnName)));
                    return true;
                }

                if (selectExpression.SelectResult == SelectResultType.Collection)
                {
                    Visit(expression);
                    return true;
                }
            }

            // Perhaps it's a union expression
            var unionExpression = expression as UnionExpression;
            if (unionExpression != null)
            {
                Visit(unionExpression);
                return true;
            }

            // Perhaps it's a join expression
            var joinExpression = expression as JoinExpression;
            if (joinExpression != null)
            {
                Visit(joinExpression);
                return true;
            }

            // Perhaps it's a table expression
            var tableExpression = expression as TableExpression;
            if (tableExpression != null)
            {
                columns.AddRange(tableExpression.Columns);
                return true;
            }

            // Otherwise add the column
            columns.Add(new ColumnDeclaration(expression, Alias.Generate(columnName)));
            return true;
        }

        /// <summary>
        /// Visits the new expression for anonymous types
        /// </summary>
        /// <param name="nex">The nex.</param>
        /// <returns></returns>
        protected override NewExpression VisitNew(NewExpression nex)
        {
            Type key = nex.Type;
            if (key == typeof(DateTime))
                return base.VisitNew(nex);

            // May be it's a grouping
            if (nex.Type.IsGenericType && nex.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                Expression expression = nex.Arguments[0];
                string columnName = nex.GetParameterName(0);

                if (AddFromExpresion(expression, columnName))        // Only add the key column and return
                    return nex;
            }

            // Solve the columns
            for (int x = 0; x < nex.Arguments.Count; x++)
            {
                Expression expression = nex.Arguments[x];
                string columnName = nex.GetParameterName(x);

                if (AddFromExpresion(expression, columnName))
                    continue;

                // Otherwise visit deeper
                Visit(expression);
            }

            return nex;
        }

        /// <summary>
        /// Find additional bindings
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected override IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            foreach (var binding in original)
            {
                string columnName = binding.Member.Name;
                Expression expression;

                if (binding.BindingType == MemberBindingType.Assignment)
                    expression = ((MemberAssignment)binding).Expression;
                else
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));

                if (AddFromExpresion(expression, columnName))
                    continue;

                VisitBinding(binding);
            }
            return original;
        }

        /// <summary>
        /// Find nested property expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static AliasedExpression FindAliasedExpression(Expression expression)
        {
            expression = ExpressionTypeFinder.Find(expression, typeof(AliasedExpression));
            return expression as AliasedExpression;
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitTableExpression(TableExpression expression)
        {
            if (expression.Columns != null) // True, if the table expression is already initialized
            {
                columns.AddRange(expression.Columns);
            }
            else
            {
                foreach (var col in expression.Projection.GetColumns(expression.Alias, DynamicCache))
                    columns.Add(col);
            }

            return expression;
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            AddSelectColumns(select, select.Columns);

            if (select.DefaultIfEmpty != null)
            {
                var lambda = (LambdaExpression) select.DefaultIfEmpty;

                inJoinCondition.Push(new JoinStruct(){joinType = JoinType.LeftOuter, name = lambda.Parameters[0].Name});
                Visit(select.DefaultIfEmpty);
                inJoinCondition.Pop();
            }

            return select;
        }

        /// <summary>
        /// Visits the scalar expression.
        /// </summary>
        /// <param name="scalar">The scalar.</param>
        /// <returns></returns>
        protected override Expression VisitScalarExpression(ScalarExpression scalar)
        {
            AddSelectColumns(scalar, scalar.Columns);
            return scalar;
        }

        /// <summary>
        /// Adds the select columns.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <param name="selection">The columns.</param>
        private void AddSelectColumns(AliasedExpression select, ReadOnlyCollection<ColumnDeclaration> selection)
        {
            foreach (var col in selection)
            {
                if (col.Expression is AggregateExpression)
                    columns.Add(col);
                else
                    columns.Add(new ColumnDeclaration(new PropertyExpression(select, col), col));
            }
        }

    }
}