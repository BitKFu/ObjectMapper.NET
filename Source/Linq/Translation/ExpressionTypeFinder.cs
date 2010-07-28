using System;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    public class ExpressionTypeFinder : DbExpressionVisitor
    {
        private ExpressionTypeFinder(ExpressionType type) 
        {
            ExpressionTypeToFind = type;
        }

        private ExpressionTypeFinder(Type type)
        {
            TypeToFind = type;
        }

        public Expression Expression { get; private set; }
        public ExpressionType? ExpressionTypeToFind { get; private set;}
        public Type TypeToFind { get; private set; }

        public static Expression Find(Expression toSearchIn, Type type)
        {
            var opf = new ExpressionTypeFinder(type);
            opf.Visit(toSearchIn);
            return opf.Expression;
        }

        public static Expression Find(Expression toSearchIn, ExpressionType type)
        {
            var opf = new ExpressionTypeFinder(type);
            opf.Visit(toSearchIn);
            return opf.Expression;
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null) 
                return exp;

            if (exp.NodeType == ExpressionTypeToFind || (TypeToFind != null && exp.GetType().IsDerivedFrom(TypeToFind)))
                Expression = exp;

            return Expression != null ? exp : base.Visit(exp);
        }
    }
}
