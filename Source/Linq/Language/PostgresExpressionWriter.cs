using System;
using System.Collections.Generic;
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

    }
}
