using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExpressionExtensions
    {
        public static Expression Equal(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.Equal(expression1, expression2);
        }

        public static Expression NotEqual(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.NotEqual(expression1, expression2);
        }

        public static Expression GreaterThan(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.GreaterThan(expression1, expression2);
        }

        public static Expression GreaterThanOrEqual(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.GreaterThanOrEqual(expression1, expression2);
        }

        public static Expression LessThan(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.LessThan(expression1, expression2);
        }

        public static Expression LessThanOrEqual(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.LessThanOrEqual(expression1, expression2);
        }

        public static Expression AndAlso(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.AndAlso(expression1, expression2);
        }

        public static Expression OrElse(this Expression expression1, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.OrElse(expression1, expression2);
        }

        public static Expression Binary(this Expression expression1, ExpressionType op, Expression expression2)
        {
            ConvertExpressions(ref expression1, ref expression2);
            return Expression.MakeBinary(op, expression1, expression2);
        }

        private static void ConvertExpressions(ref Expression expression1, ref Expression expression2)
        {
            // Remove lambda if necessary
            var lambda1 = expression1 as LambdaExpression;
            if (lambda1 != null) expression1 = lambda1.Body;

            var lambda2 = expression2 as LambdaExpression;
            if (lambda2 != null) expression2 = lambda2.Body;

            // Try to convert types
            if (expression1.Type != expression2.Type)
            {
                var isNullable1 = expression1.Type.IsNullableType();
                var isNullable2 = expression2.Type.IsNullableType();
                if (isNullable1 || isNullable2)
                {
                    var type1 = isNullable1 ? Nullable.GetUnderlyingType(expression1.Type) : expression1.Type;
                    var type2 = isNullable1 ? Nullable.GetUnderlyingType(expression2.Type) : expression2.Type;

                    if (type1 == type2)
                    {
                        if (!isNullable1)
                        {
                            expression1 = Expression.Convert(expression1, expression2.Type);
                        }
                        else if (!isNullable2)
                        {
                            expression2 = Expression.Convert(expression2, expression1.Type);
                        }
                    }
                }
            }
        }

        public static Expression[] Split(this Expression expression, params ExpressionType[] binarySeparators)
        {
            var list = new List<Expression>();
            Split(expression, list, binarySeparators);
            return list.ToArray();
        }

        private static void Split(Expression expression, List<Expression> list, ExpressionType[] binarySeparators)
        {
            if (expression != null)
            {
                if (binarySeparators.Contains(expression.NodeType))
                {
                    var bex = expression as BinaryExpression;
                    if (bex != null)
                    {
                        Split(bex.Left, list, binarySeparators);
                        Split(bex.Right, list, binarySeparators);
                    }
                }
                else
                {
                    list.Add(expression);
                }
            }
        }

        public static Expression Join(this IEnumerable<Expression> list, ExpressionType binarySeparator)
        {
            if (list != null)
            {
                var array = list.ToArray();
                if (array.Length > 0)
                {
                    return array.Aggregate((x1, x2) => Expression.MakeBinary(binarySeparator, x1, x2));
                }
            }
            return null;
        }

        public static Collection<T> AddRange<T>(this Collection<T> result, ICollection<T> toAdd)
        {
            foreach (var add in toAdd)
                result.Add(add);

            return result;
        }

        /// <summary>
        /// Evaluates the real name ot of an expression
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string GetParameterName (this NewExpression ex, int argument)
        {
            return ex.Constructor.GetParameters()[argument].Name;
        }
    }
}