using System;
using System.Collections.Generic;
using System.Data;
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
    /// Expression Writer for the Access Database Persister
    /// </summary>
    public class AccessExpressionWriter : LinqMethodInspector
    {
        readonly Stack<AliasedExpression> selectStack = new Stack<AliasedExpression>();

        /// <summary>
        /// Constructor to create an expression writer for the access database
        /// </summary>
        public AccessExpressionWriter(ILinqPersister persister, List<PropertyTupel> groupings, Cache<Type, ProjectionClass> cache) 
            : base(persister, groupings, cache)
        {
            Command = persister.CreateCommand();
        }

        /// <summary>
        /// Used to write an Sql Parameter
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitSqlParameterExpression(SqlParameterExpression expression)
        {
            int counter = NumberOfParameters;
            IDbDataParameter param;
            if (!expression.Alias.Generated)
            {
                param = LinqPersister.CreateParameter(expression.Alias.Name, expression.Type, expression.Value, false);
                Command.Parameters.Add(param);
            }
            else
                param = LinqPersister.AddParameter(Command.Parameters, ref counter, expression.Type, expression.Value, false);

            NumberOfParameters = counter;
            WriteSql(param.ParameterName);
            return expression;
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
        /// Conditional expression
        /// </summary>
        /// <param name="test"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        protected override void Conditional(Expression test, Expression ifTrue, Expression ifFalse)
        {
            InConditionalExpression++;
            try
            {
                WriteSql(" IIF(");
                Visit(test);
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
        /// Coalese
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void Coalesce(Expression left, Expression right)
        {
            WriteSql(" IIF (");
            Visit(left);
            WriteSql(" is null, ");
            Visit(right);
            WriteSql(", ");
            Visit(left);
            WriteSql(")");
        }

        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            if (join.Join != JoinType.CrossJoin)
            {
                WriteSql("(");
            }

            VisitJoinLeft(join.Left);
            switch (join.Join)
            {
                case JoinType.CrossJoin:
                    WriteSql(", ");
                    break;
                case JoinType.InnerJoin:
                    WriteSql(") INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    WriteSql(") CROSS APPLY ");
                    break;
                case JoinType.OuterApply:
                    WriteSql(") OUTER APPLY ");
                    break;
                case JoinType.LeftOuter:
                case JoinType.SingletonLeftOuter:
                    WriteSql(") LEFT OUTER JOIN ");
                    break;
            }
            var rightSelect = join.Right as SelectExpression;

            if (rightSelect != null) WriteSql("(");
            VisitJoinRight(join.Right);
            if (join.Condition != null)
            {
                WriteSql(" ON ");
                VisitPredicate(join.Condition);
            }
            if (rightSelect != null)
            {
                WriteSql(") ");
                WriteSql(rightSelect.Alias.Name);
            }

            return join;
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected override void CompareTo(MethodCallExpression expr)
        {
            bool compareWith = ComparerStack.Count > 0;
            if (!compareWith)
                WriteSql(" IIF(");

            if (expr.Object != null) Visit(expr.Object);

            compareWith = ComparerStack.Count > 0;
            if (compareWith)
            {
                // a row shall be compared with a value using a comparison operator.
                WriteSql(ComparerStack.Pop());
                Visit(expr.Arguments[0]);
            }
            else
            {
                WriteSql(" = ");
                Visit(expr.Arguments[0]);
                WriteSql(", 0, IIF(");
                Visit(expr.Object);
                WriteSql(" < ");
                Visit(expr.Arguments[0]);
                WriteSql(", -1, 1))");
            }
        }

        /// <summary>
        /// Compares the specified 
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected override void Compare(MethodCallExpression expr)
        {
            if (expr.Object != null) Visit(expr.Object);
            bool compareWith = ComparerStack.Count > 0;

            if (compareWith)
            {
                if (InConditionalExpression == 0)
                {
                    Visit(expr.Arguments[0]);
                    WriteSql(ComparerStack.Pop());
                    Visit(expr.Arguments[1]);
                }
                else
                {
                    // a row shall be compared with a value using a comparison operator.
                    WriteSql(" IIF(");
                    Visit(expr.Arguments[0]);
                    WriteSql(ComparerStack.Pop());
                    Visit(expr.Arguments[1]);
                    WriteSql(", 1, 0)");
                }
            }
            else
            {
                // a row shall be compared with a value using syntax eq=0, lt=-1, gt=1
                WriteSql(" IIF (");
                Visit(expr.Arguments[0]);
                WriteSql(" = ");
                Visit(expr.Arguments[1]);
                WriteSql(",0 ,IIF(");
                Visit(expr.Arguments[0]);
                WriteSql(" < ");
                Visit(expr.Arguments[1]);
                WriteSql(", -1, 1))");
            }
        }

        /// <summary>
        /// Row Number Expression in Access
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        protected override Expression VisitRowNumberExpression(RowNumberExpression rowNumber)
        {
            var property = OriginPropertyFinder.Find(rowNumber.OrderBy[0].Expression);
            var projection = ReflectionHelper.GetProjection(property.ParentType, DynamicCache);

            WriteSql(" DCOUNT(\"" + property.Name + "\", \"" + projection.TableName(DatabaseType.Access) + "\", \"" + property.Name +
                     " <= '\" & [" + property.Name + "] & \"'\")");
            return rowNumber;
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

            WriteSql(string.Concat(schema, TypeMapper.Quote(table.TableName(DatabaseType.Access)), " "));
            return expression;
        }

        /// <summary>
        /// Between expression
        /// </summary>
        /// <param name="between"></param>
        /// <returns></returns>
        protected override Expression VisitBetweenExpression(BetweenExpression between)
        {
            Visit(between.Expression);
            WriteSql(" BETWEEN ");

            // calculate real value
            var lower = between.Lower;
            var addLower = between.Lower as BinaryExpression;
            if (addLower != null && addLower.NodeType == ExpressionType.Add)
            {
                var val1 = addLower.Left as ValueExpression;
                var val2 = addLower.Right as ValueExpression;
                if (val1 != null && val2 != null && val1.Type == typeof(int))
                    lower = new ValueExpression(typeof (int), (int) val1.Value + (int) val2.Value);
            }

            var upper = between.Upper;
            var addUpper = between.Upper as BinaryExpression;
            if (addUpper != null && addUpper.NodeType == ExpressionType.Add)
            {
                var val1 = addUpper.Left as ValueExpression;
                var val2 = addUpper.Right as ValueExpression;
                if (val1 != null && val2 != null && val1.Type == typeof(int))
                    upper = new ValueExpression(typeof(int), (int)val1.Value + (int)val2.Value);
            }

            Visit(lower);
            WriteSql(" AND ");
            Visit(upper);
            return between;
        }

        /// <summary>
        /// Uppers the given expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void ToUpper(Expression expression)
        {
            WriteSql(" UCASE(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Lowers the given expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void ToLower(Expression expression)
        {
            WriteSql(" LCASE(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// SubString Method
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="from"></param>
        /// <param name="count"></param>
        protected override void Substring(Expression expression, Expression from, Expression count)
        {
            WriteSql(" MID(");
            Visit(expression);
            WriteSql(", ");
            Visit(from);
            WriteSql("+1 ");
            if (count != null)
            {
                WriteSql(", ");
                Visit(count);
            }
            WriteSql(")");
        }

        /// <summary>
        /// Index Of
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="indexOf"></param>
        /// <param name="startFrom"></param>
        protected override void IndexOf(Expression expression, Expression indexOf, Expression startFrom)
        {
            WriteSql(" InStr(");
            if (startFrom != null)
            {
                Visit(startFrom);
                WriteSql("+1");
                WriteSql(", ");
            }
            Visit(expression);
            WriteSql(", ");
            Visit(indexOf);
            WriteSql(")-1");
        }

        /// <summary>
        /// Trim
        /// </summary>
        /// <param name="expression"></param>
        protected override void Trim(Expression expression)
        {
            WriteSql(" TRIM(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// News the date time.
        /// </summary>
        protected override void NewDateTime(Expression year, Expression month, Expression day)
        {
            WriteSql(" CDATE(");

            var time = new[] { year, month, day };
            AddDateTimeExpressions(time);
            WriteSql(")");
        }

        /// <summary>
        /// News the date time.
        /// </summary>
        protected override void NewDateTime(Expression year, Expression month, Expression day, Expression hour,
                                        Expression minute, Expression second)
        {
            WriteSql(" CDATE(");

            var time = new[] { year, month, day, hour, minute, second };
            AddDateTimeExpressions(time);
            WriteSql(")");
        }

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

                    WriteSql(" CSTR(");
                    Visit(time[x]);
                    WriteSql(") ");
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
        /// Gets the weekday
        /// </summary>
        /// <param name="expression"></param>
        protected override void DayOfWeek(Expression expression)
        {
            WriteSql(" WEEKDAY(");
            Visit(expression);
            WriteSql(")-1");
        }

        /// <summary>
        /// Second
        /// </summary>
        /// <param name="expression"></param>
        protected override void Second(Expression expression)
        {
            WriteSql(" SECOND(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Minute
        /// </summary>
        /// <param name="expression"></param>
        protected override void Minute(Expression expression)
        {
            WriteSql(" MINUTE(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Hour
        /// </summary>
        /// <param name="expression"></param>
        protected override void Hour(Expression expression)
        {
            WriteSql(" HOUR(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Years to the given "addTo" Expression.
        /// </summary>
        protected override void AddYears(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"yyyy\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Months to the given "addTo" Expression.
        /// </summary>
        protected override void AddMonths(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"m\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Days to the given "addTo" Expression.
        /// </summary>
        protected override void AddDays(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"d\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Hours to the given "addTo" Expression.
        /// </summary>
        protected override void AddHours(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"h\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Minutes to the given "addTo" Expression.
        /// </summary>
        protected override void AddMinutes(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"n\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Seconds to the given "addTo" Expression.
        /// </summary>
        protected override void AddSeconds(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(\"s\",");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Converts the value to an integer
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected override void Floor(Expression expression)
        {
            WriteSql(" INT(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Truncates the decimal part of a double
        /// </summary>
        /// <param name="expression"></param>
        protected override void Truncate(Expression expression)
        {
            WriteSql(" FIX(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Bitwise AND
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void BitwiseAnd(Expression left, Expression right)
        {
            throw new NotSupportedException("Bitwise Operators are not supported by Microsoft Access.");
        }

        /// <summary>
        /// Bitwise XOR
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void BitwiseExclusiveOr(Expression left, Expression right)
        {
            throw new NotSupportedException("Bitwise Operators are not supported by Microsoft Access.");
        }

        /// <summary>
        /// Bitwise OR
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void BitwiseOr(Expression left, Expression right)
        {
            throw new NotSupportedException("Bitwise Operators are not supported by Microsoft Access.");
        }

        /// <summary>
        /// Bitwise Modulo
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void Modulo(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" MOD ");
            Visit(right);
        }

        /// <summary>
        /// Power Operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void Power(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" ^ ");
            Visit(right);
        }

        /// <summary>
        /// Lefts the shift.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void LeftShift(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" * (2 ^ ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Rights the shift.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected override void RightShift(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" / (2 ^ ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Calcualtes the square root
        /// </summary>
        /// <param name="expression"></param>
        protected override void Sqrt(Expression expression)
        {
            WriteSql(" SQR(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Arc Tangent Function
        /// </summary>
        /// <param name="expression"></param>
        protected override void ATan(Expression expression)
        {
            WriteSql(" ATN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Manages the integer division
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        protected override void Divide(Expression left, Expression right)
        {
            Visit(left);
            if (right.Type == typeof(int))
                WriteSql(" \\ ");
            else
                WriteSql(" / ");
            Visit(right);
        }
    }
}
