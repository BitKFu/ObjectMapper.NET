using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Language
{
    /// <summary>
    /// The LinqMethodInspector class defines commands that is invoked by the expression 
    /// </summary>
    public class LinqMethodInspector : DbPackedExpressionVisitor
    {
        /// <summary> Internal String Builder used to crate the SQL Statement </summary>
        private readonly StringBuilder builder = new StringBuilder();

        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        /// <value>The root.</value>
        public Expression Root { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqMethodInspector"/> class.
        /// </summary>
        /// <param name="persister">The persister.</param>
        /// <param name="groupings">The groupings.</param>
        /// <param name="backpack">The backpack.</param>
        protected LinqMethodInspector(ILinqPersister persister, List<PropertyTupel> groupings, ExpressionVisitorBackpack backpack)
            :base(backpack)
        {
            LinqPersister = persister;
            ComparerStack = new Stack<string>();
            groupings.ForEach(tupel => Groupings.Add(tupel));
        }

        /// <summary>
        /// Gets or sets the comparer stack.
        /// </summary>
        /// <value>The comparer stack.</value>
        protected Stack<string> ComparerStack { get; private set; }

        /// <summary>
        /// Gets the dynamic cache.
        /// </summary>
        /// <value>The dynamic cache.</value>
        protected Cache<Type, ProjectionClass> DynamicCache
        {
            get { return Backpack.ProjectionCache; }
        }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        protected StringBuilder Builder
        {
            get { return builder; }
        }

        /// <summary>
        /// Gets or sets the in conditional expression.
        /// </summary>
        /// <value>The in conditional expression.</value>
        protected int InConditionalExpression { get; set; }

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        protected virtual string Concatinator
        {
            get { return "+"; }
        }

        /// <summary>
        /// Gets the empty from clause.
        /// </summary>
        /// <value>The empty from clause.</value>
        protected virtual string EmptyFromClause
        {
            get { return string.Empty; }
        }

        /// <summary> Gets the SQL command. </summary>
        public IDbCommand Command { get; protected set; }

        /// <summary> Gets or sets the persister. </summary>
        public ILinqPersister LinqPersister { get; private set; }

        /// <summary> Gets or sets the type mapper. </summary>
        public ITypeMapper TypeMapper
        {
            get { return LinqPersister.TypeMapper; }
        }

        /// <summary> Gets or sets the number of parameters. </summary>
        protected int NumberOfParameters { get; set; }

        /// <summary>
        /// Character used to prefix the parameters
        /// </summary>
        protected virtual string ParameterPrefix
        {
            get { return "@"; }
        }

        #region Set Methods

        /// <summary>
        /// Unions the specified 
        /// </summary>
        protected virtual void Union(Expression left, Expression right, bool isAll)
        {
            Visit(left);
            WriteSql(" UNION ");
            if (isAll) WriteSql(" ALL ");
            Visit(right);
        }

        /// <summary>
        /// Likes the specified search for.
        /// </summary>
        /// <param name="searchFor">The search for.</param>
        /// <param name="searchIn">The search in.</param>
        private void Like(Expression searchFor, Expression searchIn)
        {
            Visit(searchFor);
            WriteSql(" like ");
            Visit(searchIn);
        }

        /// <summary>
        /// Visits the specified 
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected virtual void Contains(MethodCallExpression expr)
        {
            Expression containsObject = expr.Arguments.Count > 1
                                            ? expr.Arguments[0]
                                            : expr.Object;


            if (containsObject.Type.IsArray ||
                containsObject.Type.IsListType() ||
                (containsObject.Type.IsGenericType &&
                 containsObject.Type.GetGenericTypeDefinition() == typeof (IQueryable<>)))
            {
                Visit(expr.Arguments.Last());
                WriteSql(" IN (");
                Visit(containsObject);
                WriteSql(")");
                return;
            }

            Visit(containsObject);

            WriteSql(" like '%' " + Concatinator);
            foreach (Expression expression in expr.Arguments)
                Visit(expression);
            WriteSql(" " + Concatinator + " '%'");
        }

        #endregion

        #region Aggregation Methods

        /// <summary>
        /// Returns the count of the given expression 
        /// </summary>
        protected virtual void Count(Expression count, bool distinct)
        {
            WriteSql(" count(");

            var whereCall = count as MethodCallExpression;
            if (whereCall != null && whereCall.Method.Name == "Where")
                WriteSql("*");
            else
                Visit(count);

            WriteSql(")");
        }

        /// <summary>
        /// Returns the average value of the given expression
        /// </summary>
        protected virtual void Average(Expression count)
        {
            WriteSql(" avg(");
            Visit(count);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the minimum value of the given expression
        /// </summary>
        protected virtual void Min(Expression count)
        {
            WriteSql(" min(");
            Visit(count);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the maximum value of the given expression
        /// </summary>
        protected virtual void Max(Expression count)
        {
            WriteSql(" max(");
            Visit(count);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the summed value of the given expression
        /// </summary>
        protected virtual void Sum(Expression count)
        {
            WriteSql(" sum(");
            Visit(count);
            WriteSql(")");
        }

        #endregion

        #region Conditional Methods

        /// <summary>
        /// Conditionals the specified 
        /// That are expressions which are using the ? operator
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        protected virtual void Conditional(Expression test, Expression ifTrue, Expression ifFalse)
        {
            InConditionalExpression++;
            try
            {
                WriteSql(" CASE WHEN (");
                Visit(test);
                WriteSql(") THEN ");
                Visit(ifTrue);
                WriteSql(" ELSE ");
                Visit(ifFalse);
                WriteSql(" END");
            }
            finally
            {
                InConditionalExpression--;
            }
        }

        /// <summary>
        /// Coalesces the specified 
        /// That are expressions which are using the ?? operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Coalesce(Expression left, Expression right)
        {
            WriteSql(" CASE WHEN (");
            Visit(left);
            WriteSql(" is null) THEN ");
            Visit(right);
            WriteSql(" ELSE ");
            Visit(left);
            WriteSql(" END");
        }

        #endregion

        #region String Methods

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected virtual void CompareTo(MethodCallExpression expr)
        {
            bool compareWith = ComparerStack.Count > 0;
            if (!compareWith)
                WriteSql(" CASE WHEN (");

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
                WriteSql(") THEN 0 ELSE CASE WHEN (");
                Visit(expr.Object);
                WriteSql(" < ");
                Visit(expr.Arguments[0]);
                WriteSql(") THEN -1 ELSE 1 END END");
            }
        }

        /// <summary>
        /// Compares the specified 
        /// </summary>
        /// <param name="expr">The expr.</param>
        protected virtual void Compare(MethodCallExpression expr)
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
                    WriteSql(" CASE WHEN (");
                    Visit(expr.Arguments[0]);
                    WriteSql(ComparerStack.Pop());
                    Visit(expr.Arguments[1]);
                    WriteSql(") THEN 1 ELSE 0 END");
                }
            }
            else
            {
                // a row shall be compared with a value using syntax eq=0, lt=-1, gt=1
                WriteSql(" CASE WHEN (");
                Visit(expr.Arguments[0]);
                WriteSql(" = ");
                Visit(expr.Arguments[1]);
                WriteSql(") THEN 0 ELSE CASE WHEN (");
                Visit(expr.Arguments[0]);
                WriteSql(" < ");
                Visit(expr.Arguments[1]);
                WriteSql(") THEN -1 ELSE 1 END END");
            }
        }

        /// <summary>
        /// Endses the with.
        /// </summary>
        /// <param name="toQuery">To query.</param>
        /// <param name="endsWith">The ends with.</param>
        protected virtual void EndsWith(Expression toQuery, Expression endsWith)
        {
            Visit(toQuery);
            WriteSql(" like '%' " + Concatinator);
            Visit(endsWith);
        }

        /// <summary>
        /// Startses the with.
        /// </summary>
        /// <param name="toQuery">To query.</param>
        /// <param name="startsWith">The starts with.</param>
        protected virtual void StartsWith(Expression toQuery, Expression startsWith)
        {
            Visit(toQuery);
            WriteSql(" like ");
            Visit(startsWith);
            WriteSql(" " + Concatinator + " '%'");
        }

        /// <summary>
        /// Concats the specified 
        /// </summary>
        protected virtual void Concat(NewArrayExpression expression)
        {
            for (int x = 0; x < expression.Expressions.Count; x++)
            {
                if (x > 0) WriteSql(string.Concat(" ", Concatinator, " "));
                Visit(expression.Expressions[x]);
            }
        }

        /// <summary>
        /// Determines whether [is null or empty] [the specified visitor].
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void IsNullOrEmpty(Expression expression)
        {
            WriteSql("(");
            Visit(expression);
            WriteSql(" is null or len(");
            Visit(expression);
            WriteSql(")=0)");
        }


        /// <summary>
        /// Trims the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Trim(Expression expression)
        {
            WriteSql(" LTRIM(RTRIM(");
            Visit(expression);
            WriteSql("))");
        }

        /// <summary>
        /// Uppers the given expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void ToUpper(Expression expression)
        {
            WriteSql(" UPPER(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Lowers the given expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void ToLower(Expression expression)
        {
            WriteSql(" LOWER(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Does the string convert.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void ToStringConvert(Expression expression)
        {
            WriteSql(" CSTR(");
            Visit(expression);
            WriteSql(")");
        }

        #endregion

        #region Bit Operators

        /// <summary>
        /// Lefts the shift.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void LeftShift(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" * POWER(2, ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Rights the shift.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void RightShift(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" / POWER(2, ");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Ors the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void BitwiseOr(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" | ");
            Visit(right);
        }

        /// <summary>
        /// Ands the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void BitwiseAnd(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" & ");
            Visit(right);
        }

        /// <summary>
        /// Nots the specified 
        /// </summary>
        /// <param name="operand">The operand.</param>
        protected virtual void BitwiseNot(Expression operand)
        {
            WriteSql(" ~(");
            Visit(operand);
            WriteSql(")");
        }

        /// <summary>
        /// Exclusives the or.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void BitwiseExclusiveOr(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" ^ ");
            Visit(right);
        }

        #endregion

        #region Math Methods

        /// <summary>
        /// Negates the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Negate(Expression expression)
        {
            WriteSql(" -(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Adds the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Add(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" + ");
            Visit(right);
        }

        /// <summary>
        /// Subtracts the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Subtract(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" - ");
            Visit(right);
        }

        /// <summary>
        /// Divides the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Divide(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" / ");
            Visit(right);
        }

        /// <summary>
        /// Multiplies the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Multiply(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" * ");
            Visit(right);
        }

        /// <summary>
        /// Moduloes the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Modulo(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" % ");
            Visit(right);
        }

        /// <summary>
        /// Powers the specified 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void Power(Expression left, Expression right)
        {
            WriteSql("POWER(");
            Visit(left);
            WriteSql(",");
            Visit(right);
            WriteSql(")");
        }

        /// <summary>
        /// Truncates the decimal part of a double
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Floor(Expression expression)
        {
            WriteSql(" FLOOR(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Rounds the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Round(Expression expression)
        {
            WriteSql(" ROUND(");
            Visit(expression);
            WriteSql(",0");
            WriteSql(")");
        }

        /// <summary>
        /// Abses the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Abs(Expression expression)
        {
            WriteSql(" ABS(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Truncates the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Truncate(Expression expression)
        {
            if (expression.Type == typeof(DateTime))
            {
                WriteSql("CAST(FLOOR(CAST(");
                Visit(expression);
                WriteSql(" AS FLOAT)) AS DATETIME)");
            }
            else
            {
                WriteSql(" CAST(");
                Visit(expression);
                WriteSql(" as INTEGER");
                WriteSql(")");
            }
        }

        /// <summary>
        /// Mathematical Sinus Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Sin(Expression expression)
        {
            WriteSql(" SIN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Cosinus Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Cos(Expression expression)
        {
            WriteSql(" COS(");
            Visit(expression);
            WriteSql(")");
        }


        /// <summary>
        /// Mathematical Arc Tangent Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void ATan(Expression expression)
        {
            WriteSql(" ATAN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Tangent Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Tan(Expression expression)
        {
            WriteSql(" TAN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Exponent Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Exp(Expression expression)
        {
            WriteSql(" EXP(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Logarithm Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Log(Expression expression)
        {
            WriteSql(" LOG(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Mathematical Squareroot Function
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Sqrt(Expression expression)
        {
            WriteSql(" SQRT(");
            Visit(expression);
            WriteSql(")");
        }

        #endregion

        #region DateTime Methods

        /// <summary>
        /// News the date time.
        /// </summary>
        protected virtual void NewDateTime(Expression year, Expression month, Expression day)
        {
            WriteSql(" CONVERT(datetime, ");

            var time = new[] {year, month, day};
            AddDateTimeExpressions(time);
            WriteSql(", 111)");
        }

        /// <summary>
        /// News the date time.
        /// </summary>
        protected virtual void NewDateTime(Expression year, Expression month, Expression day, Expression hour,
                                        Expression minute, Expression second)
        {
            WriteSql(" CONVERT(datetime, ");

            var time = new[] {year, month, day, hour, minute, second};
            AddDateTimeExpressions(time);
            WriteSql(", 20)");
        }

        /// <summary>
        /// Adds the date time expressions.
        /// </summary>
        /// <param name="time">The time.</param>
        protected virtual void AddDateTimeExpressions(Expression[] time)
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

                    WriteSql(" CONVERT(varchar,");
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
        /// Returns the Year of the given expression
        /// </summary>
        protected virtual void Year(Expression expression)
        {
            WriteSql(" YEAR(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Month of the given expression
        /// </summary>
        protected virtual void Month(Expression expression)
        {
            WriteSql(" MONTH(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Day of the given expression
        /// </summary>
        protected virtual void Day(Expression expression)
        {
            WriteSql(" DAY(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Hour of the given expression
        /// </summary>
        protected virtual void Hour(Expression expression)
        {
            WriteSql(" DATEPART(hour,");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Minute of the given expression
        /// </summary>
        protected virtual void Minute(Expression expression)
        {
            WriteSql(" DATEPART(minute,");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Second of the given expression
        /// </summary>
        protected virtual void Second(Expression expression)
        {
            WriteSql(" DATEPART(second,");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Returns the Weekday of the given expression
        /// </summary>
        protected virtual void DayOfWeek(Expression expression)
        {
            WriteSql(" DATEPART(weekday,");
            Visit(expression);
            WriteSql(")-1");
        }

        /// <summary>
        /// Add an amount of Years to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddYears(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(YYYY,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Months to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddMonths(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(mm,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Days to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddDays(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(DAY,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Hours to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddHours(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(HH,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Minutes to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddMinutes(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(MI,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        /// <summary>
        /// Add an amount of Seconds to the given "addTo" Expression.
        /// </summary>
        protected virtual void AddSeconds(Expression addToExp, Expression addValueExp)
        {
            WriteSql(" DATEADD(SS,");
            Visit(addValueExp);
            WriteSql(",");
            Visit(addToExp);
            WriteSql(")");
        }

        #endregion

        /// <summary>
        /// Nots the specified 
        /// </summary>
        /// <param name="operand">The operand.</param>
        protected virtual void Not(Expression operand)
        {
            WriteSql(" not (");
            Visit(operand);
            WriteSql(")");
        }

        ///// <summary>
        ///// Alls the specified 
        ///// </summary>
        ///// <param name="visitor">The </param>
        ///// <param name="type">The type.</param>
        ///// <param name="expression">The expression.</param>
        //protected virtual void TopAll(Type type, Expression expression)
        //{
        //    WriteSql(" CASE WHEN (");
        //    All(visitor, type, expression);
        //    WriteSql(") THEN 1 ELSE 0 END");
        //}

        ///// <summary>
        ///// Alls the specified 
        ///// </summary>
        ///// <param name="visitor">The </param>
        ///// <param name="type">The type.</param>
        ///// <param name="expression">The expression.</param>
        //protected virtual void All(Type type, Expression expression)
        //{
        //    WriteSql(" NOT EXISTS(SELECT NULL FROM ");
        //    WriteTable(type, expression);
        //    if (expression != null)
        //    {
        //        WriteSql(" WHERE NOT ");
        //        Visit(expression);
        //    }
        //    WriteSql(")");
        //}

        /// <summary>
        /// Lengthes the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected virtual void Length(Expression expression)
        {
            WriteSql(" LEN(");
            Visit(expression);
            WriteSql(")");
        }

        /// <summary>
        /// Write the Exisits expression
        /// </summary>
        protected override Expression VisitExistsExpression(ExistsExpression expression)
        {
            WriteSql(" EXISTS(");
            Visit(expression.Selection);
            WriteSql(")");
            return expression;
        }

        ///// <summary>
        ///// Anies the specified 
        ///// </summary>
        ///// <param name="visitor">The </param>
        ///// <param name="type">The type.</param>
        ///// <param name="expression">The expression.</param>
        //protected virtual void TopAny(Type type, Expression expression)
        //{
        //    WriteSql(" CASE WHEN (");
        //    Any(visitor,type,expression);
        //    WriteSql(") THEN 1 ELSE 0 END");
        //}

        ///// <summary>
        ///// Anies the specified 
        ///// </summary>
        ///// <param name="visitor">The </param>
        ///// <param name="type">The type.</param>
        ///// <param name="expression">The expression.</param>
        //protected virtual void Any(Type type, Expression expression)
        //{
        //    WriteSql(" EXISTS(SELECT NULL FROM ");
        //    WriteTable(type, expression);
        //    if (expression != null)
        //    {
        //        WriteSql(" WHERE ");
        //        Visit(expression);
        //    }
        //    WriteSql(")");
        //}

        /// <summary>
        /// Substrings the specified 
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="from">From.</param>
        /// <param name="count">The count.</param>
        protected virtual void Substring(Expression expression, Expression from, Expression count)
        {
            WriteSql(" SUBSTRING(");
            Visit(expression);
            WriteSql(", ");
            Visit(from);
            WriteSql("+1, ");
            if (count != null)
                Visit(count);
            else
                WriteSql("8000");
            WriteSql(")");
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="indexOf">The index of.</param>
        /// <param name="startFrom">The start from.</param>
        protected virtual void IndexOf(Expression expression, Expression indexOf, Expression startFrom)
        {
            WriteSql(" CHARINDEX(");
            Visit(indexOf);
            WriteSql(", ");
            Visit(expression);

            if (startFrom != null)
            {
                WriteSql(", ");
                Visit(startFrom);
                WriteSql("+1");
            }

            WriteSql(")-1");
        }

        /// <summary>
        /// Add two expressions with an AND Condition
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void AndAlso(Expression left, Expression right)
        {
            Visit(left);
            WriteSql(" AND ");
            Visit(right);
        }

        /// <summary>
        /// Add two expressions with an OR Condition
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        protected virtual void OrElse(Expression left, Expression right)
        {
            WriteSql("(");
            Visit(left);
            WriteSql(" OR ");
            Visit(right);
            WriteSql(") ");
        }

        /// <summary>
        /// Writes the SQL.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        public void WriteSql(string sql)
        {
            builder.Append(sql);
        }

        /// <summary>
        /// Evaluates the expression and returns the database command object
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual IDbCommand EvaluateCommand(Expression expression)
        {
            Visit(expression);
            Command.CommandText = Builder.ToString();
            return Command;
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        public static IDbCommand Evaluate(Type expressionWriterType, Expression root, Expression expression,
                                          List<PropertyTupel> groupings, ILinqPersister linqPersister,
                                          ExpressionVisitorBackpack backpack)
        {
            var writer = (LinqMethodInspector) Activator.CreateInstance(expressionWriterType, linqPersister, groupings, backpack);
            writer.Root = root;
            IDbCommand command = writer.EvaluateCommand(expression);
            return command;
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression expr)
        {
            switch (expr.Method.Name)
            {
                    /*
                 * Aggregations
                 */
                case "Count":
                case "LongCount":
                    Count(expr.Arguments.Last(), false);
                    break;

                case "Average":
                    Average(expr.Arguments.Last());
                    break;

                case "Min":
                //case "First":
                    Min(expr.Arguments.Last());
                    break;

                case "Max":
                //case "Last":
                    Max(expr.Arguments.Last());
                    break;

                case "Sum":
                    Sum(expr.Arguments.Last());
                    break;

                case "Cos":
                    Cos(expr.Arguments.Last());
                    break;

                case "Atan":
                    ATan(expr.Arguments.Last());
                    break;

                case "Sin":
                    Sin(expr.Arguments.Last());
                    break;

                case "Tan":
                    Tan(expr.Arguments.Last());
                    break;

                case "Pow":
                    Power(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Exp":
                    Exp(expr.Arguments.Last());
                    break;

                case "Log":
                    Log(expr.Arguments.Last());
                    break;

                case "Sqrt":
                    Sqrt(expr.Arguments.Last());
                    break;

                case "Like":
                    Like(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Explicit":
                    Explicit(expr.Arguments.First());
                    break;

                /*
                 * Where Clause Method Callings
                 */
                case "Contains":
                    Contains(expr);
                    break;

                case "Substring":
                    Substring(expr.Object, expr.Arguments[0], expr.Arguments.Count == 2 ? expr.Arguments[1] : null);
                    break;

                case "IndexOf":
                    IndexOf(expr.Object, expr.Arguments[0], expr.Arguments.Count == 2 ? expr.Arguments[1] : null);
                    break;

                case "CompareTo":
                    CompareTo(expr);
                    break;

                case "Compare":
                    Compare(expr);
                    break;

                case "Union":
                case "UnionAll":
                    Union(expr.Arguments[0], expr.Arguments[1], expr.Method.Name == "UnionAll");
                    break;

                case "EndsWith":
                    EndsWith(expr.Object, expr.Arguments.First());
                    break;

                case "StartsWith":
                    StartsWith(expr.Object, expr.Arguments.First());
                    break;

                case "Concat":
                    NewArrayExpression arrayExp = expr.Arguments.First() as NewArrayExpression ??
                                                  Expression.NewArrayInit(expr.Arguments.First().Type, expr.Arguments);

                    Concat(arrayExp);
                    break;

                case "IsNullOrEmpty":
                    IsNullOrEmpty(expr.Arguments.First());
                    break;

                case "Negate":
                    Negate(expr.Arguments.First());
                    break;

                case "Add":
                    Add(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Subtract":
                    Subtract(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Divide":
                    Divide(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Multiply":
                    Multiply(expr.Arguments[0], expr.Arguments[1]);
                    break;

                case "Floor":
                    Floor(expr.Arguments.First());
                    break;

                case "Round":
                    Round(expr.Arguments.First());
                    break;

                case "Abs":
                    Abs(expr.Arguments.First());
                    break;

                case "Truncate":
                    Truncate(expr.Arguments.First());
                    break;

                case "Year":
                    Year(expr.Arguments.First());
                    break;

                case "Month":
                    Month(expr.Arguments.First());
                    break;

                case "Day":
                    Day(expr.Arguments.First());
                    break;

                case "Hour":
                    Hour(expr.Arguments.First());
                    break;

                case "Minute":
                    Minute(expr.Arguments.First());
                    break;

                case "Second":
                    Second(expr.Arguments.First());
                    break;

                case "DayOfWeek":
                    DayOfWeek(expr.Arguments.First());
                    break;

                case "AddYears":
                    AddYears(expr.Object, expr.Arguments.First());
                    break;

                case "AddMonths":
                    AddMonths(expr.Object, expr.Arguments.First());
                    break;

                case "AddDays":
                    AddDays(expr.Object, expr.Arguments.First());
                    break;

                case "AddHours":
                    AddHours(expr.Object, expr.Arguments.First());
                    break;

                case "AddMinutes":
                    AddMinutes(expr.Object, expr.Arguments.First());
                    break;

                case "AddSeconds":
                    AddSeconds(expr.Object, expr.Arguments.First());
                    break;

                case "Trim":
                    Trim(expr.Object);
                    break;

                case "ToUpper":
                    ToUpper(expr.Object);
                    break;

                case "ToLower":
                    ToLower(expr.Object);
                    break;

                case "ToList":
                case "First":
                case "FirstOrDefault":
                case "Last":
                case "LastOrDefault":
                    return base.VisitMethodCall(expr);

                case "ToString":
                    ToStringConvert(expr.Object);
                    break;
                    
                default:
                    throw new NotSupportedException(expr.Method.Name + " is currently not supported by the ObjectMapper .NET");
            }

            return expr;
        }

        /// <summary>
        /// Explicit Parameter replacement
        /// </summary>
        /// <param name="expression"></param>
        protected virtual void Explicit(Expression expression)
        {
            var parameterExpression = expression as SqlParameterExpression;
            if (parameterExpression == null)
                Visit(expression);
            else
            {
                if (Root is LambdaExpression)
                {
                    WriteSql("${");
                    Visit(expression);
                    WriteSql("}");
                }
                else   
                    WriteSql(TypeMapper.GetParamValueAsSQLString(parameterExpression.Value));
            }
        }

        /// <summary>
        /// Visits the expression list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        public ReadOnlyCollection<Expression> VisitList(ReadOnlyCollection<Expression> original)
        {
            return VisitExpressionList(original);
        }

        /// <summary>
        /// Visits the unary.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Negate:
                    Negate(expr.Operand);
                    break;

                case ExpressionType.Not:
                    if (expr.Operand.Type == typeof (Int32))
                        BitwiseNot(expr.Operand);
                    else
                        Not(expr.Operand);
                    break;

                case ExpressionType.Quote:
                    WriteSql("(");
                    Visit(expr.Operand);
                    WriteSql(")");
                    break;

                default:
                    return base.VisitUnary(expr);
            }

            return expr;
        }

        /// <summary>
        /// Visits the binary expresssions.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <exception cref="NotImplementedException">Thrown when an unknown Binary Exception has been thrown</exception>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Add:
                    if (expr.Method != null)
                        switch (expr.Method.Name)
                        {
                            case "Concat":
                                Concat(Expression.NewArrayInit(expr.Left.Type, expr.Left, expr.Right));
                                break;

                            default:
                                return base.VisitBinary(expr);
                        }
                    else
                        Add(expr.Left, expr.Right);
                    break;

                case ExpressionType.AddChecked:
                    throw new NotSupportedException("Not supported until now");

                case ExpressionType.Subtract:
                    Subtract(expr.Left, expr.Right);
                    break;

                case ExpressionType.SubtractChecked:
                    throw new NotSupportedException("Not supported until now");

                case ExpressionType.Multiply:
                    Multiply(expr.Left, expr.Right);
                    break;

                case ExpressionType.MultiplyChecked:
                    throw new NotSupportedException("Not supported until now");

                case ExpressionType.Divide:
                    Divide(expr.Left, expr.Right);
                    break;

                case ExpressionType.Modulo:
                    Modulo(expr.Left, expr.Right);
                    break;

                case ExpressionType.And:
                    BitwiseAnd(expr.Left, expr.Right);
                    break;

                case ExpressionType.AndAlso:
                    AndAlso(expr.Left, expr.Right);
                    break;

                case ExpressionType.Or:
                    BitwiseOr(expr.Left, expr.Right);
                    break;

                case ExpressionType.OrElse:
                    OrElse(expr.Left, expr.Right);
                    break;

                case ExpressionType.Coalesce:
                    Coalesce(expr.Left, expr.Right);
                    break;

                case ExpressionType.ArrayIndex:
                    throw new NotSupportedException("Not supported until now");

                case ExpressionType.RightShift:
                    RightShift(expr.Left, expr.Right);
                    break;

                case ExpressionType.LeftShift:
                    LeftShift(expr.Left, expr.Right);
                    break;

                case ExpressionType.ExclusiveOr:
                    BitwiseExclusiveOr(expr.Left, expr.Right);
                    break;

                case ExpressionType.Power:
                    Power(expr.Left, expr.Right);
                    break;

                default:
                    throw new NotImplementedException("Unknown Binary Type");
            }

            return expr;
        }

        /// <summary>
        /// Visits the new.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected override NewExpression VisitNew(NewExpression expr)
        {
            // DateTime SQL Equivalent
            if (expr.Type == typeof (DateTime))
            {
                if (expr.Arguments.Count == 3)
                {
                    NewDateTime(expr.Arguments[0], expr.Arguments[1], expr.Arguments[2]);
                    return expr;
                }

                if (expr.Arguments.Count == 6)
                {
                    NewDateTime(expr.Arguments[0], expr.Arguments[1], expr.Arguments[2], expr.Arguments[3],
                                expr.Arguments[4], expr.Arguments[5]);
                    return expr;
                }
                throw new NotSupportedException("Not Supported DateTimeConversion");
            }

            return base.VisitNew(expr);
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            WriteSql(string.Concat(expression.Alias.Name, ".", TypeMapper.Quote(expression.Name)));
            return expression;
        }

        /// <summary>
        /// Visits the value expression.
        /// </summary>
        protected override Expression VisitValueExpression(ValueExpression expression)
        {
            var enumerable = expression.Value as IList;
            if (enumerable != null)
            {
                bool first = true;
                IEnumerator enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (!first) WriteSql(", ");
                    WriteSql(TypeMapper.GetParamValueAsSQLString(enumerator.Current));
                    first = false;
                }
            }
            else
                WriteSql(TypeMapper.GetParamValueAsSQLString(expression.Value));

            return expression;
        }


        /// <summary>
        /// Visits the expression list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original == null)
                return null;

            for (int x = 0; x < original.Count; x++)
            {
                if (x > 0) WriteSql(", ");
                Visit(original[x]);
            }
            return original;
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            Builder.Append(c.Value);
            return c;
        }

        /// <summary>
        /// Visits the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            IRetriever tupel = GetRetriever(m);

            // Check system methods, like DateTime.Day or something that way
            if (!tupel.Target.IsGroupingType() && !tupel.Target.IsAnonymousType() &&
                (!tupel.Target.IsValueObjectType()
                 && !tupel.Target.IsProjectedType(null)))
            {
                switch (tupel.Source.Name)
                {
                    case "Year":
                        Year(m.Expression);
                        break;

                    case "Month":
                        Month(m.Expression);
                        break;

                    case "Day":
                        Day(m.Expression);
                        break;

                    case "Hour":
                        Hour(m.Expression);
                        break;

                    case "Minute":
                        Minute(m.Expression);
                        break;

                    case "Second":
                        Second(m.Expression);
                        break;

                    case "DayOfWeek":
                        DayOfWeek(m.Expression);
                        break;

                    case "Length":
                        Length(m.Expression);
                        break;

                    case "Trunc":
                    case "Date":
                        Truncate(m.Expression);
                        break;

                    default:
                        Visit(m.Expression);
                        break;
                }
            }
            else
                Visit(m.Expression);

            return m;
        }

        /// <summary>
        /// Visits the Aggretgate Expression
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected override Expression VisitAggregateExpression(AggregateExpression aggregate)
        {
            Expression argument = aggregate.Argument ?? Expression.Constant("*");

            switch (aggregate.AggregateName)
            {
                case "LongCount":
                case "Count":
                    Count(argument, aggregate.IsDistinct);
                    return aggregate;

                case "Average":
                    Average(argument);
                    return aggregate;

                case "Sum":
                    Sum(argument);
                    return aggregate;

                case "Min":
                    Min(argument);
                    return aggregate;

                case "Max":
                    Max(argument);
                    return aggregate;

                default:
                    WriteSql(aggregate.AggregateName);
                    break;
            }

            WriteSql("(");
            if (aggregate.IsDistinct) WriteSql("DISTINCT ");
            if (aggregate.Argument != null)
                Visit(aggregate.Argument);
            else
                WriteSql("*");
            WriteSql(")");
            return aggregate;
        }

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="expression">The expression.</param>
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
        /// Visits the conditional expression.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression expr)
        {
            Conditional(expr.Test, expr.IfTrue, expr.IfFalse);
            return expr;
        }

        /// <summary>
        /// Visits the rownumber expression
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        protected override Expression VisitRowNumberExpression(RowNumberExpression rowNumber)
        {
            WriteSql(" ROW_NUMBER() OVER( ORDER BY ");
            VisitOrderBy(rowNumber.OrderBy);
            WriteSql(")");
            return rowNumber;
        }

        /// <summary>
        /// Visits the order by.
        /// </summary>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        protected override ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions == null) return expressions;

            List<OrderExpression> alternate = null;
            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                if (i>0)
                    WriteSql(", ");

                OrderExpression expr = expressions[i];
                OrderExpression e = Visit(expr) as OrderExpression;

                if (alternate == null && e != expr)
                    alternate = expressions.Take(i).ToList();

                if (alternate != null)
                    alternate.Add(e);
            }

            return alternate != null ? alternate.AsReadOnly() : expressions;
        }

        /// <summary>
        /// Visits the Order Expression
        /// </summary>
        /// <param name="orderExpression"></param>
        /// <returns></returns>
        protected override Expression VisitOrderExpression(OrderExpression orderExpression)
        {
            Visit(orderExpression.Expression);
            WriteSql(" " + orderExpression.Ordering);
            return orderExpression;
        }

        /// <summary>
        /// Visits the between expression
        /// </summary>
        /// <param name="between"></param>
        /// <returns></returns>
        protected override Expression VisitBetweenExpression(BetweenExpression between)
        {
            Visit(between.Expression);
            WriteSql(" BETWEEN ");
            Visit(between.Lower);
            WriteSql(" AND ");
            Visit(between.Upper);
            return between;
        }

        /// <summary>
        /// Visits the scalar expression.
        /// </summary>
        /// <param name="select">The select.</param>
        /// <returns></returns>
        protected override Expression VisitScalarExpression(ScalarExpression select)
        {
            WriteSql("SELECT ");

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

            WriteSql(" FROM ");

            Visit(select.From);

            var aliasedFrom = select.From as AliasedExpression;
            if (aliasedFrom != null)
                WriteSql(" " + aliasedFrom.Alias.Name);

            return select;
        }

        /// <summary>
        /// Gets the value expression.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected ValueExpression GetValueExpression(Expression expr)
        {
            ValueExpression valueExpression = expr as ValueExpression;
            if (valueExpression != null)
                return valueExpression;

            UnaryExpression unaryExpression = expr as UnaryExpression;
            if (unaryExpression != null)
                return GetValueExpression(unaryExpression.Operand);

            return null;
        }

        /// <summary>
        /// Visits the comparison.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <param name="queryOperator">The query operator.</param>
        protected override Expression VisitComparison(BinaryExpression expr, QueryOperator queryOperator)
        {
            var leftConstantExpression = GetValueExpression(expr.Left);
            var rightConstantExpression = GetValueExpression(expr.Right);
            bool rightIsNull = (rightConstantExpression != null && rightConstantExpression.Value == null);
            bool leftIsNull = (leftConstantExpression != null && leftConstantExpression.Value == null);
            bool isNull = rightIsNull || leftIsNull;

            // if the right side is null, than change the query operator
            if (isNull)
            {
                switch (queryOperator)
                {
                    case QueryOperator.Equals:
                        queryOperator = QueryOperator.Is;
                        break;

                    case QueryOperator.NotEqual:
                        queryOperator = QueryOperator.IsNot;
                        break;
                }
            }

            // Select Query Operator
            switch (queryOperator)
            {
                case QueryOperator.Equals:
                    ComparerStack.Push(" = ");
                    break;
                case QueryOperator.NotEqual:
                    ComparerStack.Push(" <> ");
                    break;
                case QueryOperator.Lesser:
                    ComparerStack.Push(" < ");
                    break;
                case QueryOperator.Greater:
                    ComparerStack.Push(" > ");
                    break;
                case QueryOperator.LesserEqual:
                    ComparerStack.Push(" <= ");
                    break;
                case QueryOperator.GreaterEqual:
                    ComparerStack.Push(" >= ");
                    break;
                case QueryOperator.Is:
                    ComparerStack.Push(" IS ");
                    break;
                case QueryOperator.IsNot:
                    ComparerStack.Push(" IS NOT ");
                    break;
                case QueryOperator.In:
                    ComparerStack.Push(" IN ");
                    break;
                case QueryOperator.Like:
                    ComparerStack.Push(" LIKE ");
                    break;
                case QueryOperator.NotIn:
                    ComparerStack.Push(" NOT IN ");
                    break;
                case QueryOperator.NotLike:
                    ComparerStack.Push(" NOT LIKE ");
                    break;
                case QueryOperator.Like_NoCaseSensitive:
                    ComparerStack.Push(" LIKE ");
                    break;
                case QueryOperator.NotLike_NoCaseSensitive:
                    ComparerStack.Push(" NOT LIKE ");
                    break;
            }

            Visit(leftIsNull ? expr.Right : expr.Left);

            // In that special case of "Compare" return directly
            // Pop the comparison string
            if (ComparerStack.Count > 0)
            {
                WriteSql(ComparerStack.Pop());
                Visit(leftIsNull ? expr.Left : expr.Right);
            }

            return expr;
        }

        /// <summary>
        /// Visits the Join Expression
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            VisitJoinLeft(join.Left);
            switch (join.Join)
            {
                case JoinType.CrossJoin:
                    WriteSql(" CROSS JOIN ");
                    break;
                case JoinType.InnerJoin:
                    WriteSql(" INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    WriteSql(" CROSS APPLY ");
                    break;
                case JoinType.OuterApply:
                    WriteSql(" OUTER APPLY ");
                    break;
                case JoinType.LeftOuter:
                case JoinType.SingletonLeftOuter:
                    WriteSql(" LEFT OUTER JOIN ");
                    break;
            }

            VisitJoinRight(join.Right);

            if (join.Condition != null)
            {
                WriteSql(" ON ");
                VisitPredicate(join.Condition);
            }

            return join;
        }

        /// <summary>
        /// Visits the predicate.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        protected virtual Expression VisitPredicate(Expression expr)
        {
            Visit(expr);
            //if (!IsPredicate(expr))
            //{
            //    WriteSql(" <> 0");
            //}
            return expr;
        }

        /// <summary>
        /// Visits the union expression.
        /// </summary>
        /// <param name="union">The union.</param>
        /// <returns></returns>
        protected override Expression VisitUnionExpression(UnionExpression union)
        {
            if (Root != union) WriteSql(" (");
            Visit(union.First);
            WriteSql(" UNION ");
            if (union.UnionAll) WriteSql("ALL ");
            Visit(union.Second);
            if (Root != union) WriteSql(") ");
            return union;
        }

        /// <summary>
        /// Visits the join left.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        protected virtual Expression VisitJoinLeft(Expression source)
        {
            var subSelect = Visit(source) as IDbExpressionWithResult;
            if (subSelect != null)
                WriteSql(" " + subSelect.Alias.Name);

            return source;
        }

        /// <summary>
        /// Visits the join right.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        protected virtual Expression VisitJoinRight(Expression source)
        {
            var subSelect = Visit(source) as IDbExpressionWithResult;
            if (subSelect != null)
                WriteSql(" " + subSelect.Alias.Name);

            return source;
        }

        /// <summary>
        /// Determines whether the specified type is boolean.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is boolean; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsBoolean(Type type)
        {
            return type == typeof (bool) || type == typeof (bool?);
        }

        /// <summary>
        /// Determines whether the specified expr is predicate.
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>
        /// 	<c>true</c> if the specified expr is predicate; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsPredicate(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(expr.Type);
                case ExpressionType.Not:
                    return IsBoolean(expr.Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case (ExpressionType) DbExpressionType.Between:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(expr.Type);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Visits the sys date expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysDateExpression(SysDateExpression expression)
        {
            WriteSql("CAST(FLOOR(CAST(GETDATE() AS FLOAT)) AS DATETIME)");
            return base.VisitSysDateExpression(expression);
        }

        /// <summary>
        /// Visits the sys time expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected override Expression VisitSysTimeExpression(SysTimeExpression expression)
        {
            WriteSql("GETDATE()");
            return base.VisitSysTimeExpression(expression);
        }

        /// <summary>
        /// Adds a RowNum
        /// </summary>
        /// <param name="rowNumExpression"></param>
        /// <returns></returns>
        protected override Expression VisitRowNumExpression(RowNumExpression rowNumExpression)
        {
            WriteSql("ROWNUM");
            return rowNumExpression;
        }

        /// <summary>
        /// Writes an Cast Expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitCastExpression(CastExpression expression)
        {
            if (!LinqPersister.TypeMapper.DbMappingTable.ContainsKey(expression.TargetType))
                return base.VisitCastExpression(expression);

            WriteSql("CAST(");
            Visit(expression.Expression);
            WriteSql(" AS ");
            WriteSql(LinqPersister.TypeMapper.DbMappingTable[expression.TargetType]);
            WriteSql(")");

            return expression;
        }

        /// <summary>
        /// Write the select function epxression
        /// </summary>
        /// <param name="sfe"></param>
        /// <returns></returns>
        protected override Expression VisitSelectFunctionExpression(SelectFunctionExpression sfe)
        {
            WriteSql(sfe.Function);
            return sfe;
        }

        /// <summary>
        /// Visits the element initializer list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        protected override IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            var n = original.Count;

            for (int i = 0; i < n; i++)
            {
                if (i > 0)
                    WriteSql(", ");

                var init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;

            return original;
        }
    }
}