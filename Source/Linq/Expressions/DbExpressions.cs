using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// Defines the new type of database Expressions
    /// </summary>
    public enum DbExpressionType 
    {
        ///<summary> Start of the new Database Expression Type</summary>
        DbExpression = 999,

        /// <summary> Used to represent a column, which is always a property</summary>
        PropertyExpression ,

        /// <summary> Used to define a single constant value </summary>
        ValueExpression,

        /// <summary> Used to define a table definition for a from clause </summary>
        TableExpression,

        /// <summary> Definition of a SQL Select statement </summary>
        SelectExpression,

        /// <summary> Defines a value which is given to the database via an Parameter.
        /// The Opossite is the ValueExpression. </summary>
        SqlParameterExpression,

        /// <summary> Join Expression </summary>
        Join,
        
        /// <summary> Defines an aggregate expression, like max() or sum() </summary>
        Aggregate,
        
        /// <summary> Defines the rowcount expression, like rownum </summary>
        RowCount,

        /// <summary> Defines the between expression, like between</summary>
        Between,

        /// <summary> Defines a Union expression </summary>
        Union,

        /// <summary> Defines a RowNum Expression</summary>
        RowNum,

        /// <summary> Defines an Exists Expression </summary>
        Exists,
        
        /// <summary> Defines an Cast Expression </summary>
        Cast,

        /// <summary> Used to output the result of a selectfunction </summary>
        SelectFunction,
        
        /// <summary> Defines an aggregate expression that might be substituded by a subquery </summary>
        AggregateSubquery,

        /// <summary> Defines an order expression </summary>
        Ordering,

        /// <summary> Defines a sysdate expression </summary>
        SysDate,

        /// <summary> Defines a systime expression </summary>
        SysTime,
        ScalarExpression,
        LateBinding
    }

    /// <summary>
    /// Base Class for the new database expressions used for aggregating the linq expressions
    /// </summary>
    public abstract class DbExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbExpression"/> class.
        /// </summary>
        /// <param name="nodeType">The <see cref="T:System.Linq.Expressions.ExpressionType"/> to set as the node type.</param>
        /// <param name="type">The <see cref="T:System.Type"/> to set as the type of the expression that this <see cref="T:System.Linq.Expressions.Expression"/> represents.</param>
        protected DbExpression(DbExpressionType nodeType, Type type) : base((ExpressionType)nodeType, type)
        {
            RevealedType = type.RevealType();
        }

        /// <summary>
        /// Returns the revealed type 
        /// </summary>
        public Type RevealedType { get; private set;}
    }

    /// <summary>
    /// Aliased Expressions are used for all kinds of expression that can be substituded with an alias
    /// </summary>
    public abstract class AliasedExpression : DbExpression
    {
        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public Alias Alias { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasedExpression"/> class.
        /// </summary>
        /// <param name="nodeType">Type of the node.</param>
        /// <param name="type">The type.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="projection">The projection.</param>
        protected AliasedExpression(DbExpressionType nodeType, Type type, Alias alias, ProjectionClass projection) : base(nodeType, type)
        {
            Alias = alias;
            Projection = projection;
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>The projection.</value>
        public ProjectionClass Projection { get; protected set; }
    }

    /// <summary>
    /// This class is used, when the querybinder needs to evaluate a AliasedExpression, but can't do that, because of a late member binding
    /// </summary>
    public class LateBindingExpression : AliasedExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LateBindingExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public LateBindingExpression(Expression expression) 
            : base(DbExpressionType.LateBinding, expression.Type, null, null)
        {
            Binding = expression;
        }

        /// <summary> Gets or sets the expression. </summary>
        public Expression Binding { get; private set; }

        public override string ToString()
        {
            return Binding.ToString();
        }
    }


    /// <summary>
    /// This class represents a column, which is always mapped to a property
    /// </summary>
    public class PropertyExpression : AliasedExpression
    {
        /// <summary> Gets or sets the name of the property. </summary>
        public string PropertyName { get; private set; }

        private string name;

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public string Name
        {
            get
            {
                return ReferringColumn != null ? ReferringColumn.Alias.Name : name;
            }
            internal set
            {
                name = value ?? string.Empty;
            }
        }

        /// <summary> Gets or sets the type of the content. </summary>
        public Type ContentType { get; private set; }

        /// <summary> Gets or sets the type of the parent. </summary>
        public Type ParentType { get; private set; }

        /// <summary> Is set, if the property referes to an other alias member </summary>
        public ColumnDeclaration ReferringColumn { get; private set; }

        /// <summary> Gets or sets a value indicating whether this <see cref="PropertyExpression"/> is expandable. </summary>
        public bool Expandable { get { return ContentType.IsValueObjectType(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="copy">The copy.</param>
        internal PropertyExpression(PropertyExpression copy)
            : this(copy.Type, copy)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="expression">The expression.</param>
        internal PropertyExpression(Type type, PropertyExpression expression)
            : base(DbExpressionType.PropertyExpression, type, expression.Alias, expression.Projection)
        {
            PropertyName = expression.PropertyName;
            Name = expression.Name;
            ReferringColumn = expression.ReferringColumn;
            ContentType = expression.ContentType;
            ParentType = expression.ParentType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        public PropertyExpression(ProjectionClass projection, Alias tableAlias, PropertyInfo propInfo)
            : base(DbExpressionType.PropertyExpression, propInfo.PropertyType, tableAlias, projection)
        {
            var info = Internal.Property.GetPropertyInstance(propInfo);
            
            Name = info.MetaInfo.ColumnName;
            PropertyName = info.MetaInfo.PropertyName;

            ContentType = info.PropertyType;
            ParentType = RevealedType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="aliasedExpression">The aliased expression.</param>
        /// <param name="refferingColumn">The reffering column.</param>
        public PropertyExpression(AliasedExpression aliasedExpression, ColumnDeclaration refferingColumn)
            :this(aliasedExpression.Projection, aliasedExpression.Alias, refferingColumn)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="referringColumn">The referring alias.</param>
        public PropertyExpression(ProjectionClass projection, Alias tableAlias, ColumnDeclaration referringColumn)
            : base(DbExpressionType.PropertyExpression, referringColumn.Type, tableAlias, projection)
        {
            Name = null;
            ReferringColumn = referringColumn;
            ContentType = typeof(void);
            ParentType = RevealedType;
            PropertyName = referringColumn.PropertyName;
            
            var referringProperty = referringColumn.Expression as PropertyExpression;
            if (referringProperty != null)
            {
                PropertyName = referringProperty.PropertyName;
                ContentType = referringProperty.ContentType;
                ParentType = referringProperty.ParentType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="projection">The projection.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="field">The field.</param>
        public PropertyExpression(Type type, ProjectionClass projection, Alias tableAlias, FieldDescription field)
            : base(DbExpressionType.PropertyExpression, type, tableAlias, projection)
        {
            ContentType = field.ContentType;
            ParentType = field.ParentType;
            PropertyName = field.PropertyName;
            Name = field.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="copy">The copy.</param>
        public PropertyExpression(AliasedExpression table, PropertyExpression copy)
            :this(copy.Type, table.Projection, table.Alias, copy)
        {
            IDbExpressionWithResult result = table as IDbExpressionWithResult;
            if (result != null)
            {
                var original = OriginPropertyFinder.Find(copy) ?? copy;
                ReferringColumn = result.Columns.FirstOrDefault(x => original.Equals(x.OriginalProperty) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the property expression
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="projection">The projection.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="copy">The copy.</param>
        public PropertyExpression(Type type, ProjectionClass projection, Alias tableAlias, PropertyExpression copy)
            : base(DbExpressionType.PropertyExpression, type, tableAlias, projection)
        {
            ContentType = copy.ContentType;
            ParentType = copy.ParentType;
            PropertyName = copy.PropertyName;
            Name = copy.name;
            ReferringColumn = copy.ReferringColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="fromAlias">From alias.</param>
        /// <param name="column">The column.</param>
        public PropertyExpression(AliasedExpression fromAlias, PropertyInfo column) 
            :this(fromAlias.Projection, fromAlias.Alias, column)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="projection">The projection.</param>
        /// <param name="alias">The alias.</param>
        public PropertyExpression(Type expression, ProjectionClass projection, PropertyExpression alias) : this(expression, alias)
        {
            Projection = projection;
        }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return string.Concat(Alias.Name, ".", Name); 
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            PropertyExpression prop = obj as PropertyExpression;
            if (prop != null)
                return prop.ToString() == ToString();
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    /// <summary>
    /// A RowNum column is a special column of the database 
    /// </summary>
    public class RowNumberExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowNumberExpression"/> class.
        /// </summary>
        /// <param name="orderBy"></param>
        public RowNumberExpression(IList<OrderExpression> orderBy)
            : base(DbExpressionType.RowCount, typeof(int))
        {
            OrderBy = new ReadOnlyCollection<OrderExpression>(orderBy);
        }

        /// <summary> Gets or sets the OrderBy Collection </summary>
        public ReadOnlyCollection<OrderExpression> OrderBy { get; private set;}

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(" ROW_NUMBER() OVER( ORDER BY ");
            foreach (var orderBy in OrderBy)
                builder.Append(string.Concat(",", orderBy.Expression));
            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// Defines the Oracle RowNum Expression
    /// </summary>
    public class RowNumExpression : DbExpression
    {
        ///<summary>
        /// Constructor for the Oracle ROWNUM Expression
        /// </summary>
        public RowNumExpression()
            :base(DbExpressionType.RowNum, typeof(int))
        {
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "ROWNUM";
        }
    }

    /// <summary>
    /// SysDateExpression
    /// </summary>
    public class SysDateExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysDateExpression"/> class.
        /// </summary>
        public SysDateExpression()
            :base(DbExpressionType.SysDate, typeof(DateTime))
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "SYSDATE";
        }
    }

    /// <summary>
    /// SysDateExpression
    /// </summary>
    public class SysTimeExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysTimeExpression"/> class.
        /// </summary>
        public SysTimeExpression()
            : base(DbExpressionType.SysTime, typeof(DateTime))
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "SYSTIME";
        }
    }

    /// <summary>
    /// Defines a CAST Expression in order to convert data types
    /// </summary>
    public class CastExpression : DbExpression
    {
        public Expression Expression { get; private set; }
        public Type TargetType { get; private set; }
        
        /// <summary> Constructor </summary>
        public CastExpression(Expression expression, Type targetType)
            :base (DbExpressionType.Cast, expression.Type)
        {
            Expression = expression;
            TargetType = targetType;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "CAST(" + Expression+ " AS " + TargetType.Name + ")";
        }
    }

    /// <summary>
    /// Used to define a select function attribute as an expression
    /// </summary>
    public class SelectFunctionExpression : DbExpression
    {
        public string Function { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SelectFunctionExpression(Type type, string function)
            :base(DbExpressionType.SelectFunction, type)
        {
            Function = function;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return Function;
        }
    }

    /// <summary>
    /// Defines the SQL Exists Expression
    /// </summary>
    public class ExistsExpression : DbExpression
    {
        ///<summary>SubSelect for the exists expression</summary>
        public SelectExpression Selection { get; private set;}

        /// <summary>
        /// Constructor for the exists expression
        /// </summary>
        /// <param name="select"></param>
        public ExistsExpression(SelectExpression select)
            :base(DbExpressionType.Exists, typeof(bool))
        {
            Selection = select;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "EXISTS(" + Selection + ")";
        }
    }

    /// <summary>
    /// Defines a BETWEEN Sql Expression
    /// </summary>
    public class BetweenExpression : DbExpression
    {
        /// <summary>
        /// Initializes the Between Expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public BetweenExpression(Expression expression, Expression lower, Expression upper)
            : base(DbExpressionType.Between, expression.Type)
        {
            Expression = expression;
            Lower = lower;
            Upper = upper;
        }

        /// <summary>Gets or sets the expression </summary>
        public Expression Expression { get; private set;}

        /// <summary>Gets or sets the lower border </summary>
        public Expression Lower { get; private set; }

        /// <summary>Gets or sets the upper border </summary>
        public Expression Upper { get; private set;}

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return string.Concat(Expression.ToString(), " BETWEEN ", Lower.ToString(), " AND ", Upper.ToString());
        }
    }
    /// <summary>
    /// A column declaration is the result of a select statement, which can be either a PropertyExpression
    /// or a SelectExpression for example
    /// </summary>
    public class ColumnDeclaration 
    {
        /// <summary> Gets or sets the expression. </summary>
        public Expression Expression { get; private set; }

        /// <summary> Gets or sets the alias. </summary>
        public Alias Alias { get; internal set; }

        private string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDeclaration"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="alias">The alias.</param>
        public ColumnDeclaration(Expression expression, Alias alias) 
        {
            ParameterExpression parameter = expression as ParameterExpression;
            Expression = expression;
            Alias = alias;

            if (parameter != null)
                propertyName = parameter.Name;
            else
                propertyName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDeclaration"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        public ColumnDeclaration(Expression expression, Alias alias, string propertyName)
            :this(expression, alias)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDeclaration"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="col">The col.</param>
        public ColumnDeclaration(Expression expression, ColumnDeclaration col)
        {
            Expression = expression;
            Alias = col.Alias.Generated ? col.Alias : Alias.Generate(col.Alias.Name);
            propertyName = col.propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDeclaration"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="field">The field.</param>
        public ColumnDeclaration(Expression expression, FieldDescription field)
        {
            Expression = expression;
            Alias = Alias.Generate(field.Name);
            PropertyName = field.PropertyName;
        }

        private PropertyExpression originalProperty;

        /// <summary>
        /// Returns the original assigned property
        /// </summary>
        public PropertyExpression OriginalProperty
        {
            get
            {
                if (originalProperty != null)
                    return originalProperty;

                originalProperty = OriginPropertyFinder.Find(Expression);
                return originalProperty;
            }
        }

        /// <summary>
        /// Returns the Original Column Name (not the alias)
        /// </summary>
        public string PropertyName
        {
            get
            {
                var property = OriginalProperty;
                return property != null ? property.PropertyName : propertyName;
            }
            set
            {
                propertyName = value;
            }

        }

        /// <summary>
        /// Returns the original expression type
        /// </summary>
        public Type RevealedType
        {
             get
             {
                 var property = OriginalProperty;
                 return property != null ? property.RevealedType : null;
             }   
        }

        /// <summary>
        /// Returns an Key
        /// </summary>
        public string Key
        {
            get {return Expression != null ? Expression.ToString(): string.Empty;}
        }

        /// <summary>
        /// Returns the Expression Type
        /// </summary>
        public Type Type
        {
            get
            {
                if (Expression != null)
                    return Expression.Type;

                return null;
            }
        }

        /// <summary>
        /// Basic abstraction in order to understand the query a little better
        /// </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return string.Concat(Expression.ToString(), " as ", Alias.Name);
        }

        public override bool Equals(object obj)
        {
            ColumnDeclaration toCompare = obj as ColumnDeclaration;
            if (toCompare != null)
                return Expression.Equals(toCompare.Expression);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }
    }

    /// <summary>
    /// This class represents a single constant value
    /// </summary>
    public class ValueExpression : DbExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueExpression"/> class.
        /// </summary>
        public ValueExpression(Type contentType, object value)
            : base(DbExpressionType.ValueExpression, contentType)
        {
            Value = value;
        }

        /// <summary> Gets or sets the value. </summary>
        public object Value { get; private set; }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "null";
        }
    }

    /// <summary>
    /// This class represents a parameter value
    /// </summary>
    public class SqlParameterExpression : AliasedExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterExpression"/> class.
        /// </summary>
        /// <param name="parentProjection">Type of the parent.</param>
        /// <param name="value">The value.</param>
        public SqlParameterExpression(Type parentType, object value)
            : base(DbExpressionType.SqlParameterExpression, parentType, Alias.Generate(AliasType.Parameter), null)
        {
            Value = value;
            ContentType = value != null ? value.GetType() : typeof(void);
        }

        /// <summary>
        /// Create a named Sql Parameter Expression
        /// </summary>
        public SqlParameterExpression(Type parentType, object value, string alias)
            : base(DbExpressionType.SqlParameterExpression, parentType, Alias.Generate(alias), null)
        {
            Value = value;
            ContentType = value != null ? value.GetType() : typeof(void);
        }

        /// <summary> Gets or sets the value. </summary>
        public object Value { get; private set; }

        /// <summary> Gets or sets the type of the content. </summary>
        public Type ContentType { get; private set; }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return ":" + Alias.Name;  
        }
    }

    /// <summary>
    /// This expression defines the ordering
    /// </summary>
    public class OrderExpression : DbExpression
    {
        /// <summary> Gets or sets the ordering. </summary>
        public Ordering Ordering { get; private set; }

        /// <summary> Gets or sets the expression. </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderExpression"/> class.
        /// </summary>
        /// <param name="ordering">The ordering.</param>
        /// <param name="expression">The expression.</param>
        public OrderExpression(Ordering ordering, Expression expression)
            :base(DbExpressionType.Ordering, expression.Type)
        {
            Ordering = ordering;
            Expression = expression;
        }
    }

    /// <summary>
    /// Defines the result of the selection
    /// </summary>
    public enum SelectResultType
    {
        /// <summary> Returns a collection </summary>
        Collection,

        /// <summary> Returns a single object </summary>
        SingleObject,

        /// <summary> Returns a single object or default</summary>
        SingleObjectOrDefault,

        /// <summary> Returns a single aggregate </summary>
        SingleAggregate
    }

    /// <summary>
    /// Defines an expression that can be used as a result, like SelectExpression, TableExpression or UnionExpression
    /// </summary>
    public interface IDbExpressionWithResult 
    {
        /// <summary> Gets or sets the columns. </summary>
        ReadOnlyCollection<ColumnDeclaration> Columns { get;  }

        /// <summary> Gets or sets the alias. </summary>
        Alias Alias { get; set; }

        /// <summary> Gets the type of the revealed. </summary>
        Type RevealedType { get;  }

        /// <summary> Gets the projection. </summary>
        ProjectionClass Projection { get; }

        /// <summary> Gets from. </summary>
        IDbExpressionWithResult FromExpression { get;}

        /// <summary> Gets the default if empty. </summary>
        Expression DefaultIfEmpty { get; }

        /// <value>The selector.</value>
        Expression Selector { get; }
    }

    /// <summary>
    /// This expression returns one value, a scalar type as a result of an aggregation
    /// </summary>
    public class ScalarExpression : AliasedExpression, IDbExpressionWithResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarExpression"/> class.
        /// </summary>
        public ScalarExpression(Type type, Alias alias, ColumnDeclaration column, Expression selector, AliasedExpression from)
            :base(DbExpressionType.ScalarExpression, type, alias, from.Projection)
        {
            Columns = new ReadOnlyCollection<ColumnDeclaration>(new List<ColumnDeclaration>(){column});
            From = from;
            Selector = selector;
        }

        #region Implementation of IDbExpressionWithResult

        /// <summary> Gets or sets the columns. </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set;}

        /// <summary> Gets from. </summary>
        public IDbExpressionWithResult FromExpression
        {
            get { return From as IDbExpressionWithResult; }
        }

        /// <summary> Gets the default if empty. </summary>
        public Expression DefaultIfEmpty
        {
            get { return null;}
        }

        /// <summary> Gets or sets the From Clause Expression </summary>
        public Expression From { get; private set; }

        /// <summary> Gets or sets the selector. </summary>
        public Expression Selector { get; private set; }

        #endregion

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ");

            //if (!string.IsNullOrEmpty(SqlId))
            //    builder.Append("/* " + SqlId + "*/ ");

            //if (!string.IsNullOrEmpty(Hint))
            //    builder.Append("/*+ " + Hint + "*/ ");

            //if (IsDistinct)
            //    builder.Append("DISTINCT ");

            //if (Take != null)
            //{
            //    builder.Append("TOP ");
            //    builder.Append(Take.ToString());
            //    builder.Append(" ");
            //}

            if (Columns != null)
            {
                for (int x = 0; x < Columns.Count; x++)
                {
                    if (x > 0) builder.Append(", ");

                    var col = Columns[x];
                    builder.Append(col.ToString());
                }
            }

            if (From != null)
            {
                builder.Append(" FROM ");
                builder.Append(From.ToString());
            }

            //if (Where != null)
            //{
            //    builder.Append(" WHERE ");
            //    builder.Append(Where.ToString());
            //}

            //if (GroupBy != null)
            //{
            //    builder.Append(" GROUP BY ");
            //    for (int x = 0; x < GroupBy.Count; x++)
            //    {
            //        if (x > 0) builder.Append(", ");
            //        builder.Append(GroupBy[x].ToString());
            //    }
            //}

            //if (OrderBy != null)
            //{
            //    builder.Append(" ORDER BY ");
            //    for (int x = 0; x < OrderBy.Count; x++)
            //    {
            //        if (x > 0) builder.Append(", ");
            //        builder.Append(OrderBy[x].Expression.ToString());
            //        builder.Append(" ");
            //        builder.Append(OrderBy[x].Ordering.ToString());
            //    }
            //}
 

            return builder.ToString();
        }

    }

    /// <summary>
    /// This expression represents a SQL Select expression
    /// </summary>
    public class SelectExpression : AliasedExpression, IDbExpressionWithResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpression"/> class.
        /// </summary>
        internal SelectExpression(Type type, ProjectionClass projection, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns, Expression selector, AliasedExpression from, Expression where, ReadOnlyCollection<OrderExpression> orderBy, ReadOnlyCollection<Expression> groupBy, Expression skip, Expression take, bool isDistinct, bool isReverse, SelectResultType selectResult, string sqlId, string hint, Expression defaultIfEmpty) 
            : base(DbExpressionType.SelectExpression, type, alias, projection)
        {
            Columns = columns;
            IsDistinct = isDistinct;
            Selector = selector;
            From = from;
            Where = where;
            OrderBy = orderBy;
            GroupBy = groupBy;
            Take = take;
            Skip = skip;
            IsReverse = isReverse;
            SelectResult = selectResult;
            SqlId = sqlId;
            Hint = hint;
            DefaultIfEmpty = defaultIfEmpty;
        }

        /// <summary>
        /// Gets a value indicating whether [default if empty].
        /// </summary>
        /// <value><c>true</c> if [default if empty]; otherwise, <c>false</c>.</value>
        public Expression DefaultIfEmpty { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpression"/> class.
        /// </summary>
        public SelectExpression(Type type, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns, Expression selector, AliasedExpression from, Expression where, ReadOnlyCollection<OrderExpression> orderBy, ReadOnlyCollection<Expression> groupBy)
            : this(type, from != null ? from.Projection : null, alias, columns, selector, from, where, orderBy, groupBy, null, null, false, false, SelectResultType.Collection, null,null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpression"/> class.
        /// </summary>
        public SelectExpression(Type type, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns, Expression selector, AliasedExpression from, Expression where)
            : this(type, from != null ? from.Projection : null, alias, columns, selector, from, where, null, null, null, null, false, false, SelectResultType.Collection, null, null, null)
        {
        }

        public SelectExpression(Type type, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns, Expression selector, AliasedExpression from, Expression where, Expression defaultIfEmpty)
            : this(type, from != null ? from.Projection : null, alias, columns, selector, from, where, null, null, null, null, false, false, SelectResultType.Collection, null, null, defaultIfEmpty)
        {
        }

        /// <summary> Gets or sets the select columns. </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }

        /// <summary> Unique SQL identifier, used for SQL Replacement </summary>
        public string SqlId { get; private set; }

        /// <summary> A Hint is used to optimize a database query </summary>
        public string Hint { get; private set; }

        /// <summary> True, if the select shall return distinct values </summary>
        public bool IsDistinct { get; private set; }

        /// <summary> Gets or sets the From Clause Expression </summary>
        public AliasedExpression From { get; private set; }

        /// <summary> Gets or sets the Where Clause Expression </summary>
        public Expression Where { get; private set; }

        /// <summary> Gets or sets the Selector of the selection </summary>
        public Expression Selector { get; private set; }

        /// <summary> Gets or sets the OrderBy Expressions </summary>
        public ReadOnlyCollection<OrderExpression> OrderBy { get; private set; }

        /// <summary> Gets or sets the GroupBy Expression </summary>
        public ReadOnlyCollection<Expression> GroupBy { get; private set; }
        
        /// <summary> If the select is paged, the Take Expression defines the amount of rows to take </summary>
        public Expression Take { get; private set; }

        /// <summary> If the select is paged, the Skip Expression defines the amount of rows to skip </summary>
        public Expression Skip { get; private set; }

        /// <summary> True, if the ordering shall be reversed. </summary>
        public bool IsReverse { get; private set; }

        /// <summary> Gets a value indicating whether this instance is single. </summary>
        public SelectResultType SelectResult { get; private set; }

        /// <summary> Gets from. </summary>
        public IDbExpressionWithResult FromExpression
        {
            get { return From as IDbExpressionWithResult; }
        }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            bool notFirst = true;

            if (notFirst) builder.Append("(");
            builder.Append("SELECT ");

            if (!string.IsNullOrEmpty(SqlId))
                builder.Append("/* " + SqlId + "*/ ");

            if (!string.IsNullOrEmpty(Hint))
                builder.Append("/*+ " + Hint + "*/ ");

            if (IsDistinct)
                builder.Append("DISTINCT ");

            if (Take != null)
            {
                builder.Append("TOP ");
                builder.Append(Take.ToString());
                builder.Append(" ");
            }

            if (Columns != null)
            {
                for (int x = 0; x < Columns.Count; x++)
                {
                    if (x > 0) builder.Append(", ");

                    var col = Columns[x];
                    builder.Append(col.ToString());
                }
            }

            if (From != null)
            {
                builder.Append(" FROM ");
                builder.Append(From.ToString());
            }

            if (Where != null)
            {
                builder.Append(" WHERE ");
                builder.Append(Where.ToString());
            }

            if (GroupBy != null)
            {
                builder.Append(" GROUP BY ");
                for (int x = 0; x < GroupBy.Count; x++)
                {
                    if (x > 0) builder.Append(", ");
                    builder.Append(GroupBy[x].ToString());
                }
            }

            if (OrderBy != null)
            {
                builder.Append(" ORDER BY ");
                for (int x = 0; x < OrderBy.Count; x++)
                {
                    if (x > 0) builder.Append(", ");
                    builder.Append(OrderBy[x].Expression.ToString());
                    builder.Append(" ");
                    builder.Append(OrderBy[x].Ordering.ToString());
                }
            }
            if (notFirst)
            {
                builder.Append(") ");
                builder.Append(Alias.Name);
            }

            return builder.ToString();
        }
    }

    public class AggregateSubqueryExpression : AliasedExpression
    {
        public AggregateSubqueryExpression(Alias groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
            : base(DbExpressionType.AggregateSubquery, aggregateAsSubquery.Type, groupByAlias, aggregateAsSubquery.Projection)
        {
            AggregateInGroupSelect = aggregateInGroupSelect;
            //GroupByAlias = groupByAlias;
            AggregateAsSubquery = aggregateAsSubquery;
        }
        //public Alias GroupByAlias { get; private set; }
        public Expression AggregateInGroupSelect { get; private set; }
        public ScalarExpression AggregateAsSubquery { get; private set; }
    }

    /// <summary>
    /// A kind of SQL join
    /// </summary>
    public enum JoinType
    {
        /// <summary> CrossJoin </summary>
        CrossJoin,

        /// <summary> InnerJoin </summary>
        InnerJoin,

        /// <summary> Cross Apply </summary>
        CrossApply,

        /// <summary> Outer Apply </summary>
        OuterApply,

        /// <summary> Left Outer </summary>
        LeftOuter,

        /// <summary> Singleton Left Outer </summary>
        SingletonLeftOuter
    }

    /// <summary>
    /// A custom expression node representing a Union expression
    /// </summary>
    public class UnionExpression : AliasedExpression, IDbExpressionWithResult
    {
        /// <summary>
        /// Constructor call
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="projection">The projection.</param>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="unionAll">if set to <c>true</c> [union all].</param>
        public UnionExpression(Type resultType, ProjectionClass projection, Expression first, Expression second, Alias alias, ReadOnlyCollection<ColumnDeclaration> columns, bool unionAll)
            : base(DbExpressionType.Union, resultType, alias, projection)
        {
            First = first;
            Second = second;
            UnionAll = unionAll;
            Columns = columns;
        }

        /// <summary> First union expression </summary>
        public Expression First { get; private set; }

        /// <summary> Second union expression  </summary>
        public Expression Second { get; private set; }

        /// <summary> Union All TRUE or FALSE </summary>
        public bool UnionAll { get; private set; }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(First);
            builder.Append(" UNION ");
            if (UnionAll) builder.Append("ALL ");
            builder.Append(Second);
            return builder.ToString();
        }

        #region Implementation of IDbExpressionWithResult

        /// <summary> Gets or sets the select columns. </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }

        /// <summary> Gets from. </summary>
        public IDbExpressionWithResult FromExpression
        {
            get { return null; }
        }

        /// <summary> Gets the default if empty. </summary>
        public Expression DefaultIfEmpty
        {
            get { return null;}
        }

        /// <value>The selector.</value>
        public Expression Selector
        {
            get { return null;}
        }

        #endregion
    }


    /// <summary>
    /// A custom expression node representing a SQL join clause
    /// </summary>
    public class JoinExpression : AliasedExpression, IDbExpressionWithResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoinExpression"/> class.
        /// </summary>
        public JoinExpression(Type resultType, ProjectionClass projection, JoinType joinType, Expression left, Expression right, Expression condition)
            : base(DbExpressionType.Join, resultType, Alias.Generate(AliasType.Join), projection)
        {
            Join = joinType;
            Left = left;
            Right = right;
            Condition = condition;

            // Only the outer Apply and Cross Apply have an Alias
            if (joinType != JoinType.CrossApply && joinType != JoinType.OuterApply)
                Alias = Alias.Generate("");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinExpression"/> class.
        /// </summary>
        public JoinExpression(Type resultType, ProjectionClass projection, JoinType joinType, Expression left, Expression right, Expression condition, string joinName)
            : base(DbExpressionType.Join, resultType, Alias.Generate(joinName), projection)
        {
            Join = joinType;
            Left = left;
            Right = right;
            Condition = condition;

            // Only the outer Apply and Cross Apply have an Alias
            if (joinType != JoinType.CrossApply && joinType != JoinType.OuterApply)
                Alias = Alias.Generate("");
        }

        /// <summary> Gets or sets the join. </summary>
        public JoinType Join { get; private set; }

        /// <summary> Gets or sets the left. </summary>
        public Expression Left { get; private set; }

        /// <summary> Gets or sets the right. </summary>
        public Expression Right { get; private set; }

        /// <summary> Gets or sets the condition. </summary>
        public new Expression Condition { get; private set; }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Left);
            switch (Join)
            {
                case JoinType.CrossJoin:
                    builder.Append(" CROSS JOIN ");
                    break;
                case JoinType.InnerJoin:
                    builder.Append(" INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    builder.Append(" CROSS APPLY ");
                    break;
                case JoinType.OuterApply:
                    builder.Append(" OUTER APPLY ");
                    break;
                case JoinType.LeftOuter:
                case JoinType.SingletonLeftOuter:
                    builder.Append(" LEFT OUTER JOIN ");
                    break;
            }
            builder.Append(Right);
            if (Condition != null)
            {
                builder.Append(" ON ");
                builder.Append(Condition);
            }
            return builder.ToString();
        }

        /// <summary> Gets from. </summary>
        public IDbExpressionWithResult FromExpression
        {
            get { return null; }
        }

        /// <summary> Gets the default if empty. </summary>
        public Expression DefaultIfEmpty
        {
            get { return null;}
        }

        /// <value>The selector.</value>
        public Expression Selector
        {
            get { return null;}
        }

        #region Implementation of IDbExpressionWithResult

        /// <summary> Gets or sets the columns. </summary>
        public ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get
            {
                var rightColumns = ((IDbExpressionWithResult) Right).Columns;
                var leftColumns = ((IDbExpressionWithResult) Left).Columns;
                ColumnDeclaration[] result = new ColumnDeclaration[rightColumns.Count + leftColumns.Count];
                rightColumns.CopyTo(result,0);
                leftColumns.CopyTo(result, rightColumns.Count);
                return new ReadOnlyCollection<ColumnDeclaration>(result);
            }
        }

        #endregion
    }

    /// <summary>
    /// This class covers an aggregate expression
    /// </summary>
    public class AggregateExpression : DbExpression
    {
        /// <summary> Gets or sets the aggregate name </summary>
        public string AggregateName { get; private set;}
        
        /// <summary> Gets or sets the Argument </summary>
        public Expression Argument { get; private set; }

        /// <summary> True, if the aggregate expression is distinct </summary>
        public bool IsDistinct { get; private set; }

        /// <summary>
        /// Constructor of the aggregate expression
        /// </summary>
        public AggregateExpression(Type type, string aggregateName, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            AggregateName = aggregateName;
            Argument = argument;
            IsDistinct = isDistinct;
        }

        /// <summary> Returns an abstract query text </summary>
        [DebuggerStepThrough]
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(AggregateName);
            builder.Append("(");
            if (IsDistinct) builder.Append("DISTINCT ");
            if (Argument != null)
                builder.Append(Argument.ToString());
            else
                builder.Append("*");
            builder.Append(")");
            return builder.ToString();
        }
    }
}