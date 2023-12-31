﻿using System;
using System.Linq.Expressions;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// PropertyMapping
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PropertyMapping(Type typeToFind, ColumnDeclaration columnToFind)
        {
            Type = typeToFind;
            Column = columnToFind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapping"/> class.
        /// </summary>
        /// <param name="typeToFind">The type to find.</param>
        /// <param name="columnToFind">The column to find.</param>
        /// <param name="foundExpression">The found expression.</param>
        public PropertyMapping(Type typeToFind, ColumnDeclaration columnToFind, AliasedExpression foundExpression)
            :this(typeToFind, columnToFind)
        {
            FromClause = foundExpression;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; private set;}

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        public ColumnDeclaration Column { get; private set;}

        /// <summary>
        /// Gets or sets from clause.
        /// </summary>
        /// <value>From clause.</value>
        public AliasedExpression FromClause { get; private set; }
    }

    /// <summary>
    /// TableExpressionFinder
    /// </summary>
    public class TableExpressionFinder : DbExpressionVisitor
    {
        private readonly PropertyMapping searchFor;
        private PropertyMapping foundMapping;

        /// <summary>
        /// Search for the table type
        /// </summary>
        private TableExpressionFinder(PropertyMapping searchForMapping)
        {
            searchFor = searchForMapping;
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="type">The type.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static PropertyMapping FindForType(Expression expression, Type type, ColumnDeclaration columnName)
        {
            var finder = new TableExpressionFinder(new PropertyMapping(type, columnName));
            finder.Visit(expression);
            return finder.foundMapping;
        }

        /// <summary>
        /// Replace the table expression with the surrounding select, if it covers the table expression
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (select.RevealedType == searchFor.Type)
            {
                foundMapping = new PropertyMapping(searchFor.Type, MapColumnName(select, 
                    searchFor.FromClause != null 
                    ? searchFor.FromClause.Alias.Name
                    : null, searchFor.Column), select);
                return select;
            }

            base.VisitSelectExpression(select);

            if (foundMapping != null)
                foundMapping = new PropertyMapping(foundMapping.Type, MapColumnName(select, 
                    foundMapping.FromClause != null 
                    ? foundMapping.FromClause.Alias.Name
                    : null, foundMapping.Column), select);
            
            return select;
        }

        /// <summary>
        /// Returns the alias name for the mapped column 
        /// </summary>
        private static ColumnDeclaration MapColumnName(SelectExpression expression, string tableAlias, ColumnDeclaration columnName)
        {
            foreach (var column in expression.Columns)
            {
                var property = ColumnProjector.FindAliasedExpression(column.Expression) as PropertyExpression;
                if (property != null && string.Equals(property.PropertyName, columnName.PropertyName, StringComparison.InvariantCultureIgnoreCase) 
                                     && (string.Equals(property.Alias.Name, tableAlias, StringComparison.InvariantCultureIgnoreCase) || tableAlias == null) )
                    return column;
            }

            return null;
        }

        /// <summary>
        /// Search for the table expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitTableExpression(TableExpression expression)
        {
            if (expression.RevealedType == searchFor.Type) 
                foundMapping = new PropertyMapping(searchFor.Type, searchFor.Column, expression);

            return base.VisitTableExpression(expression);
        }

        /// <summary>
        /// Overwrite the visit expression
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected override Expression Visit(Expression exp)
        {
            if (foundMapping != null) return exp;
            return base.Visit(exp);
        }
    }
}
