using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Queries;

namespace AdFactum.Data.Linq
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ExpressionVisitor
    {
#if TRACE
        protected static int Deep = 1;
#endif

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;

            try
            {
#if TRACE
                Console.WriteLine(new string(' ', Deep*2) + exp.NodeType + ": " + exp);
                Deep++;
#endif
                switch (exp.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    case ExpressionType.UnaryPlus:
                        return VisitUnary((UnaryExpression) exp);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.Power:
                        return VisitBinary((BinaryExpression) exp);

                    case ExpressionType.Equal:
                        return VisitComparison((BinaryExpression)exp, QueryOperator.Equals);

                    case ExpressionType.GreaterThan:
                        return VisitComparison((BinaryExpression)exp, QueryOperator.Greater);

                    case ExpressionType.GreaterThanOrEqual:
                        return VisitComparison((BinaryExpression)exp, QueryOperator.GreaterEqual);

                    case ExpressionType.LessThan:
                        return VisitComparison((BinaryExpression)exp, QueryOperator.Lesser);

                    case ExpressionType.LessThanOrEqual:
                        return VisitComparison((BinaryExpression)exp, QueryOperator.LesserEqual);

                    case ExpressionType.NotEqual:
                        return VisitComparison((BinaryExpression) exp, QueryOperator.NotEqual);

                    case ExpressionType.TypeIs:
                        return VisitTypeIs((TypeBinaryExpression) exp);
                    case ExpressionType.Conditional:
                        return VisitConditional((ConditionalExpression) exp);
                    case ExpressionType.Constant:
                        return VisitConstant((ConstantExpression) exp);
                    case ExpressionType.Parameter:
                        return VisitParameter((ParameterExpression) exp);
                    case ExpressionType.MemberAccess:
                        return VisitMemberAccess((MemberExpression) exp);
                    case ExpressionType.Call:
                        return VisitMethodCall((MethodCallExpression) exp);
                    case ExpressionType.Lambda:
                        return VisitLambda((LambdaExpression) exp);
                    case ExpressionType.New:
                        return VisitNew((NewExpression) exp);
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        return VisitNewArray((NewArrayExpression) exp);
                    case ExpressionType.Invoke:
                        return VisitInvocation((InvocationExpression) exp);
                    case ExpressionType.MemberInit:
                        return VisitMemberInit((MemberInitExpression) exp);
                    case ExpressionType.ListInit:
                        return VisitListInit((ListInitExpression) exp);
                    default:
                        return VisitUnknown(exp);
                }
            }
            finally
            {
#if TRACE
                Deep--;
#endif
            }
        }

        /// <summary>
        /// Visits the unknown.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected virtual Expression VisitUnknown(Expression expression)
        {
            throw new Exception(string.Format("Unhandled expression type: '{0}'", expression.NodeType));
        }

        /// <summary>
        /// Visits the binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns></returns>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment) binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding) binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding) binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        /// <summary>
        /// Visits the element initializer.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <returns></returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        /// <summary>
        /// Visits the unary.
        /// </summary>
        /// <param name="u">The u.</param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = Visit(u.Operand);
            return UpdateUnary(u, operand, u.Type, u.Method);
        }

        /// <summary>
        /// Updates the unary.
        /// </summary>
        /// <param name="u">The u.</param>
        /// <param name="operand">The operand.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        protected UnaryExpression UpdateUnary(UnaryExpression u, Expression operand, Type resultType, MethodInfo method)
        {
            if (u.Operand != operand || u.Type != resultType || u.Method != method)
            {
                return Expression.MakeUnary(u.NodeType, operand, resultType, method);
            }
            return u;
        }

        /// <summary>
        /// Visits the binary.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);
            Expression conversion = Visit(b.Conversion);
            return UpdateBinary(b, left, right, conversion, b.IsLiftedToNull, b.Method);
        }

        /// <summary>
        /// Visits the comparison.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="queryOperator">The query operator.</param>
        /// <returns></returns>
        protected virtual Expression VisitComparison(BinaryExpression b, QueryOperator queryOperator)
        {
            Expression left = Visit(b.Left);
            Expression right = Visit(b.Right);
            Expression conversion = Visit(b.Conversion);
            return UpdateBinary(b, left, right, conversion, b.IsLiftedToNull, b.Method);
        }

        /// <summary>
        /// Updates the binary.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="conversion">The conversion.</param>
        /// <param name="isLiftedToNull">if set to <c>true</c> [is lifted to null].</param>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        protected BinaryExpression UpdateBinary(BinaryExpression b, Expression left, Expression right,
                                                Expression conversion, bool isLiftedToNull, MethodInfo method)
        {
            if (left != b.Left || right != b.Right || conversion != b.Conversion || method != b.Method ||
                isLiftedToNull != b.IsLiftedToNull)
            {
                return b.NodeType == ExpressionType.Coalesce && b.Conversion != null
                           ? Expression.Coalesce(left, right, conversion as LambdaExpression)
                           : Expression.MakeBinary(b.NodeType, left, right, isLiftedToNull, method);
            }
            return b;
        }

        /// <summary>
        /// Visits the type is.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expr = Visit(b.Expression);
            return UpdateTypeIs(b, expr, b.TypeOperand);
        }

        /// <summary>
        /// Updates the type is.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="typeOperand">The type operand.</param>
        /// <returns></returns>
        protected TypeBinaryExpression UpdateTypeIs(TypeBinaryExpression b, Expression expression, Type typeOperand)
        {
            if (expression != b.Expression || typeOperand != b.TypeOperand)
            {
                return Expression.TypeIs(expression, typeOperand);
            }
            return b;
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        /// <summary>
        /// Visits the conditional.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = Visit(c.Test);
            Expression ifTrue = Visit(c.IfTrue);
            Expression ifFalse = Visit(c.IfFalse);
            return UpdateConditional(c, test, ifTrue, ifFalse);
        }

        /// <summary>
        /// Updates the conditional.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="test">The test.</param>
        /// <param name="ifTrue">If true.</param>
        /// <param name="ifFalse">If false.</param>
        /// <returns></returns>
        protected ConditionalExpression UpdateConditional(ConditionalExpression c, Expression test, Expression ifTrue,
                                                          Expression ifFalse)
        {
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }

        /// <summary>
        /// Visits the parameter.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        /// <summary>
        /// Visits the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression exp = Visit(m.Expression);
            return UpdateMemberAccess(m, exp, m.Member);
        }

        /// <summary>
        /// Updates the member access.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        protected MemberExpression UpdateMemberAccess(MemberExpression m, Expression expression, MemberInfo member)
        {
            if (expression != m.Expression || member != m.Member)
            {
                return Expression.MakeMemberAccess(expression, member);
            }
            return m;
        }

        /// <summary>
        /// Visits the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression obj = Visit(m.Object);
            IEnumerable<Expression> args = VisitExpressionList(m.Arguments);
            return UpdateMethodCall(m, obj, m.Method, args);
        }

        /// <summary>
        /// Updates the method call.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="obj">The obj.</param>
        /// <param name="method">The method.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected MethodCallExpression UpdateMethodCall(MethodCallExpression m, Expression obj, MethodInfo method,
                                                        IEnumerable<Expression> args)
        {
            if (obj != m.Object || method != m.Method || args != m.Arguments)
            {
                return Expression.Call(obj, method, args);
            }
            return m;
        }

        /// <summary>
        /// Visits the expression list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original != null)
            {
                List<Expression> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    Expression p = Visit(original[i]);
                    if (list != null)
                    {
                        list.Add(p);
                    }
                    else if (p != original[i])
                    {
                        list = new List<Expression>(n);
                        for (int j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }
                        list.Add(p);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return original;
        }

        /// <summary>
        /// Visits the member assignment.
        /// </summary>
        /// <param name="assignment">The assignment.</param>
        /// <returns></returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = Visit(assignment.Expression);
            return UpdateMemberAssignment(assignment, assignment.Member, e);
        }

        /// <summary>
        /// Updates the member assignment.
        /// </summary>
        /// <param name="assignment">The assignment.</param>
        /// <param name="member">The member.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected MemberAssignment UpdateMemberAssignment(MemberAssignment assignment, MemberInfo member,
                                                          Expression expression)
        {
            if (expression != assignment.Expression || member != assignment.Member)
            {
                return Expression.Bind(member, expression);
            }
            return assignment;
        }

        /// <summary>
        /// Visits the member member binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns></returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(binding.Bindings);
            return UpdateMemberMemberBinding(binding, binding.Member, bindings);
        }

        /// <summary>
        /// Updates the member member binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="member">The member.</param>
        /// <param name="bindings">The bindings.</param>
        /// <returns></returns>
        protected MemberMemberBinding UpdateMemberMemberBinding(MemberMemberBinding binding, MemberInfo member,
                                                                IEnumerable<MemberBinding> bindings)
        {
            if (bindings != binding.Bindings || member != binding.Member)
            {
                return Expression.MemberBind(member, bindings);
            }
            return binding;
        }

        /// <summary>
        /// Visits the member list binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns></returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(binding.Initializers);
            return UpdateMemberListBinding(binding, binding.Member, initializers);
        }

        /// <summary>
        /// Updates the member list binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="member">The member.</param>
        /// <param name="initializers">The initializers.</param>
        /// <returns></returns>
        protected MemberListBinding UpdateMemberListBinding(MemberListBinding binding, MemberInfo member,
                                                            IEnumerable<ElementInit> initializers)
        {
            if (initializers != binding.Initializers || member != binding.Member)
            {
                return Expression.ListBind(member, initializers);
            }
            return binding;
        }

        /// <summary>
        /// Visits the binding list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        /// <summary>
        /// Visits the element initializer list.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns></returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
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

        /// <summary>
        /// Visits the lambda.
        /// </summary>
        /// <param name="lambda">The lambda.</param>
        /// <returns></returns>
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = Visit(lambda.Body);
            return UpdateLambda(lambda, lambda.Type, body, lambda.Parameters);
        }

        /// <summary>
        /// Updates the lambda.
        /// </summary>
        /// <param name="lambda">The lambda.</param>
        /// <param name="delegateType">Type of the delegate.</param>
        /// <param name="body">The body.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected LambdaExpression UpdateLambda(LambdaExpression lambda, Type delegateType, Expression body,
                                                IEnumerable<ParameterExpression> parameters)
        {
            if (body != lambda.Body || parameters != lambda.Parameters || delegateType != lambda.Type)
            {
                return Expression.Lambda(delegateType, body, parameters);
            }
            return lambda;
        }

        /// <summary>
        /// Visits the new.
        /// </summary>
        /// <param name="nex">The nex.</param>
        /// <returns></returns>
        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            return UpdateNew(nex, nex.Constructor, args, nex.Members);
        }

        /// <summary>
        /// Updates the new.
        /// </summary>
        /// <param name="nex">The nex.</param>
        /// <param name="constructor">The constructor.</param>
        /// <param name="args">The args.</param>
        /// <param name="members">The members.</param>
        /// <returns></returns>
        protected NewExpression UpdateNew(NewExpression nex, ConstructorInfo constructor, IEnumerable<Expression> args,
                                          IEnumerable<MemberInfo> members)
        {
            if (args != nex.Arguments || constructor != nex.Constructor || members != nex.Members)
            {
                return nex.Members != null ? Expression.New(constructor, args, members) : Expression.New(constructor, args);
            }
            return nex;
        }

        /// <summary>
        /// Visits the member init.
        /// </summary>
        /// <param name="init">The init.</param>
        /// <returns></returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            return UpdateMemberInit(init, n, bindings);
        }

        /// <summary>
        /// Updates the member init.
        /// </summary>
        /// <param name="init">The init.</param>
        /// <param name="nex">The nex.</param>
        /// <param name="bindings">The bindings.</param>
        /// <returns></returns>
        protected MemberInitExpression UpdateMemberInit(MemberInitExpression init, NewExpression nex,
                                                        IEnumerable<MemberBinding> bindings)
        {
            if (nex != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(nex, bindings);
            }
            return init;
        }

        /// <summary>
        /// Visits the list init.
        /// </summary>
        /// <param name="init">The init.</param>
        /// <returns></returns>
        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(init.Initializers);
            return UpdateListInit(init, n, initializers);
        }

        /// <summary>
        /// Updates the list init.
        /// </summary>
        /// <param name="init">The init.</param>
        /// <param name="nex">The nex.</param>
        /// <param name="initializers">The initializers.</param>
        /// <returns></returns>
        protected ListInitExpression UpdateListInit(ListInitExpression init, NewExpression nex,
                                                    IEnumerable<ElementInit> initializers)
        {
            if (nex != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(nex, initializers);
            }
            return init;
        }

        /// <summary>
        /// Visits the new array.
        /// </summary>
        /// <param name="na">The na.</param>
        /// <returns></returns>
        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(na.Expressions);
            return UpdateNewArray(na, na.Type, exprs);
        }

        /// <summary>
        /// Updates the new array.
        /// </summary>
        /// <param name="na">The na.</param>
        /// <param name="arrayType">Type of the array.</param>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        protected NewArrayExpression UpdateNewArray(NewArrayExpression na, Type arrayType,
                                                    IEnumerable<Expression> expressions)
        {
            if (expressions != na.Expressions || na.Type != arrayType)
            {
                return na.NodeType == ExpressionType.NewArrayInit 
                    ? Expression.NewArrayInit(arrayType.GetElementType(), expressions) 
                    : Expression.NewArrayBounds(arrayType.GetElementType(), expressions);
            }
            return na;
        }

        /// <summary>
        /// Visits the invocation.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = VisitExpressionList(iv.Arguments);
            Expression expr = Visit(iv.Expression);
            return UpdateInvocation(iv, expr, args);
        }

        /// <summary>
        /// Updates the invocation.
        /// </summary>
        /// <param name="iv">The iv.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected InvocationExpression UpdateInvocation(InvocationExpression iv, Expression expression,
                                                        IEnumerable<Expression> args)
        {
            if (args != iv.Arguments || expression != iv.Expression)
            {
                return Expression.Invoke(expression, args);
            }
            return iv;
        }
    }
}