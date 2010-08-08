using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Translation
{
    /// <summary>
    /// The UnusedColumnRemover removes unused, that means, not referenced columns of subqueries.
    /// That's especially necessary for In-Conditions and subselects
    /// </summary>
    public class UnusedColumnRemover : DbExpressionVisitor
    {
        private class UsedDeclarations
        {
            public UsedDeclarations(bool exlusive, List<ColumnDeclaration> columns)
            {
                Exclusive = exlusive;
                Columns = columns;
            }

            public bool Exclusive { get; private set; }
            public List<ColumnDeclaration> Columns { get; private set; }
        }

        private readonly Stack<UsedDeclarations> usedColumns = new Stack<UsedDeclarations>();
        private UsedDeclarations currentlyUsedColumns;

        private readonly Cache<Type, ProjectionClass> dynamicCache;

        private UnusedColumnRemover(Cache<Type, ProjectionClass> cache)
        {
            dynamicCache = cache;
#if TRACE
            Console.WriteLine("\nUnusedColumnRemover:");
#endif

        }

        /// <summary>
        /// The Column Remover removes unused columns out of an expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="dynamicCache"></param>
        /// <returns></returns>
        public static Expression Rewrite (Expression expression, Cache<Type, ProjectionClass> dynamicCache)
        {
            var ucr = new UnusedColumnRemover(dynamicCache);
            expression = ucr.Visit(expression);

            return expression;
        }

        /// <summary>
        /// This method extracts the valid columns from a selection expression.
        /// This is done by the expected result type of the select expression and the push down columns of the surrounding selection.
        /// </summary>
        /// <param name="aliased">The aliased.</param>
        /// <returns></returns>
        private List<ColumnDeclaration> GetValidColumns(SelectExpression aliased)
        {
            IEnumerable<string> validMembers = new List<string>();

            // If only distinct columns shall returned, than do nothing
            if (aliased.IsDistinct)
                return new List<ColumnDeclaration> (aliased.Columns);

            // If it's not top-Level, than return only the valid from the parent sselection 
            UsedDeclarations usedDeclarations;
            if (usedColumns.Count > 0)
            {
                usedDeclarations = usedColumns.Peek();
                validMembers = usedDeclarations.Columns.Select(cd => cd.Alias.Name);
            }
            // If it's top-level, than evaluate all 
            else
            {
                var pc = ReflectionHelper.GetProjection(aliased.RevealedType, dynamicCache);
                
                IEnumerable<string> complexMappings = null;
                IEnumerable<string> propertyInstances = null;
                IEnumerable<string> memberProjections = null;
                IEnumerable<string> memberBindings = null;
                List<string> valueObjectProjections = null;

                if (pc.ComplexTypeColumnMapping != null)
                    foreach (var ctcm in pc.ComplexTypeColumnMapping.Where(ctcm => ctcm != null))
                    {
                        if (complexMappings == null) complexMappings = new List<string>();
                        complexMappings = complexMappings.Concat(ctcm.Select(cd => cd.Alias.Name));
                    }

                if (pc.GetConstructorPropertyInstances()!=null)
                    propertyInstances = pc.GetConstructorPropertyInstances().Keys.Select(key => key.Name);

                if (pc.MemberProjections != null)
                    memberProjections =  pc.MemberProjections.Keys;

                if (pc.MemberBindings != null)
                    memberBindings = pc.MemberBindings.Keys.Select(property => property.MetaInfo.ColumnName);

                if (pc.ProjectedType.IsValueObjectType())
                {
                    var originalPc = dynamicCache.Contains(pc.ProjectedType) ? ReflectionHelper.GetProjection(pc.ProjectedType, null) : pc;
                    Dictionary<string, FieldDescription> flatTemplates = originalPc.GetFieldTemplates(true);
                    Dictionary<string, FieldDescription> deepTemplates = originalPc.GetFieldTemplates(false);
                    Dictionary<string, FieldDescription> newFlatTemplates = new Dictionary<string, FieldDescription>();
                    Dictionary<string, FieldDescription> newDeepTemplates = new Dictionary<string, FieldDescription>();

                    valueObjectProjections = new List<string>();

                    var unmappedColumns = new List<ColumnDeclaration>();
                    var usedTemplates = new HashSet<string>();

                    foreach (var column in aliased.Columns)
                    {
                        var original = column.OriginalProperty;
                        
                        // Try to map the original property
                        if (original != null && original.ParentType == originalPc.ProjectedType)
                        {
                            var upperedAlias = column.Alias.Name; //.ToUpper();
                            FieldDescription field;
                            if (deepTemplates.TryGetValue(original.Name, out field))
                            {
                                if (!usedTemplates.Contains(original.Name)) usedTemplates.Add(original.Name);
                                
                                valueObjectProjections.Add(column.Alias.Name);
                                newDeepTemplates.Add(upperedAlias, new FieldDescription(column.Alias.Name, field.ParentType, field.FieldType, field.ContentType, field.CustomProperty, field.IsPrimary));
                            }

                            if (flatTemplates.TryGetValue(original.Name, out field))
                            {
                                if (!usedTemplates.Contains(original.Name)) usedTemplates.Add(original.Name);
                                
                                valueObjectProjections.Add(column.Alias.Name);
                                newFlatTemplates.Add(upperedAlias, new FieldDescription(column.Alias.Name, field.ParentType, field.FieldType, field.ContentType, field.CustomProperty, field.IsPrimary));
                            }
                        }
                        else
                            unmappedColumns.Add(column);
                    }

                    // As a second step, try to fill all unmapped columns
                    foreach (var column in unmappedColumns)
                    {
                        var upperedAlias = column.Alias.Name; //.ToUpper();
                        if (usedTemplates.Contains(upperedAlias))
                            continue;

                        // Do a simple alias mapping, if the original property is not available or does not match
                        FieldDescription field;
                        if (deepTemplates.TryGetValue(column.Alias.Name, out field))
                        {
                            valueObjectProjections.Add(column.Alias.Name);
                            newDeepTemplates.Add(column.Alias.Name, new FieldDescription(column.Alias.Name, field.ParentType, field.FieldType, field.ContentType, field.CustomProperty, field.IsPrimary));
                        }

                        if (flatTemplates.TryGetValue(column.Alias.Name, out field))
                        {
                            valueObjectProjections.Add(column.Alias.Name);
                            newFlatTemplates.Add(column.Alias.Name, new FieldDescription(column.Alias.Name, field.ParentType, field.FieldType, field.ContentType, field.CustomProperty, field.IsPrimary));
                        }
                    }

                    // Attach all deep links, that do not depend on the current table selection
                    foreach (var template in deepTemplates.Where(kvp => kvp.Value.FieldType == typeof (ListLink) || kvp.Value.FieldType == typeof (OneToManyLink)))
                        newDeepTemplates.Add(template.Key, template.Value);

                    // Insert the new Projection (including the new column name mapping into the dynamic cache of the expression)
                    var newProjection = new ProjectionClass(originalPc, newFlatTemplates, newDeepTemplates);
                    dynamicCache.Insert(pc.ProjectedType, newProjection);

                    valueObjectProjections.AddRange(deepTemplates.Where(kvp => kvp.Value.FieldType.Equals(typeof(Link))).Select(kvp => kvp.Key + "#TYP"));
                }

                if (propertyInstances != null) validMembers = validMembers.Concat(propertyInstances);
                if (memberProjections != null) validMembers = validMembers.Concat(memberProjections);
                if (memberBindings != null) validMembers = validMembers.Concat(memberBindings);
                if (valueObjectProjections != null) validMembers = validMembers.Concat(valueObjectProjections);
                if (complexMappings != null) validMembers = validMembers.Concat(complexMappings);
            }

            return aliased.Columns.Where(c => validMembers.Select(x=>x.ToLower()).Contains(c.Alias.Name.ToLower()) 
                //|| c.Expression is ConditionalExpression
                || c.Expression is RowNumberExpression 
                || c.Expression is RowNumExpression).ToList();
        }

        /// <summary>
        /// Remove unused columns of the select expression
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        protected override Expression VisitSelectExpression(SelectExpression select)
        {
            if (select == null)
                return select;


            // Visit the Sub Clause first in order to select push Down Columns
            currentlyUsedColumns = new UsedDeclarations(false, new List<ColumnDeclaration>());

            var where = Visit(select.Where);
            var orderBy = VisitOrderBy(select.OrderBy);
            var groupBy = VisitExpressionList(select.GroupBy);
            var skip = Visit(select.Skip);
            var take = Visit(select.Take);
            List<ColumnDeclaration> validColumns = usedColumns.Count == 0
                                    ? new List<ColumnDeclaration>(VisitColumnDeclarations(select.Columns)) 
                                    : new List<ColumnDeclaration>(VisitAllButPropertyExpressions(select.Columns));
            var selector = select.Selector;

            // Only Projected Types can be tested
            if (select.RevealedType.IsProjectedType(dynamicCache))
            {
                // Shorten the select columns
                validColumns = GetValidColumns(select);
                currentlyUsedColumns.Columns.AddRange(validColumns);
            }

            // If it the From is an join, than visit the condition to take that append to the currently used columns
            JoinExpression join = select.From as JoinExpression;
            if (join != null)
                Visit(join.Condition);

            usedColumns.Push(currentlyUsedColumns);
            try
            {
                // now dive
                var from = VisitSource(select.From);
                return UpdateSelect(select, selector, from, where, orderBy, groupBy, skip, take, select.IsDistinct, select.IsReverse, new ReadOnlyCollection<ColumnDeclaration>(validColumns), select.SqlId, select.Hint, select.DefaultIfEmpty);
            }
            finally
            {
                usedColumns.Pop();
            }
        }

        /// <summary>
        /// Visits all but property expressions.
        /// </summary>
        /// <param name="declarations">The declarations.</param>
        /// <returns></returns>
        private IEnumerable<ColumnDeclaration> VisitAllButPropertyExpressions(ReadOnlyCollection<ColumnDeclaration> declarations)
        {
            foreach (var declaration in declarations)
            {
                if (!(declaration.Expression is PropertyExpression))
                    Visit(declaration.Expression);
            }

            return declarations;
        }

        /// <summary>
        /// Visits the join expression.
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        protected override Expression VisitJoinExpression(JoinExpression join)
        {
            var condition = Visit(join.Condition);
            var left = VisitSource(join.Left);
            var right = VisitSource(join.Right);
            return UpdateJoin(join, join.Join, left, right, condition);
        }

        /// <summary>
        /// Visits the column expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitColumn(PropertyExpression expression)
        {
            if (expression.ReferringColumn != null)
                currentlyUsedColumns.Columns.Add(expression.ReferringColumn);

            return base.VisitColumn(expression);
        }

        /// <summary>
        /// This checks the contains method and removes all other columns, of the subsequent selects
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            bool pop = false;
            if (m.Method.Name == "Contains" && m.Arguments.Count()==2 && m.Arguments.First().Type.IsQueryable())
            {
                var property = m.Arguments.Last() as PropertyExpression;
                if (property != null)
                {
                    usedColumns.Push(new UsedDeclarations(true, 
                        new List<ColumnDeclaration>
                        {
                            new ColumnDeclaration(property, Alias.Generate(property.Name))
                        }));

                    pop = true;
                }
            }

            try
            {
                return base.VisitMethodCall(m);
            }
            finally
            {
                if (pop)
                    usedColumns.Pop();
            }
        }
    }
}
