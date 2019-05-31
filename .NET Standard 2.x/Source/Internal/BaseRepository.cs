using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// BaseRepository
    /// </summary>
    public abstract class BaseRepository : IRepository
    {
        /// <summary> Sql Tracer </summary>
        protected ISqlTracer SqlTracer { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracer"></param>
        protected BaseRepository(ISqlTracer tracer)
        {
            SqlTracer = tracer;
        }

        /// <summary>
        /// Writes the repository.
        /// </summary>
        public virtual void WriteRepository(ObjectMapper mapper, IEnumerable<Type> types)
        {
            VersionInfo versionInfo = mapper.GetVersionInfo();
            if (versionInfo.HasReleased)
                throw new VersionHasAlreadyReleasedException(mapper.TransactionContext.DatabaseMajorVersion,
                                                             mapper.TransactionContext.DatabaseMinorVersion);

            InsertEntityMetaInfo(mapper, types);
            InsertEntityRelations(mapper, types);
            InsertEntityPredicates(mapper, types);
        }

        /// <summary>
        /// Gets the repository types.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Type> GetRepositoryTypes()
        {
            var repositoryTypes = new List<Type>
                                      {
                                          typeof (VersionInfo),
                                          typeof (EntityInfo),
                                          typeof (EntityPredicate),
                                          typeof (EntityRelation)
                                      };
            return repositoryTypes;
        }

        #region Repository Helper

        /// <summary>
        /// Inserts the entity meta info.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="types">The types.</param>
        protected virtual void InsertEntityMetaInfo(ObjectMapper mapper, IEnumerable<Type> types)
        {
            /*
			 * Cleanup tables
			 */
            VersionInfo versionInfo = mapper.GetVersionInfo();
            ICondition deleteCondition = new ConditionList(
                new AndCondition(typeof(EntityInfo), "VersionInfo", versionInfo.Id));

            /*
			 * Write Repository
			 */
            mapper.BeginTransaction();
            mapper.Delete(typeof(EntityInfo), deleteCondition);
            mapper.Flush();
            var shortNames = new ArrayList();

            foreach (Type type in types)
            {
                if (Table.GetTableInstance(type)
                        .IsAccessible(mapper.TransactionContext.DatabaseMajorVersion,
                                      mapper.TransactionContext.DatabaseMinorVersion) == false)
                    continue;

                /*
				 * Store data
				 */
                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                IDictionary fields = projection.GetFieldTemplates(false);
                mapper.Save(new EntityInfo(versionInfo, type, shortNames));

                foreach (FieldDescription description in fields.Values)
                    if (description.FieldType.Equals(typeof(ListLink)))
                        mapper.Save(new EntityInfo(versionInfo, type, description.Name, shortNames));
            }

            try
            {
                mapper.Commit();
            }
            catch (Exception)
            {
                mapper.Rollback();
                throw;
            }
        }


        /// <summary>
        /// Inserts the entity relations.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="types">The types.</param>
        protected virtual void InsertEntityRelations(ObjectMapper mapper, IEnumerable<Type> types)
        {
            /*
			 * Cleanup tables
			 */
            VersionInfo versionInfo = mapper.GetVersionInfo();
            ICondition deleteCondition = new ConditionList(
                new AndCondition(typeof(EntityRelation), "VersionInfo", versionInfo.Id));

            var definedRelations = new Dictionary<string, EntityRelation>();

            /*
			 * Search for relations
			 */
            foreach (Type type in types)
            {
                if (Table.GetTableInstance(type)
                        .IsAccessible(mapper.TransactionContext.DatabaseMajorVersion,
                                      mapper.TransactionContext.DatabaseMinorVersion) == false)
                    continue;

                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                IDictionary fields = projection.GetFieldTemplates(false);

                foreach (FieldDescription description in fields.Values)
                {
                    if (description is VirtualFieldDescription)
                        continue;

                    if (description.FieldType.Equals(typeof(Field)))
                        continue;

                    var relation = new EntityRelation();
                    relation.Initialize(versionInfo, mapper, description);
                    if (relation.OrmRelation == EntityRelation.OrmType.None)
                        continue;

                    if (definedRelations.ContainsKey(relation.UniqueIdentifierKey))
                    {
                        EntityRelation alreadyDefinedRelation = definedRelations[relation.UniqueIdentifierKey];
                        if (alreadyDefinedRelation.OrmRelation == EntityRelation.OrmType.Association)
                            definedRelations.Remove(relation.UniqueIdentifierKey);

                    }

                    if (!definedRelations.ContainsKey(relation.UniqueIdentifierKey))
                    {
                        definedRelations.Add(relation.UniqueIdentifierKey, relation);
                    }
                    else
                    {
                        if ((SqlTracer != null) && (SqlTracer.TraceErrorEnabled))
                            SqlTracer.ErrorMessage("Can't add relation: " + relation.DebugInfo(),
                                                   "InsertEntityRelations");
                    }
                }
            }

            /*
			 * Search virtual links
			 */
            foreach (Type type in types)
            {
                if (Table.GetTableInstance(type)
                        .IsAccessible(mapper.TransactionContext.DatabaseMajorVersion,
                                      mapper.TransactionContext.DatabaseMinorVersion) == false)
                    continue;

                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                IDictionary fields = projection.GetFieldTemplates(false);

                foreach (FieldDescription description in fields.Values)
                {
                    var vfd = description as VirtualFieldDescription;

                    /*
					 * Only look for virtual links without a global join field,
					 * because only this fields can be referenced directly
					 */
                    if ((vfd != null) && (vfd.GlobalJoinField == null) && (vfd.IsLinkTableUsed == false)
                        && !(vfd.CurrentJoinField is VirtualFieldDescription))
                    {
                        var relation = new EntityRelation();
                        relation.Initialize(versionInfo, mapper, vfd, EntityRelation.OrmType.Association);

                        if (!definedRelations.ContainsKey(relation.UniqueIdentifierKey))
                        {
                            definedRelations.Add(relation.UniqueIdentifierKey, relation);
                        }
                        else
                        {
                            if ((SqlTracer != null) && (SqlTracer.TraceErrorEnabled))
                                SqlTracer.ErrorMessage("Can't add relation: " + relation.DebugInfo(),
                                                       "InsertEntityRelations");
                        }
                    }
                }
            }

            /*
             * Search for user defined relations
             */
            foreach (Type type in types)
            {
                Table tableInstance = Table.GetTableInstance(type);

                if (tableInstance
                        .IsAccessible(mapper.TransactionContext.DatabaseMajorVersion,
                                      mapper.TransactionContext.DatabaseMinorVersion) == false)
                    continue;

                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                IDictionary fields = projection.GetFieldTemplates(false);
                List<EntityRelation> relations = GetForeignKeyForUserDefinedEntityRelations(versionInfo, mapper,
                                                                                            tableInstance.DefaultName, fields);
                foreach (EntityRelation relation in relations)
                {
                    if (!definedRelations.ContainsKey(relation.UniqueIdentifierKey))
                    {
                        definedRelations.Add(relation.UniqueIdentifierKey, relation);
                    }
                    else
                    {
                        if ((SqlTracer != null) && (SqlTracer.TraceErrorEnabled))
                            SqlTracer.ErrorMessage("Can't add relation: " + relation.DebugInfo(),
                                                   "InsertEntityRelations");
                    }
                }
            }


            /*
			 * Write Repository
			 */
            mapper.BeginTransaction();
            mapper.Delete(typeof(EntityRelation), deleteCondition);
            mapper.Flush();

            foreach (var relation in definedRelations)
                mapper.Save(relation.Value);

            /*
			 * Commit all
			 */
            try
            {
                mapper.Commit();
            }
            catch (Exception)
            {
                mapper.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserts the entity predicates.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="types">The types.</param>
        protected virtual void InsertEntityPredicates(ObjectMapper mapper, IEnumerable<Type> types)
        {
            IList uniqueEntries = new ArrayList();

            /*
			 * Cleanup tables
			 */
            VersionInfo versionInfo = mapper.GetVersionInfo();
            ICondition deleteCondition = new ConditionList(
                new AndCondition(typeof(EntityPredicate), "VersionInfo", versionInfo.Id));

            /*
			 * Meta Info schreibern
			 */
            mapper.BeginTransaction();
            mapper.Delete(typeof(EntityPredicate), deleteCondition);
            mapper.Flush();

            /*
			 * Search virtual links
			 */
            foreach (Type type in types)
            {
                if (Table.GetTableInstance(type)
                        .IsAccessible(mapper.TransactionContext.DatabaseMajorVersion,
                                      mapper.TransactionContext.DatabaseMinorVersion) == false)
                    continue;

                var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
                IDictionary fields = projection.GetFieldTemplates(false);

                foreach (FieldDescription description in fields.Values)
                {
                    var vfd = description as VirtualFieldDescription;

                    if (vfd != null)
                    {
                        var predicate = new EntityPredicate(versionInfo, vfd);
                        if (!uniqueEntries.Contains(predicate))
                        {
                            mapper.Save(predicate);
                            uniqueEntries.Add(predicate);
                        }
                    }
                }
            }

            /*
			 * Commit all
			 */
            try
            {
                mapper.Commit();
            }
            catch (Exception)
            {
                mapper.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Gets the foreign key for user defines.
        /// </summary>
        /// <param name="versionInfo">The version info.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <returns></returns>
        protected virtual List<EntityRelation> GetForeignKeyForUserDefinedEntityRelations(VersionInfo versionInfo,
                                                                                          ObjectMapper mapper,
                                                                                          String tableName,
                                                                                          IDictionary fieldTemplates)
        {
            var relations = new List<EntityRelation>();
            var keyGroupConstraints = new Hashtable();

            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
                 * Is there a unique field?
                 */
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsForeignKey))
                {
                    string constraint = field.Name;

                    /*
                     * If the property contains a single unique key
                     */
                    ForeignKeyGroup defaultGroup = field.CustomProperty.GetForeignKeyDefaultGroup;
                    if (defaultGroup != null)
                    {
                        var relation = new EntityRelation();
                        relation.Initialize(versionInfo,
                                            defaultGroup.ForeignTable,
                                            defaultGroup.ForeignColumn,
                                            Table.GetTableInstance(field.ParentType).DefaultName,
                                            constraint, EntityRelation.OrmType.Association
                            );
                        relations.Add(relation);
                    }

                    /*
                     * Step through all other key groups, and gather the fields
                     */
                    IEnumerator enumerator = field.CustomProperty.MetaInfo.ForeignKeyGroups.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var group = (ForeignKeyGroup)enumerator.Current;
                        if (group.Number > 0)
                        {
                            if (keyGroupConstraints[group.Number] == null)
                                keyGroupConstraints[group.Number] = new SortedList();

                            int position = ((SortedList)keyGroupConstraints[group.Number]).Count;
                            if (group.Ordering > 0) position = group.Ordering;
                            group.Column = constraint;
                            ((SortedList)keyGroupConstraints[group.Number]).Add(position, group);
                        }
                    }
                }
            }

            /*
             * Now add the combined unique keys
             */
            foreach (SortedList sortedConstraint in keyGroupConstraints.Values)
            {
                var constraint = new StringBuilder();
                var foreignColumns = new StringBuilder();
                string foreignTable = string.Empty;
                bool first = true;
                foreach (DictionaryEntry entry in sortedConstraint)
                {
                    if (!first) constraint.Append(", ");
                    if (!first) foreignColumns.Append(", ");
                    var group = (ForeignKeyGroup)entry.Value;

                    constraint.Append(group.Column);
                    foreignColumns.Append(group.ForeignColumn);
                    foreignTable = group.ForeignTable;
                    first = false;
                }

                var relation = new EntityRelation();
                relation.Initialize(versionInfo,
                                    foreignTable,
                                    foreignColumns.ToString(),
                                    tableName,
                                    constraint.ToString(), EntityRelation.OrmType.Association
                    );
                relations.Add(relation);
            }

            return relations;
        }


        #endregion


    }
}
