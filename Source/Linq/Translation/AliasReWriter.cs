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
    /// This Re Writer is used to solve alias expressions
    /// </summary>
    public class AliasReWriter : DbPackedExpressionVisitor
    {
        private readonly Dictionary<AliasType, int> globalAliasCounter = new Dictionary<AliasType, int>();
        private readonly Dictionary<string, string> globalColumnAliasReference = new Dictionary<string, string>();

        private Dictionary<string, string> inJoinCondition = new Dictionary<string, string>();

        private AliasReWriter(ExpressionVisitorBackpack backpack) 
            :base(backpack)
        {
#if TRACE
            Console.WriteLine("\nAliasReWriter:");
#endif
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        public static Expression Rewrite(Expression expression, ExpressionVisitorBackpack backpack)
        {
            var writer = new AliasReWriter(backpack);
            return writer.Visit(expression);
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitTableExpression(TableExpression expression)
        {
            var result = base.VisitTableExpression(expression);

            if (expression.Alias.Generated)
            {
                int counter;
                if (globalAliasCounter.TryGetValue(AliasType.Table, out counter))
                    globalAliasCounter.Remove(AliasType.Table);

                counter++;
                expression.Alias.Name = "T" + counter;
                globalAliasCounter.Add(AliasType.Table, counter);
            }

            return result;
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            // Solve alias for global selection
            if (union.Alias.Generated)
            {
                int counter;
                if (globalAliasCounter.TryGetValue(AliasType.Union, out counter))
                    globalAliasCounter.Remove(AliasType.Union);

                counter++;
                union.Alias.Name = "U" + counter;
                globalAliasCounter.Add(AliasType.Union, counter);
            }

            return base.VisitUnionExpression(union);
        }

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            var result = base.VisitSelectExpression(select) as SelectExpression;
            //var columns = MemberBinder.GetColumns(result.From, result.Columns, result.Selector, dynamicCache);

            // Solve aliases for columns
            SolveColumns(select.Columns);

            // Solve alias for global selection
            if (select.Alias.Generated)
            {
                int counter;
                if (globalAliasCounter.TryGetValue(AliasType.Select, out counter))
                    globalAliasCounter.Remove(AliasType.Select);

                counter++;
                select.Alias.Name = "S" + counter;
                globalAliasCounter.Add(AliasType.Select, counter);
            }

            return result;
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            // Try to find out, if it's a reference
            string referencedAlias;
            if (globalColumnAliasReference.TryGetValue(expression.Name, out referencedAlias))
                expression.Name = referencedAlias;

            return base.VisitColumn(expression);
        }

        /// <summary>
        /// Solves the columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        private void SolveColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            var aliasCounter = new Dictionary<string, int>() { { "C", 0 } };

            int counter;
            foreach (var col in columns)
            {
                string prefix = "C";

                // if it's not generated take this always as the base name
                if (!col.Alias.Generated)
                {
                    prefix = col.Alias.Name;

                    // perhaps, we have to replace it with the join condition name
                    if (inJoinCondition.ContainsKey(prefix))
                        prefix = inJoinCondition[prefix];
                }
                else
                {
                    // if the column is a property expression, than take the property name as the base name
                    var prop = col.Expression as PropertyExpression;
                    if (prop != null)
                        prefix = prop.Name;
                }

                // try to read the current sql alias prefix counter
                if (aliasCounter.TryGetValue(prefix, out counter))
                {
                    aliasCounter.Remove(prefix);
                    counter++;
                }

                string newAlias = string.Concat(prefix, (counter > 0) ? counter.ToString() : string.Empty);
                if (col.Alias.Generated)
                    globalColumnAliasReference.Add(col.Alias.Name, newAlias);

                col.Alias.Name = newAlias;
                aliasCounter.Add(prefix, counter);
            }
        }

        /// <summary>
        /// Visits the scalar expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitScalarExpression(ScalarExpression expression)
        {
            var result = base.VisitScalarExpression(expression);

            // Solve aliases for columns
            SolveColumns(expression.Columns);

            return result;
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            if (join.Alias.Generated)
            {
                var pc = ReflectionHelper.GetProjection(join.RevealedType, Backpack.ProjectionCache);
                var newExpression = pc.NewExpression;

                string newAlias = null;
                if (newExpression != null && newExpression.Members.Count == 2)
                {
                    newAlias = newExpression.GetParameterName(1);
                }

                string oldAlias = join.Alias.Name;

                int counter;
                if (globalAliasCounter.TryGetValue(AliasType.Join, out counter))
                    globalAliasCounter.Remove(AliasType.Join);

                counter++;
                join.Alias.Name = newAlias ?? "J" + counter;
                globalAliasCounter.Add(AliasType.Join, counter);

                inJoinCondition.Add(oldAlias, join.Alias.Name);
            }

            return base.VisitJoinExpression(join);
        }
    }
}