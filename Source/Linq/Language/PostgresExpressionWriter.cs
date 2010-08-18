using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Language
{
    /// <summary>
    /// This class is used to generate SQL Statements out of the expression tree
    /// </summary>
    public class PostgresExpressionWriter : LinqMethodInspector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlExpressionWriter"/> class.
        /// </summary>
        public PostgresExpressionWriter(ILinqPersister nativePersister, List<PropertyTupel> groupings, Cache<Type, ProjectionClass> cache)
            : base(nativePersister, groupings, cache)
        {
            Command = nativePersister.CreateCommand();
        }

        readonly Stack<AliasedExpression> selectStack = new Stack<AliasedExpression>();

        /// <summary>
        /// Visits the select expression.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (selectStack.Count>0)
            {
                WriteSql("(");
            }

            selectStack.Push(select);
            try
            {
                WriteSql("SELECT ");

                if (!string.IsNullOrEmpty(select.SqlId))
                    WriteSql("/* " + select.SqlId + " */ ");

                if (!string.IsNullOrEmpty(select.Hint))
                    WriteSql("/*+ " + select.Hint + " */ ");

                if (select.IsDistinct)
                    WriteSql("DISTINCT ");

                if (select.Columns != null)
                {
                    for (var x = 0; x < select.Columns.Count; x++)
                    {
                        if (x > 0) WriteSql(", ");

                        var col = select.Columns[x];
                        var prop = col.Expression as PropertyExpression;
                        Visit(select.Columns[x].Expression);

                        if (prop == null || !string.Equals(prop.Name, col.Alias.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            WriteSql(" as ");
                            WriteSql(TypeMapper.Quote(col.Alias.Name));
                        }
                    }
                }

                if (select.Columns == null || select.Columns.Count == 0)
                    WriteSql("NULL");

                if (select.From != null) WriteSql(" FROM ");

                var subSelect = Visit(select.From);
                var aliasedFrom = subSelect as IDbExpressionWithResult;
                if (aliasedFrom != null)
                    WriteSql(aliasedFrom.Alias.Name);

                if (select.Where != null)
                {
                    WriteSql(" WHERE ");
                    Visit(select.Where);
                }

                if (select.GroupBy != null)
                {
                    WriteSql(" GROUP BY ");
                    for (var x = 0; x < select.GroupBy.Count; x++)
                    {
                        if (x > 0) WriteSql(", ");
                        Visit(select.GroupBy[x]);
                    }
                }

                if (select.OrderBy != null)
                {
                    WriteSql(" ORDER BY ");
                    for (var x = 0; x < select.OrderBy.Count; x++)
                    {
                        if (x > 0) WriteSql(", ");
                        Visit(select.OrderBy[x].Expression);
                        WriteSql(" ");
                        Builder.Append(select.OrderBy[x].Ordering);
                    }
                }

                if (select.Skip != null)
                {
                    WriteSql(" OFFSET ");
                    Visit(select.Skip);
                    WriteSql(" ");
                }

                if (select.Take != null)
                {
                    WriteSql(" LIMIT ");
                    Visit(select.Take);
                    WriteSql(" ");
                }

                return select;
            }
            finally
            {
                selectStack.Pop();

                if (selectStack.Count > 0)
                {
                    WriteSql(")");
                }
            }
        }

        protected override Expression VisitScalarExpression(ScalarExpression select)
        {
            if (selectStack.Count > 0)
            {
                WriteSql("(");
            }

            selectStack.Push(select);
            try
            {
                return base.VisitScalarExpression(select);
            }
            finally
            {
                selectStack.Pop();

                if (selectStack.Count > 0)
                {
                    WriteSql(")");
                }
            }
        }

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        protected override string Concatinator
        {
            get
            {
                return "||";
            }
        }

        /// <summary>
        /// Return the length of a string
        /// </summary>
        /// <param name="expression"></param>
        protected override void Length(Expression expression)
        {
            WriteSql(" LENGTH(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Visits the table expression.
        /// </summary>
        protected override Expression VisitTableExpression(TableExpression expression)
        {
            ProjectionClass table = ReflectionHelper.GetProjection(expression.RevealedType, DynamicCache);

            string schema = string.IsNullOrEmpty(LinqPersister.DatabaseSchema)
                                ? string.Empty
                                : string.Concat(LinqPersister.DatabaseSchema, ".");

            WriteSql(string.Concat(schema, TypeMapper.Quote(table.TableName(DatabaseType.Postgres)), " "));
            return expression;
        }

        /// <summary>
        /// News the date time.
        /// </summary>
        protected override void NewDateTime(Expression year, Expression month, Expression day)
        {
            WriteSql(" TO_DATE(");
            var time = new[] { year, month, day };
            AddDateTimeExpressions(time);
            WriteSql(", 'YYYY/MM/DD')");
        }

        /// <summary>
        /// News the date time.
        /// </summary>
        protected override void NewDateTime(Expression year, Expression month, Expression day, Expression hour,
                                        Expression minute, Expression second)
        {
            WriteSql(" TO_TIMESTAMP(");
            var time = new[] { year, month, day, hour, minute, second };
            AddDateTimeExpressions(time);
            WriteSql(", 'YYYY/MM/DD HH-MI-SS')");
        }

        /// <summary>
        /// Adds the date time expressions.
        /// </summary>
        /// <param name="time">The time.</param>
        protected override void AddDateTimeExpressions(Expression[] time)
        {
            char separator = time.Length == 3 ? '/' : '-';

            string constString = string.Empty;
            for (int x = 0; x < time.Length; x++)
            {
                if (x == 3) separator = ' ';
                if (x == 4) separator = ':';

                var constTime = time[x] as ValueExpression;
                if (constTime != null)
                {
                    if (x > 0) constString += separator;
                    constString += constTime.Value.ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(constString)) WriteSql("'" + constString + "'");
                    if (x > 0) WriteSql(Concatinator + " '" + separator + "' " + Concatinator);

                    WriteSql(" CAST(");
                    Visit(time[x]);
                    WriteSql(" as VARCHAR(32)) ");
                    constString = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(constString))
            {
                if (time.Any(x => !(x is ValueExpression)))
                    WriteSql(Concatinator + " ");

                WriteSql("'" + constString + "'");
            }
        }

        /// <summary>
        /// Returns the Year of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Year(Expression expression)
        {
            WriteSql(" EXTRACT(YEAR FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Month of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Month(Expression expression)
        {
            WriteSql(" EXTRACT(MONTH FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Day of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Day(Expression expression)
        {
            WriteSql(" EXTRACT(DAY FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Second of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Second(Expression expression)
        {
            WriteSql(" EXTRACT(SECONDS FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Minute of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Minute(Expression expression)
        {
            WriteSql(" EXTRACT(MINUTES FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Hour of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void Hour(Expression expression)
        {
            WriteSql(" EXTRACT(HOURS FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Weekday of the given expression
        /// </summary>
        /// <param name="expression"></param>
        protected override void DayOfWeek(Expression expression)
        {
            WriteSql(" EXTRACT(DOW FROM ");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Years to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddYears(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' year' AS INTERVAL)");
        }

        /// <summary>
        /// Add an amount of Months to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddMonths(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' months' AS INTERVAL)");
        }

        /// <summary>
        /// Add an amount of Days to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddDays(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' day' AS INTERVAL)");
        }

        /// <summary>
        /// Add an amount of Hours to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddHours(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' hours' AS INTERVAL)");
        }

        /// <summary>
        /// Add an amount of Minutes to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddMinutes(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' minutes' AS INTERVAL)");
        }

        /// <summary>
        /// Add an amount of Seconds to the given "addTo" Expression.
        /// </summary>
        /// <param name="addToExp"></param>
        /// <param name="addValueExp"></param>
        protected override void AddSeconds(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + CAST (CAST(");
            Visit(addValueExp);
            WriteSql(" AS CHARACTER VARYING) || ' seconds' AS INTERVAL)");
        }

        /// <summary>
        /// Does the string convert.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void ToStringConvert(Expression expression)
        {
            WriteSql(" CAST (");
            Visit(expression);
            WriteSql("AS CHARACTER VARYING)");
        }

        /// <summary>
        /// Visits the sys date expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysDateExpression(SysDateExpression expression)
        {
            WriteSql("CURRENT_DATE");
            return expression;
        }

        /// <summary>
        /// Visits the sys time expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysTimeExpression(SysTimeExpression expression)
        {
            WriteSql("CURRENT_TIME");
            return expression;
        }

        /// <summary>
        /// Exclusives the or.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void BitwiseExclusiveOr(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" # ");
            Visit(right);
        }

        protected override void IndexOf(Expression expression, Expression indexOf, Expression startFrom)
        {
            WriteSql(" STRPOS(");
            if (startFrom == null)
                Visit(expression);
            else
            {
                WriteSql("SUBSTR(");
                Visit(expression);
                WriteSql(", ");
                Visit(startFrom);
                WriteSql("-1");
                WriteSql(")");
            }
            WriteSql(", ");
            Visit(indexOf);
            WriteSql(")");

            if (startFrom != null)
            {
                WriteSql("+");
                Visit(startFrom);
            }
            else
                WriteSql("-1");

        }

        /// <summary>
        /// Determines whether [is null or empty] [the specified visitor].
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void IsNullOrEmpty(Expression expression)
        {
            WriteSql("(");
            Visit(expression);
            WriteSql(" IS NULL OR LENGTH(");
            Visit(expression);
            WriteSql(")=0)");
        }

        /// <summary>
        /// Truncates the specified
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void Truncate(Expression expression)
        {
            WriteSql(" TRUNC(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Calculating the natural logarithm
        /// </summary>
        /// <param name="expression"></param>
        protected override void Log(Expression expression)
        {
            WriteSql(" LN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSqlParameterExpression(SqlParameterExpression expression)
        {
            if (expression.ContentType.IsEnum)
            {
                WriteSql("CAST(");
                base.VisitSqlParameterExpression(expression);
                WriteSql(" AS ");
                WriteSql(TypeMapper.Quote(expression.ContentType.Name));
                WriteSql(")");
            }
            else
                base.VisitSqlParameterExpression(expression);

            return expression;
        }
    }
}
