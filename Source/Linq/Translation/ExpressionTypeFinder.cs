using System;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// ExpressionTypeFinder
    /// </summary>
    public class ExpressionTypeFinder : DbExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTypeFinder"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        private ExpressionTypeFinder(ExpressionType type) 
        {
            ExpressionTypeToFind = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTypeFinder"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        private ExpressionTypeFinder(Type type)
        {
            TypeToFind = type;
        }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression Expression { get; private set; }

        /// <summary>
        /// Gets or sets the expression type to find.
        /// </summary>
        /// <value>The expression type to find.</value>
        public ExpressionType? ExpressionTypeToFind { get; private set;}

        /// <summary>
        /// Gets or sets the type to find.
        /// </summary>
        /// <value>The type to find.</value>
        public Type TypeToFind { get; private set; }

        /// <summary>
        /// Finds the specified to search in.
        /// </summary>
        /// <param name="toSearchIn">To search in.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Expression Find(Expression toSearchIn, Type type)
        {
            var opf = new ExpressionTypeFinder(type);
            opf.Visit(toSearchIn);
            return opf.Expression;
        }

        /// <summary>
        /// Finds the specified to search in.
        /// </summary>
        /// <param name="toSearchIn">To search in.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Expression Find(Expression toSearchIn, ExpressionType type)
        {
            var opf = new ExpressionTypeFinder(type);
            opf.Visit(toSearchIn);
            return opf.Expression;
        }

        /// <summary>
        /// Visits the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
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
