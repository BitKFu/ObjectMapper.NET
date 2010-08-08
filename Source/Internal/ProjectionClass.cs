using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Linq;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Projection.Attributes;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
#if VS2008

#endif

namespace AdFactum.Data.Internal
{
    public class NameAndType
    {
        public NameAndType(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public Type Type { get; private set;}
        public string Name  { get; private set;}
    }

    /// <summary>
    /// Defines a tupel with two properties.
    /// One the target property and one the projection property
    /// </summary>
    public class MemberProjectionTupel
    {
        private readonly Type projectedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberProjectionTupel"/> class.
        /// </summary>
        public MemberProjectionTupel(Property targetProperty, Type projectedOntoType, Property projectedOntoProperty, IAggregate aggregation, GroupByAttribute grouping)
        {
            Target = targetProperty;
            ProjectedOnto = projectedOntoProperty;
            projectedType = projectedOntoType;
            MemberAggregation = aggregation;
            MemberGrouping = grouping;
        }

        /// <summary> Gets or sets the member grouping. </summary>
        public GroupByAttribute MemberGrouping { get;  private set; }

        /// <summary> Gets or sets the member aggregation. </summary>
        public IAggregate MemberAggregation { get;  private set; }

        /// <summary> Gets the projected onto. </summary>
        public Property ProjectedOnto { get; private set; }

        /// <summary> Gets the target. </summary>
        private Property Target { get;  set; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>The column.</value>
        public string SelectColumn
        {
            get
            {
                var targetColumn = Target.MetaInfo.ColumnName;
                return string.Concat(WhereColumn, " AS ", Condition.QUOTE_OPEN, targetColumn, Condition.QUOTE_CLOSE);
            }
        }

        /// <summary>
        /// Gets the select column without table.
        /// </summary>
        /// <value>The select column without table.</value>
        public string SelectColumnWithoutTable
        {
            get
            {
                var sourceColumn = ProjectedOnto.MetaInfo.ColumnName;
                var targetColumn = Target.MetaInfo.ColumnName;

                return string.Concat(Condition.QUOTE_OPEN, sourceColumn, Condition.QUOTE_CLOSE, " AS ",
                                     Condition.QUOTE_OPEN, targetColumn, Condition.QUOTE_CLOSE);
            }
        }

        /// <summary>
        /// Gets the where column.
        /// </summary>
        /// <value>The where column.</value>
        public string WhereColumn
        {
            get
            {
                string sourceColumn = ProjectedOnto.MetaInfo.ColumnName;
                sourceColumn = string.Concat( 
                    Condition.QUOTE_OPEN, SourceTable, Condition.QUOTE_CLOSE,
                    ".", Condition.QUOTE_OPEN, sourceColumn, Condition.QUOTE_CLOSE);

                if (MemberAggregation != null)
                    sourceColumn = string.Format(MemberAggregation.Aggregation, sourceColumn);

                return sourceColumn;
            }
        }

        /// <summary>
        /// Gets the source table.
        /// </summary>
        /// <value>The source table.</value>
        public string SourceTable
        {
            get { return Table.GetTableInstance(projectedType).Name; }
        }

        /// <summary>
        /// Gets the source column.
        /// </summary>
        /// <value>The source column.</value>
        public string SourceColumn
        {
            get { return ProjectedOnto.MetaInfo.ColumnName; }
        }

        /// <summary>
        /// Gets the select target column without table.
        /// </summary>
        /// <value>The select target column without table.</value>
        public string SelectTargetColumnWithoutTable
        {
            get { return Target.MetaInfo.ColumnName; }
        }
    }

    /// <summary>
    /// This class defines a projection to a specific object
    /// </summary>
    public class ProjectionClass : ICloneable
    {
        /// <summary>
        /// Defines the projection Class
        /// </summary>
        public Type ProjectedType { get; internal set;}

        /// <summary> The NewExpression if the Projection is bound to an anonymous class </summary>
        public Expression Expression { get; internal set; }

        /// <summary>
        /// Gets the new expression.
        /// </summary>
        /// <value>The new expression.</value>
        public NewExpression NewExpression
        {
            get
            {
                NewExpression newExpression = Expression as NewExpression;
                if (newExpression != null) return newExpression;

                MemberInitExpression initExpression = Expression as MemberInitExpression;
                if (initExpression != null)
                    return initExpression.NewExpression;

                return null;
            }
        }

        /// <summary>
        /// Constructor Parameters
        /// </summary>
        private object[] constructorParameters;

        /// <summary>
        /// Column mapping for complex types
        /// </summary>
        public ReadOnlyCollection<ColumnDeclaration>[] ComplexTypeColumnMapping
        {
            get; 
            private set;
        }

        /// <summary>
        /// Set if a special constructor is needed
        /// </summary>
        public ConstructorInfo Constructor { get; private set; }

        /// <summary>
        /// Gets the MemberBindings
        /// </summary>
        public Dictionary<Property, Expression> MemberBindings { get; private set; }

        /// <summary>
        /// Gets the list of all member projections
        /// </summary>
        /// <value>The member projections.</value>
        public Dictionary<string, MemberProjectionTupel> MemberProjections { get; private set; }

        /// <summary>
        /// Gets the list of all type projections.
        /// </summary>
        /// <value>The type projections.</value>
        public Dictionary<Type, Table> TypeProjections { get; private set; }

        /// <summary>
        /// Gets or sets the table name overwrite.
        /// </summary>
        /// <value>The table name overwrite.</value>
        public string TableNameOverwrite { private get; set; }

        private string primaryKeyColumns;
        private Set tables;
        private string targetColumnsOnly;

        /// <summary>
        /// Default constructor
        /// </summary>
        private ProjectionClass()
        {
            MemberBindings = new Dictionary<Property, Expression>();
            MemberProjections = new Dictionary<string, MemberProjectionTupel>();
            TypeProjections = new Dictionary<Type, Table>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionClass"/> class.
        /// </summary>
        /// <param name="copy">The copy.</param>
        private ProjectionClass(ProjectionClass copy)
        {
            ProjectedType = copy.ProjectedType;
            Expression = copy.Expression;
            constructorParameters = copy.constructorParameters;
            ComplexTypeColumnMapping = copy.ComplexTypeColumnMapping;
            Constructor = copy.Constructor;
            MemberProjections = copy.MemberProjections;
            TypeProjections = copy.TypeProjections;
            TableNameOverwrite = copy.TableNameOverwrite;
            MemberBindings = copy.MemberBindings;

            primaryKeyColumns = copy.primaryKeyColumns;
            tables = copy.tables;
            targetColumnsOnly = copy.targetColumnsOnly;

            flatTemplates = copy.flatTemplates;
            deepTemplates = copy.deepTemplates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionClass"/> class.
        /// </summary>
        /// <param name="copy">The copy.</param>
        /// <param name="newFlatTemplates">The new flat templates.</param>
        /// <param name="newDeepTemplates">The new deep templates.</param>
        public ProjectionClass(ProjectionClass copy, 
            Dictionary<string, FieldDescription> newFlatTemplates, 
            Dictionary<string, FieldDescription> newDeepTemplates)
            :this(copy)
        {
            flatTemplates = newFlatTemplates;
            deepTemplates = newDeepTemplates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionClass"/> class.
        /// </summary>
        /// <param name="projectionClassParameter">The projection class parameter.</param>
        public ProjectionClass(Type projectionClassParameter) : this()
        {
            ProjectedType = projectionClassParameter;

            if (ProjectedType.IsValueObjectType())
                LoadValueObjectProjections();
            else
                LoadMemberProjections();
        }

        /// <summary>
        /// Creates a new projection
        /// </summary>
        public ProjectionClass(Expression expression, Dictionary<ParameterExpression, MappingStruct> parameterMapping) : this()
        {
            Expression = expression;
            ProjectedType = expression.Type;
            
            Constructor = NewExpression.Constructor;
            CreateFromAnonymousType(NewExpression.Arguments, parameterMapping);
        }

        /// <summary>
        /// Creates the projection from an anoymous type
        /// </summary>
        /// <param name="constructorArguments">The arguments.</param>
        private void CreateFromAnonymousType(ReadOnlyCollection<Expression> constructorArguments, Dictionary<ParameterExpression, MappingStruct> parameterMapping)
        {
            var parameters = Constructor.GetParameters();
            constructorParameters = new object[constructorArguments.Count];
            ComplexTypeColumnMapping = new ReadOnlyCollection<ColumnDeclaration>[constructorArguments.Count];

            for (var x = 0; x < constructorArguments.Count; x++)
            {
                var argument = x < constructorArguments.Count ? constructorArguments[x] : null;
                constructorParameters[x] = new Property(parameters[x]);

                var pe = argument as ParameterExpression;
                if (pe != null)
                {
                    MappingStruct mapping;
                    parameterMapping.TryGetValue(pe, out mapping);
                    argument = mapping.Expression;
                }

                /*
                 * Evaluate Method call expressions
                 */
                var mce = argument as MethodCallExpression;
                if (mce != null)
                {
                    argument = mce.Arguments.Count > 1 ? ((LambdaExpression)mce.Arguments[1]).Body : 
                        mce.Arguments.Count > 0 ? mce.Arguments[0] : null;
                }

                var unary = argument as UnaryExpression;
                if (unary != null)
                    argument = unary.Operand;

                if (argument is ConditionalExpression || argument is MemberExpression)
                    continue;

                /*
                 * Set a constant definition
                 */
                var ce = argument as ValueExpression;
                if (ce != null)
                {
                    constructorParameters[x] = ce.Value;
                    continue;
                }

                /*
                 * Result of the Select expression
                 */
                var se = argument as SelectExpression;
                if (se != null)
                {
                    if (se.RevealedType.IsValueObjectType())
                    {
                        ComplexTypeColumnMapping[x] = se.Columns;
                        constructorParameters[x] = se.Type.RevealType();
                    }
                    continue;
                }

                var te = argument as TableExpression;
                if (te != null)
                {
                    constructorParameters[x] = te.Type.RevealType();
                    ComplexTypeColumnMapping[x] = te.Columns;
                    continue;
                }
            }
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        public static string GetAttributeKey(MemberInfo info)
        {
            var isClass = info as Type;
            if (isClass != null)
                return isClass.FullName;

            // If It's a getter
            var className = info.DeclaringType.FullName;
            var methodName = info.Name;
            if (!methodName.StartsWith("get_"))
                methodName = "get_" + methodName;

            return string.Concat(className, ".", methodName);
        }

        /// <summary>
        /// Constructor Parameters
        /// </summary>
        public object[] CopyOfConstructorParameters
        {
            get { return constructorParameters != null ? (object[]) constructorParameters.Clone() : null; }
        }

        /// <summary>
        /// Loads the class projections.
        /// </summary>
        private void LoadValueObjectProjections()
        {
            var projectionTable = Table.GetTableInstance(ProjectedType);
            TypeProjections.Add(ProjectedType, projectionTable);
        }

        /// <summary>
        /// Loads the projections.
        /// </summary>
        private void LoadMemberProjections()
        {
            var counter = Property.GetPropertyInstances(ProjectedType).GetEnumerator();
            while (counter.MoveNext())
            {
                var target = counter.Current.Value;
                var attributes = counter.Current.Key.GetCustomAttributes(true);

                InitPropertyProjection(attributes, target);
            }
        }

        /// <summary>
        /// Inits the projections.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="target">The target.</param>
        private void InitPropertyProjection(object[] attributes, Property target)
        {
            /*
             * Search for projection attributes
             */
            IAggregate memberAggregation = null;
            GroupByAttribute memberGrouping = null;
            ProjectOntoPropertyAttribute memberProjectedOnto = null;
            Property projectedOntoProperty = null;
            foreach (Attribute attribute in attributes)
            {
                /*
                 * Project onto attribute
                 */
                var projectOnto = attribute as ProjectOntoPropertyAttribute;
                if (projectOnto != null)
                {
                    memberProjectedOnto = projectOnto;
                    var projectedOntoInfo = memberProjectedOnto.ProjectedType.GetPropertyInfo(memberProjectedOnto.ProjectedProperty);
                    projectedOntoProperty = Property.GetPropertyInstance(projectedOntoInfo);
                    continue;
                }

                /*
                 * Sql Aggregate Functions
                 */
                var aggregateAttribute = attribute as IAggregate;
                if (aggregateAttribute != null)
                {
                    memberAggregation = aggregateAttribute;
                    continue;
                }

                /*
                 * Group By Clause
                 */
                var groupByAttribute = attribute as GroupByAttribute;
                if (groupByAttribute == null) continue;
                memberGrouping = groupByAttribute;
                continue;
            }

            /*
             * Add the member projection
             */
            if (memberProjectedOnto != null)
            {
                var mpt = new MemberProjectionTupel(
                    target,
                    memberProjectedOnto.ProjectedType,
                    projectedOntoProperty,
                    memberAggregation,
                    memberGrouping);

                MemberProjections.Add(target.Key, mpt);
            }
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <returns></returns>
        public string GetColumns(ICondition whereClause, string additonalColumns)
        {
            var whereClauseTables = whereClause != null ? whereClause.Tables : null;

            /*
             * Calculate columns
             */
            var result = additonalColumns ?? string.Empty;
            foreach (var pair in MemberProjections)
            {
                var mpt = pair.Value;
                if (mpt == null) continue;
                if (result.Length > 0) result += ", ";
                result += mpt.SelectColumn;
                continue;
            }

            foreach (var pair in TypeProjections)
            {
                var projectionTable = pair.Value;
                if (projectionTable == null) continue;
                Set.Tupel tableTupel = null;
                if (whereClauseTables != null) tableTupel = whereClauseTables.GetTupel(projectionTable.Name);
                if (tableTupel == null) tableTupel = Tables.GetTupel(projectionTable.Name);

                if (result.Length > 0) result += ", ";
                result += string.Concat(tableTupel.Table, ".*");
            }

            return result;
        }

        /// <summary>
        /// Gets the grouping.
        /// </summary>
        /// <returns></returns>
        public string GetGrouping()
        {
            var result = string.Empty;
            foreach (var pair in MemberProjections)
            {
                var mpt = pair.Value;
                if (mpt == null || mpt.MemberGrouping == null) continue;
                if (result.Length > 0) result += ", ";
                result += mpt.WhereColumn;
            }
            return result;
        }


        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <value>The tables.</value>
        public Set Tables
        {
            get
            {
                /*
                 * Return - if already calculated
                 */
                if (tables != null)
                    return tables;

                lock (this)
                {
                    /*
                     * Return - if already calculated
                     */
                    if (tables != null)
                        return tables;

                    var newTableSet = new Set();

                    /*
                     * Calculate tables
                     */
                    foreach (var pair in MemberProjections)
                    {
                        var mpt = pair.Value;
                        if (mpt != null)
                            newTableSet.Add(mpt.SourceTable);
                    }

                    foreach (var pair in TypeProjections)
                    {
                        var projectionTable = pair.Value;
                        if (projectionTable != null)
                            newTableSet.Add(projectionTable.Name);
                    }

                    tables = newTableSet;
                    return tables;
                }
            }
        }

        /// <summary>
        /// Gets the primary key columns.
        /// </summary>
        /// <value>The primary key columns.</value>
        public string PrimaryKeyColumns
        {
            get
            {
                /*
                 * Return - if already calculated
                 */
                if (primaryKeyColumns != null)
                    return primaryKeyColumns;

                lock (this)
                {
                    /*
                     * Return - if already calculated
                     */
                    if (primaryKeyColumns != null)
                        return primaryKeyColumns;

                    /*
                     * Calculate columns
                     */
                    var result = string.Empty;
                    foreach (var pair in MemberProjections)
                    {
                        var mpt = pair.Value;
                        if ((mpt == null) || (!mpt.ProjectedOnto.MetaInfo.IsPrimaryKey)) continue;
                        if (result.Length > 0) result += ", ";
                        result += mpt.WhereColumn;
                    }

                    foreach (var pair in TypeProjections)
                    {
                        var projectionTable = pair.Value;
                        if (projectionTable == null) continue;

                        var projection = ReflectionHelper.GetProjection(projectionTable.ClassType, null);
                        if (result.Length > 0) result += ", ";
                        result += string.Concat(Condition.SCHEMA_REPLACE, Condition.QUOTE_OPEN, projectionTable.Name,
                                                Condition.QUOTE_CLOSE, ".",
                                                Condition.QUOTE_OPEN, projection.GetPrimaryKeyDescription().Name, Condition.QUOTE_CLOSE);
                    }

                    primaryKeyColumns = result;
                    return primaryKeyColumns;
                }
            }
        }

        /// <summary>
        /// Gets the target columns only.
        /// </summary>
        /// <value>The target columns only.</value>
        public string TargetColumnsOnly
        {
            get
            {
                /*
                 * Return - if already calculated
                 */
                if (targetColumnsOnly != null)
                    return targetColumnsOnly;

                lock (this)
                {
                    /*
                     * Return - if already calculated
                     */
                    if (targetColumnsOnly != null)
                        return targetColumnsOnly;

                    /*
                     * Calculate columns
                     */
                    var result = string.Empty;
                    foreach (var pair in MemberProjections)
                    {
                        var mpt = pair.Value;
                        if (mpt == null) continue;
                        if (result.Length > 0) result += ", ";
                        result += mpt.SelectTargetColumnWithoutTable;
                    }

                    foreach (var pair in TypeProjections)
                    {
                        var projectionTable = pair.Value;
                        if (projectionTable == null) continue;
                        if (result.Length > 0) result += ", ";
                        result += "*";
                    }

                    targetColumnsOnly = result;
                    return targetColumnsOnly;
                }
            }
        }

        private string columnsOnly;
        
        /// <summary>
        /// Gets the columns only.
        /// </summary>
        /// <value>The columns only.</value>
        public string ColumnsOnly
        {
            get
            {
                /*
                 * Return - if already calculated
                 */
                if (columnsOnly != null)
                    return columnsOnly;

                lock (this)
                {
                    /*
                     * Return - if already calculated
                     */
                    if (columnsOnly != null)
                        return columnsOnly;

                    /*
                     * Calculate columns
                     */
                    var result = string.Empty;
                    foreach (var pair in MemberProjections)
                    {
                        var mpt = pair.Value;
                        if (mpt == null) continue;
                        if (result.Length > 0) result += ", ";
                        result += mpt.SelectColumnWithoutTable;
                    }

                    foreach (var pair in TypeProjections)
                    {
                        var projectionTable = pair.Value;
                        if (projectionTable == null) continue;
                        if (result.Length > 0) result += ", ";
                        result += "*";
                    }

                    columnsOnly = result;
                    return columnsOnly;
                }
            }
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName
        {
            get { return TableNameOverwrite ?? Table.GetTableInstance(ProjectedType).Name; }
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public ReadOnlyCollection<ColumnDeclaration> GetColumns(Alias tableAlias, Cache<Type, ProjectionClass> dynamicCache)
        {
            var columns = new Collection<ColumnDeclaration>();

            if (Expression != null)
            {
                var expressionColumns = ColumnProjector.Evaluate(Expression, dynamicCache);
                foreach (var column in expressionColumns)
                {
                    var aliasedExpression = column.Expression as AliasedExpression;
                    if (aliasedExpression != null)
                        aliasedExpression.SetAlias(tableAlias);
                    
                    columns.Add(new ColumnDeclaration(aliasedExpression ?? column.Expression, column));
                }

                return new ReadOnlyCollection<ColumnDeclaration>(columns);
            }

            // Only use it for standard types
            Dictionary<string, FieldDescription> fields = GetFieldTemplates(false);
            foreach (var field in fields)
            {
                if (field.Value.CustomProperty.IsReadOnly || field.Value.CustomProperty.MetaInfo.IsIgnore)
                    continue;

                if (field.Value.FieldType.Equals(typeof(Field)))
                {
                    var selectFunction = field.Value.CustomProperty.MetaInfo.SelectFunction;
                    if (string.IsNullOrEmpty(selectFunction))
                    {
                        var memberAccess = new PropertyExpression(field.Value.ContentType, this, tableAlias, field.Value);
                        columns.Add(new ColumnDeclaration(memberAccess, field.Value));
                    }
                    else
                    {
                        var selectFunctionExpr = new SelectFunctionExpression(ProjectedType, selectFunction);
                        columns.Add(new ColumnDeclaration(selectFunctionExpr, field.Value));
                    }
                }

                if (field.Value.FieldType.Equals(typeof(Link)))
                {
                    var memberAccess = new PropertyExpression(field.Value.ContentType, this, tableAlias, field.Value);
                    columns.Add(new ColumnDeclaration(memberAccess, field.Value));

                    if (field.Value.CustomProperty.MetaInfo.IsGeneralLinked)
                    {
                        var typAccess = new PropertyExpression(field.Value.ContentType, this, tableAlias,
                            new FieldDescription(memberAccess.Name + "#TYP",ProjectedType, typeof (string),false));
                        columns.Add(new ColumnDeclaration(typAccess, Alias.Generate(field.Key + "#TYP")));
                    }
                    continue;
                }

                if (field.Value.FieldType.Equals(typeof(SpecializedLink)))
                {
                    var memberAccess = new PropertyExpression(field.Value.ContentType, this, tableAlias, field.Value);
                    columns.Add(new ColumnDeclaration(memberAccess, field.Value));
                }
            }
            return new ReadOnlyCollection<ColumnDeclaration>(columns);
        }

        /// <summary>
        /// Gets the constructor property instances.
        /// </summary>
        /// <returns></returns>
        public Dictionary<NameAndType, Property> GetConstructorPropertyInstances()
        {
            if (Constructor == null) return null;

            var parameters = Constructor.GetParameters();
            return parameters.ToDictionary(
                p => new NameAndType(p.Name, p.ParameterType),
                p => new Property(p));
        }


        #region GetFieldTemplates Replacement, that was previous stored within the ReflectionHelper class.

        private Dictionary<string, FieldDescription> flatTemplates;
        private Dictionary<string, FieldDescription> deepTemplates;
        private FieldDescription primaryKeyDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionClass"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="complexMappings">The declarations.</param>
        public ProjectionClass(Expression expression, ReadOnlyCollection<ColumnDeclaration>[] complexMappings, Dictionary<ParameterExpression, MappingStruct> parameterMapping)
            : this(expression, parameterMapping)
        {
            ComplexTypeColumnMapping = complexMappings ?? ComplexTypeColumnMapping;
        }

        /// <summary>
        /// Gets the field templates for an projection
        /// </summary>
        public Dictionary<string, FieldDescription> GetFieldTemplates(bool flat)
        {
            if (flat && flatTemplates != null) return flatTemplates;
            if (!flat && deepTemplates != null) return deepTemplates;

            Dictionary<string, FieldDescription> result;

            if (Constructor != null && Constructor.GetParameters().Length > 0)
            {
                var counter = GetConstructorPropertyInstances().Values.Concat(MemberBindings.Keys).ToList();
                result = EvaluateFieldTemplates(ProjectedType, flat, counter);
            }
            else
            {
                var counter = Property.GetPropertyInstances(ProjectedType).Values.Concat(MemberBindings.Keys).ToList();
                result = EvaluateFieldTemplates(ProjectedType, flat, counter);
            }

            // Add Complex Column Mapping Templates
            if (ComplexTypeColumnMapping != null)
                foreach (ColumnDeclaration column in from columnDeclarations in ComplexTypeColumnMapping where columnDeclarations != null 
                                                     from column in columnDeclarations select column)
                    result.Add(column.Alias.Name, null);

            if (flat) flatTemplates = result;
                 else deepTemplates = result;

            return result;
        }

        /// <summary>
        /// Evaluates the field templates.
        /// </summary>
        private static Dictionary<string, FieldDescription> EvaluateFieldTemplates(Type type, bool flat, List<Property> properties)
        {
            var result = new Dictionary<string, FieldDescription>();
            var counter = properties.GetEnumerator();
            while (counter.MoveNext())
            {
                var property = counter.Current;
                var propertyCustomInfo = property.MetaInfo;
                var propertyInfo = property.PropertyInfo;
                var propertyVirtualLink = ReflectionHelper.GetVirtualLinkInstance(propertyInfo);

                /*
                 * Eventuell die Eigenschaft überlesen, wenn der Zugriff verweigert wird
                 */
                if (propertyCustomInfo == null || propertyCustomInfo.IsIgnore)
                    continue;

                /*
                 * Prüfen, ob das Property nicht mit einem neueren Property überschrieben werden muss
                 */
                var propertyName = propertyCustomInfo.ColumnName; //.ToUpper();

                /*
                 * Handelt es sich um einen Virtuellen Link?
                 */
                if (propertyVirtualLink != null)
                {
                    var virtualField = new VirtualFieldDescription
                        (type, propertyName, property.PropertyType, property,
                         propertyVirtualLink);

                    result.Add(propertyName, virtualField);
                    continue;
                }

                /*
                 * Handelt es sich um eine Feld - Wert Zuordnung ?
                 */
                if (!flat)
                {
                    bool maybeRemoveIt = false;
                    if (result.ContainsKey(propertyName))
                    {
                        if (propertyInfo.DeclaringType.Equals(type))
                            maybeRemoveIt = true;  // Only Remove, if it's a flat type
                        else
                            continue;
                    }

                    if (property.PropertyType.IsListType() && propertyCustomInfo.IsOneToManyAssociation)
                    {
                        if (maybeRemoveIt)
                            result.Remove(propertyName);
                        result.Add(propertyName, new FieldDescription(propertyName, type, typeof(OneToManyLink), propertyCustomInfo.LinkTarget, property, false));
                        continue;
                    }

                    if (property.PropertyType.IsListType() || property.PropertyType.IsDictionaryType())
                    {
                        if (maybeRemoveIt)
                            result.Remove(propertyName);
                        result.Add(propertyName, new FieldDescription(propertyName, type, typeof(ListLink), property.PropertyType, property, false));
                        continue;
                    }

                    if (property.PropertyType.IsValueObjectType())
                    {
                        if (maybeRemoveIt)
                            result.Remove(propertyName);

                        if (propertyCustomInfo.IsGeneralLinked)
                            result.Add(propertyName, new FieldDescription(propertyName, type, typeof(Link), property.PropertyType, property, false));
                        else
                            result.Add(propertyName, new FieldDescription(propertyName, type, typeof(SpecializedLink), propertyCustomInfo.LinkTarget, property, false));

                        continue;
                    }

                    if (maybeRemoveIt)
                        continue;
                }
                else
                    if (property.PropertyType.IsComplexType())
                        continue;

                if (!result.ContainsKey(propertyName))
                    result.Add(propertyName, new FieldDescription(propertyName, type, typeof(Field), property.PropertyType, property, false));
            }

            return result;
        }

        /// <summary>
        /// Gets the primary key description.
        /// </summary>
        /// <returns></returns>
        public FieldDescription GetPrimaryKeyDescription()
        {
            if (primaryKeyDescription != null)
                return primaryKeyDescription;

            /*
             * If it's an anonymous type return null
             */
            if (ProjectedType.IsReadOnlyType())
                return null;

            primaryKeyDescription = GetFieldTemplates(false).FirstOrDefault(primary => primary.Value.IsPrimary).Value;
            if (primaryKeyDescription == null)
                throw new NoPrimaryKeyFoundException(ProjectedType);

            return primaryKeyDescription;
        }

        #endregion

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new ProjectionClass(this);
        }
    }
}