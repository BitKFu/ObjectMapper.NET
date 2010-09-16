using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// This class represents a single table. 
    /// It's generated out of the first constant expression in a main linq branch
    /// </summary>
    public class TableExpression : AliasedExpression, IDbExpressionWithResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableExpression"/> class.
        /// </summary>
        public TableExpression(Type type, ProjectionClass projection, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns)
            : base(DbExpressionType.TableExpression, type, alias, projection)
        {
            Cache<Type, ProjectionClass> cache = new Cache<Type, ProjectionClass>("Temp TableExpression");
            cache.Insert(type, projection);

            Columns = columns;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableExpression"/> class.
        /// </summary>
        public TableExpression(Type type, ProjectionClass projection, Alias alias) 
            : base(DbExpressionType.TableExpression, type, alias, projection)
        {
            Cache<Type, ProjectionClass> cache = new Cache<Type, ProjectionClass>("Temp TableExpression");
            cache.Insert(type, projection);

            Columns = ColumnProjector.Evaluate(this, cache);
        }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var table = Table.GetTableInstance(RevealedType);
            return string.Concat(table.DefaultName, " ", Alias.Name);
        }

        /// <summary> Gets or sets the select columns. </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }

        /// <summary> Gets from. </summary>
        public IDbExpressionWithResult[] FromExpression
        {
            get { return null;}
        }

        /// <summary> Gets the default if empty. </summary>
        public Expression DefaultIfEmpty
        {
            get { return null; }
        }

        /// <value>The selector.</value>
        public Expression Selector
        {
            get { return null;}
        }
    }
}