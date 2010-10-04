using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Language
{
    /// <summary>
    /// The Oracle Expression Writer transforms the Linq Tree to the SQL Expression
    /// </summary>
    public class OracleExpressionWriter : LinqMethodInspector
    {
        /// <summary> Returns the oracle Concatinator string  </summary>
        protected override string Concatinator { get { return "||"; } }

        /// <summary> Map to solve cast recursions </summary>
        private HashSet<Expression> recursion = new HashSet<Expression>();

        /// <summary> Used to output subselects and in order to define if they are using parenthesis or not </summary>
        readonly Stack<AliasedExpression> selectStack = new Stack<AliasedExpression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlExpressionWriter"/> class.
        /// </summary>
        public OracleExpressionWriter(ILinqPersister nativePersister, List<PropertyTupel> groupings, ExpressionVisitorBackpack backpack)
            : base(nativePersister, groupings, backpack)
        {
            Command = nativePersister.CreateCommand();
        }

        /// <summary>
        /// Writes the corresponding table name
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitTableExpression(TableExpression expression)
        {
            var table = expression.Projection;
            WriteSql(string.Concat(LinqPersister.DatabaseSchema, ".", table.TableName(DatabaseType.Oracle)));
            return expression;
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

                if (select.Take != null)
                {
                    WriteSql("TOP ");
                    Visit(select.Take);
                    WriteSql(" ");
                }

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

                WriteSql(" FROM ");
                if (select.From == null) WriteSql("DUAL");

                var subSelect = Visit(select.From);
                var aliasedFrom = subSelect as IDbExpressionWithResult;
                if (aliasedFrom != null)
                    WriteSql(" " + aliasedFrom.Alias.Name);

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

        #region DateTime Methods

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
            WriteSql(" TO_DATE(");
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
        protected override void Year(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'YYYY')");
        }

        /// <summary>
        /// Returns the Month of the given expression
        /// </summary>
        protected override void Month(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'MM')");
        }

        /// <summary>
        /// Returns the Day of the given expression
        /// </summary>
        protected override void Day(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'DD')");
        }

        /// <summary>
        /// Returns the Hour of the given expression
        /// </summary>
        protected override void Hour(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'HH')");
        }

        /// <summary>
        /// Returns the Minute of the given expression
        /// </summary>
        protected override void Minute(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'MI')");
        }

        /// <summary>
        /// Returns the Second of the given expression
        /// </summary>
        protected override void Second(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'SS')");
        }

        /// <summary>
        /// Returns the Weekday of the given expression
        /// </summary>
        protected override void DayOfWeek(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(", 'D')");
        }

        /// <summary>
        /// Add an amount of Years to the given "addTo" Expression.
        /// </summary>
        protected override void AddYears(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + NUMTOYMINTERVAL(");
            Visit(addValueExp);
            WriteSql(", 'year')");
        }

        /// <summary>
        /// Add an amount of Months to the given "addTo" Expression.
        /// </summary>
        protected override void AddMonths(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" ADD_MONTHS(");
            Visit(addToExp);
            WriteSql(",");
            Visit(addValueExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Days to the given "addTo" Expression.
        /// </summary>
        protected override void AddDays(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + NUMTODSINTERVAL(");
            Visit(addValueExp);
            WriteSql(", 'DAY')");
        }

        /// <summary>
        /// Add an amount of Hours to the given "addTo" Expression.
        /// </summary>
        protected override void AddHours(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + NUMTODSINTERVAL(");
            Visit(addValueExp);
            WriteSql(", 'HOUR')");
        }

        /// <summary>
        /// Add an amount of Minutes to the given "addTo" Expression.
        /// </summary>
        protected override void AddMinutes(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + NUMTODSINTERVAL(");
            Visit(addValueExp);
            WriteSql(", 'MINUTE')");
        }

        /// <summary>
        /// Add an amount of Seconds to the given "addTo" Expression.
        /// </summary>
        protected override void AddSeconds(Expression addToExp, Expression addValueExp)
        {
            Visit(addToExp);
            WriteSql(" + NUMTODSINTERVAL(");
            Visit(addValueExp);
            WriteSql(", 'SECOND')");
        }

        #endregion

        protected override void Truncate(Expression expression)
        {
            WriteSql(" TRUNC(");
            Visit(expression);
            WriteSql(")");
        }

        #region Bit Operators

        /// <summary>
        /// Ors the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void BitwiseOr(Expression left, Expression right)
        {
            WriteSql(" (");
            Visit(left);
            WriteSql(" - ");
            BitwiseAnd(left, right);
            WriteSql(" + ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Ands the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void BitwiseAnd(Expression left, Expression right)
        {
            WriteSql(" BITAND(");
            Visit(left);
            WriteSql(", ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Exclusives the or.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void BitwiseExclusiveOr(Expression left, Expression right)
        {
            WriteSql(" (");
            BitwiseOr(left,right);
            WriteSql(" - ");
            BitwiseAnd(left, right);
            WriteSql(")");
        }

        /// <summary>
        /// Nots the specified 
        /// </summary>
        /// <param name="operand">The operand.</param>
        protected override void BitwiseNot(Expression operand)
        {
            WriteSql(" (0 - ");
            Visit(operand);
            WriteSql(")-1 ");
        }

        #endregion

        protected override void Modulo(Expression left, Expression right)
        {
            WriteSql(" MOD(");
            Visit(left);
            WriteSql(", ");
            Visit(right);
            WriteSql(")");
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
        /// Returns a substring of a given string
        /// </summary>
        protected override void Substring(Expression expression, Expression from, Expression count)
        {
            WriteSql(" SUBSTR(");
            Visit(expression);
            WriteSql(", ");
            Visit(from);
            WriteSql("+1");
            if (count != null)
            {
                WriteSql(", ");
                Visit(count);
            }
            WriteSql(")");
        }

        /// <summary>
        /// Is NULL OR EMPTY TEST
        /// </summary>
        protected override void IsNullOrEmpty(Expression expression)
        {
            WriteSql("(");
            Visit(expression);
            WriteSql(" IS NULL OR LENGTH(");
            Visit(expression);
            WriteSql(")=0)");
        }

        /// <summary>
        /// Returns the index of a special character
        /// </summary>
        protected override void IndexOf(Expression expression, Expression indexOf, Expression startFrom)
        {
            WriteSql(" INSTR(");
            Visit(expression);
            WriteSql(", ");
            Visit(indexOf);

            if (startFrom != null)
            {
                WriteSql(", ");
                Visit(startFrom);
                WriteSql("+1");
            }

            WriteSql(")-1");
        }

        /// <summary>
        /// Writes the aggregate expression
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            var argument = aggregate.Argument ?? new ValueExpression(typeof (string), "*");

            var mce = ExpressionTypeFinder.Find(argument, ExpressionType.Call) as MethodCallExpression;
            if (mce != null)
            {
                switch (mce.Method.Name)
                {
                    case "Atan":
                    case "Cos":
                    case "Log":
                    case "Tan":
                    case "Sin":
                    case "Exp":
                        argument = new CastExpression(argument, typeof (decimal));
                        return base.VisitAggregateExpression(
                            new AggregateExpression(aggregate.Type, aggregate.AggregateName, argument, aggregate.IsDistinct));
                }
            }

            if (aggregate.AggregateName == "Average" && !recursion.Contains(aggregate))
            {
                recursion.Add(aggregate);
                return Visit(new CastExpression(aggregate, typeof(decimal)));
            }

            return base.VisitAggregateExpression(aggregate);
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
        /// Conditional operation
        /// </summary>
        protected override void Conditional(Expression test, Expression ifTrue, Expression ifFalse)
        {
            BinaryExpression binary = test as BinaryExpression;
            if (binary == null || binary.NodeType != ExpressionType.Equal)
            {
                base.Conditional(test, ifTrue, ifFalse);
                return;
            }

            // Use Decode if possible
            InConditionalExpression++;
            try
            {
                WriteSql(" DECODE(");
                Visit(binary.Left);
                WriteSql(", ");
                Visit(binary.Right);
                WriteSql(", ");
                Visit(ifTrue);
                WriteSql(", ");
                Visit(ifFalse);
                WriteSql(")");
            }
            finally
            {
                InConditionalExpression--;
            }
        }

        /// <summary>
        /// Coalesce operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void Coalesce(Expression left, Expression right)
        {
            WriteSql(" NVL(");
            Visit(left);
            WriteSql(", ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Character used to prefix the parameters
        /// </summary>
        protected override string ParameterPrefix { get { return ":"; } }

        ///// <summary>
        ///// SQL Paramter Expression
        ///// </summary>
        ///// <param name="expression"></param>
        ///// <returns></returns>
        //protected override Expression VisitSqlParameterExpression(SqlParameterExpression expression)
        //{
        //    bool guid = (expression.RevealedType == typeof (Guid));

        //    if (guid) WriteSql("HEXTORAW(");
        //    var result = base.VisitSqlParameterExpression(expression);
        //    if (guid) WriteSql(")");

        //    return result;
        //}

        /// <summary>
        /// Trims the specified
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void Trim(Expression expression)
        {
            WriteSql(" TRIM(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Visits the sys date expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysDateExpression(SysDateExpression expression)
        {
            WriteSql("TRUNC(SYSDATE)");
            return expression;
        }

        /// <summary>
        /// Visits the sys time expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysTimeExpression(SysTimeExpression expression)
        {
            WriteSql("SYSDATE");
            return expression;
        }

        /// <summary>
        /// Does the string convert.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void ToStringConvert(Expression expression)
        {
            WriteSql(" TO_CHAR(");
            Visit(expression);
            WriteSql(")");
        }

    }
}
