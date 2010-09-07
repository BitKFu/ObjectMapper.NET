using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Linq;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
#if VS2008

#endif

namespace AdFactum.Data.Internal
{
    ///<summary>
    /// Persistentes Objekt mit Funktionen zum Speichern in der Datenbank
    ///</summary>
    [Serializable]
    public sealed class PersistentObject : IModification
    {
        private readonly PersistentProperties properties;

        /// <summary>
        /// Typ des zu speichernden Fieldvalue Objects
        /// </summary>
        private ProjectionClass projection;

        /// <summary>
        /// True, wenn es sich um ein neu angelegtes Persistenzobjekt handelt
        /// </summary>
        private bool isNew;

        /// <summary>
        /// Zeigt an, ob das Persistenzobjekt flach geladen wurde
        /// </summary>
        private bool isFlatLoaded;

        private object temporaryCreated = null;

        /// <summary>
        /// Gets the temporary created.
        /// </summary>
        /// <returns></returns>
        public object GetTemporaryCreated()
        {
            var result = temporaryCreated;
            temporaryCreated = null;
            return result;
        }

        /// <summary>
        /// Intern genutzter Konstruktor
        /// </summary>
        public PersistentObject()
        {
            var vo = new ValueObject();
            properties = new PersistentProperties();

//            projection = ReflectionHelper.GetProjection(vo.GetType(), null);
            Id = vo.Id;
            IsNew = true;
            IsModified = true;
            IsFlatLoaded = true;
        }

        /// <summary>
        /// Creates an persistent object with the mapper, object type and object id informations
        /// </summary>
        /// <param name="projection">Object type</param>
        /// <param name="objectId">Object Id</param>
        public PersistentObject(ProjectionClass projection, object objectId)
        {
            this.projection = projection;

            var idFieldDescription = projection.GetPrimaryKeyDescription();
            properties = new PersistentProperties();
            properties.FieldProperties = properties.FieldProperties.Add(idFieldDescription.Name, new Field(idFieldDescription, objectId));

            Id = objectId;
            IsNew = true;
            IsModified = true;
            IsFlatLoaded = true;
        }

        /// <summary>
        /// Constructor that initialized a persistent object with a value object
        /// </summary>
        /// <param name="pVo">Fieldvalue object</param>
        /// <param name="mapper">Database Mapper</param>
        /// <param name="hash">Object Hash</param>
        /// <param name="hierarchyLevel">Maximum Save Deep</param>
        /// <param name="recursionTest">The recursion test.</param>
        /// <param name="secondStepUpdate">if set to <c>true</c> [second step update].</param>
        public PersistentObject(
            IValueObject pVo,
            ObjectMapper mapper,
            ObjectHash hash,
            int hierarchyLevel,
            ConstraintSaveList recursionTest,
            ref bool secondStepUpdate
            )
        {
            properties = new PersistentProperties();
            PrivateUpdate(pVo, mapper, hash, hierarchyLevel, null, recursionTest, ref secondStepUpdate, false);

            IsNew = true;
            IsModified = true;
            IsFlatLoaded = HierarchyLevel.IsFlatLoaded(hierarchyLevel);
        }

        /// <summary>
        /// Constructor that loads the persistent object with given felddefinitions, but without any values.
        /// </summary>
        public PersistentObject(
            ProjectionClass projection,
            bool flatLoaded,
            PersistentProperties fieldProperties,
            object pId
            )
        {
            Debug.Assert((projection != null), "No typename defined");
            Debug.Assert(fieldProperties != null, "No properties defined");

            this.projection = projection;
            Id = pId;
            properties = fieldProperties;

            IsNew = false;
            IsModified = false;
            IsFlatLoaded = flatLoaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentObject"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public PersistentObject(
            PersistentObject source
            )
        {
            properties = (PersistentProperties) source.properties.Clone();
            projection = source.projection;
            Id = source.Id;
            IsNew = false;
            IsDeleted = false;
            IsModified = false;
            IsFlatLoaded = source.IsFlatLoaded;
        }

        /// <summary>
        /// Method to update an existing persistent object.
        /// </summary>
        /// <param name="vo">Fieldvalue Object</param>
        /// <param name="mapper">Database Mapper object</param>
        /// <param name="hash">Object Hash is used for caching strategy</param>
        /// <param name="hierarchyLevel">Maximum hierarchie level for updating the persistent object.</param>
        /// <param name="recursionTest">The recursion test.</param>
        /// <param name="secondStepUpdate">if set to <c>true</c> [second step update].</param>
        /// <remarks>It's mandatory that the persistent object and the given value object has the same ID</remarks>
        public void Update(
            IValueObject vo,
            ObjectMapper mapper,
            ObjectHash hash,
            int hierarchyLevel,
            ConstraintSaveList recursionTest,
            ref bool secondStepUpdate
            )
        {
            PrivateUpdate(vo, mapper, hash, hierarchyLevel, null, recursionTest, ref secondStepUpdate, false);
        }


        public void UpdateOneToMany(
            IValueObject vo,
            ObjectMapper mapper,
            ObjectHash hash,
            int hierarchyLevel,
            ConstraintSaveList recursionTest,
            ref bool secondStepUpdate
            )
        {
            if (HierarchyLevel.IsFlatLoaded(hierarchyLevel))
                return;

            PrivateUpdate(vo, mapper, hash, hierarchyLevel, null, recursionTest, ref secondStepUpdate, true);
        }

        /// <summary>
        /// Update an internal persistent object property
        /// </summary>
        internal void InternalUpdateField(
            PropertyInfo property,
            Property propertyCustomInfo,
            VirtualLinkAttribute virtualLinkCustomInfo,
            object fieldObject,
            ObjectMapper mapper,
            ObjectHash hash,
            int hierarchyLevel,
            IDictionary globalParameter,
            ConstraintSaveList recursionTest,
            ref bool secondStepUpdate,
            bool postUpdate
            )
        {
            //string columnName = mapper.Persister.TypeMapper.DoCasing(propertyCustomInfo.MetaInfo.ColumnName);
            string columnName = propertyCustomInfo.MetaInfo.ColumnName;

            /*
			 * Soll ein tiefen Speichern stattfinden?
			 */
            if (hierarchyLevel > 0)
            {
                /*
                 * Is it a list type? 
                 */
                if (postUpdate) // that's for all one to many updates
                {
                    // Update One to many after saving the main object
                    if (property.PropertyType.IsListType())
                    {
                        // Not One to many associations will be handled in pre part (see below)
                        if (!propertyCustomInfo.MetaInfo.IsOneToManyAssociation)
                            return;

                        /*
                         * If list is not initialized, do it now with a standard
                         */
                        if (fieldObject == null)
                            fieldObject = new ArrayList();

                        /*
                         * Existiert schon eine Liste für einen Listentyp ? 
                         */
                        Dictionary<string, IModification> propertyList;
                        if (!properties.ListProperties.TryGetValue(columnName, out propertyList))
                        {
                            propertyList = new Dictionary<string, IModification>();
                            properties.ListProperties = properties.ListProperties.Add(columnName, propertyList);
                        }

                        /*
                         * Alle Elemente des Dictionarys auf nicht validiert setzen
                         */
                        foreach (var entry in propertyList)
                            entry.Value.IsDeleted = true;

                        /*
                         * Alle Elemente der Liste iterieren
                         */
                        IEnumerator enumerator = ((IEnumerable)fieldObject).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (!(enumerator.Current is IValueObject))
                                throw new WrongTypeException(typeof(IValueObject), enumerator.Current.GetType());

                            var tempVo = (IValueObject)enumerator.Current;
                            PersistentObject po = mapper.PrivateSave(tempVo, hash,
                                                                     Math.Max(HierarchyLevel.DecLevel(hierarchyLevel), 0),
                                                                     globalParameter, recursionTest,
                                                                     ref secondStepUpdate);

                            /*
                             * Existiert der Counter Eintrag bereits ? 
                             */
                            string key = ConstraintSaveList.CalculateKey(tempVo);
                            if (!propertyList.ContainsKey(key))
                            {
                                Property joinProperty = Property.GetPropertyInstance(
                                    propertyCustomInfo.MetaInfo.LinkTarget.GetPropertyInfo(propertyCustomInfo.MetaInfo.LinkedTargetProperty));

                                var joinField = po.Properties.FieldProperties.Get(joinProperty.MetaInfo.ColumnName) as Field;
                                if (joinField == null)
                                {
                                    // Maybe we have a deepload in cache
                                    var joinLink = po.Properties.FieldProperties.Get(joinProperty.MetaInfo.ColumnName) as SpecializedLink;
                                    if (joinLink != null)
                                        joinField = joinLink.Property;
                                }

                                /*
                                 * Einen neuen anlegen und einfügen
                                 */
                                var dictionaryLink = new OneToManyLink( new FieldDescription(columnName, ObjectType, property.PropertyType, false), joinField);
                                dictionaryLink.SetLinkedObject(po, hash, mapper);
                                propertyList.Add(key, dictionaryLink);
                            }
                            else
                            {
                                ((OneToManyLink)propertyList[key]).IsDeleted = false;
                                ((OneToManyLink)propertyList[key]).IsModified = false;
                            }
                        }


                        /*
                         * Alle nicht validen Objekte zur Löschung freigeben 
                         */
                        foreach (KeyValuePair<string, IModification> entry in propertyList)
                        {
                            var dictionaryLink = (OneToManyLink)entry.Value;
                            if (dictionaryLink.IsDeleted)
                            {
                                mapper.Delete((IValueObject) dictionaryLink.Property.Value);
                            }
                        }

                        return;
                    }
                }
                else
                {
                    /*
                     * Handelt es sich um eine Feld - Wert Zuordnung ?
                     */
                    if (property.PropertyType.IsDictionaryType())
                    {
                        /*
                         * If list is not initialized, do it now with a standard
                         */
                        if (fieldObject == null)
                            fieldObject = new Hashtable();

                        /*
                         * Existiert schon eine Liste für einen Listentyp ? 
                         */
                        Dictionary<object, IModification> propertyList;
                        if (!properties.DictProperties.TryGetValue(columnName, out propertyList))
                        {
                            propertyList = new Dictionary<object, IModification>();
                            properties.DictProperties = properties.DictProperties.Add(columnName, propertyList);
                        }

                        /*
                         * Alle Elemente des Dictionarys auf nicht validiert setzen
                         */
                        foreach (var entry in propertyList)
                            entry.Value.IsDeleted = true;

                        /*
                         * Alle Elemente der Liste iterieren
                         */
                        IDictionaryEnumerator enumerator = ((IDictionary) fieldObject).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Debug.Assert(enumerator.Value is IValueObject,
                                         "A IValueObject is expected - found: " + enumerator.Value.GetType().FullName);
                            if (!(enumerator.Value is IValueObject))
                                throw new WrongTypeException(typeof (IValueObject), enumerator.Value.GetType());

                            object key = enumerator.Key;
                            var tempVo = (IValueObject) enumerator.Value;
                            PersistentObject po = mapper.PrivateSave(tempVo, hash,
                                                                     HierarchyLevel.DecLevel(hierarchyLevel),
                                                                     globalParameter, recursionTest,
                                                                     ref secondStepUpdate);

                            /*
                             * Existiert der Counter Eintrag bereits ? 
                             */
                            ListLink dictionaryLink;
                            if (propertyList.ContainsKey(key))
                            {
                                /*
                                 * Dann den bestehenden Eintrag überschreiben
                                 */
                                dictionaryLink = (ListLink) propertyList[key];
                                dictionaryLink.SetLinkedObject(po, hash, mapper);
                            }
                            else
                            {
                                /*
                                 * Einen neuen anlegen und einfügen
                                 */
                                dictionaryLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget, this, po, key);
                                propertyList.Add(key, dictionaryLink);
                            }
                        }

                        /*
                         * Alle nicht validen Objekte zur Löschung freigeben 
                         */
                        foreach (KeyValuePair<object, IModification> entry in propertyList)
                        {
                            var dictionaryLink = (ListLink) entry.Value;
                            if (dictionaryLink.IsDeleted)
                                dictionaryLink.SetLinkedObject(null, hash, mapper);
                        }

                        return;
                    }

                    /* 
                     * Handelt es sich um eine Feldliste ? 
                     */
                    if (property.PropertyType.IsListType())
                    {
                        // One to many associations will be handled in post part (see above)
                        if (propertyCustomInfo.MetaInfo.IsOneToManyAssociation)
                            return;

                        /*
                         * If list is not initialized, do it now with a standard
                         */
                        if (fieldObject == null)
                            fieldObject = new ArrayList();

                        /*
                         * Existiert schon eine Liste für einen Listentyp ? 
                         */
                        Dictionary<string, IModification> propertyList;
                        if (!properties.ListProperties.TryGetValue(columnName, out propertyList))
                        {
                            propertyList = new Dictionary<string, IModification>();
                            properties.ListProperties = properties.ListProperties.Add(columnName, propertyList);
                        }

                        /*
                         * Alle Elemente des Dictionarys auf nicht validiert setzen
                         */
                        foreach (var entry in propertyList)
                            entry.Value.IsDeleted = true;

                        /*
                         * Alle Elemente der Liste iterieren
                         */
                        IEnumerator enumerator = ((IEnumerable) fieldObject).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (!(enumerator.Current is IValueObject))
                                throw new WrongTypeException(typeof (IValueObject), enumerator.Current.GetType());

                            var tempVo = (IValueObject) enumerator.Current;
                            PersistentObject po = mapper.PrivateSave(tempVo, hash,
                                                                     HierarchyLevel.DecLevel(hierarchyLevel),
                                                                     globalParameter, recursionTest,
                                                                     ref secondStepUpdate);

                            /*
                             * Existiert der Counter Eintrag bereits ? 
                             */
                            string key = ConstraintSaveList.CalculateKey(tempVo);
                            if (!propertyList.ContainsKey(key))
                            {
                                /*
                                 * Einen neuen anlegen und einfügen
                                 */
                                var dictionaryLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget != null, this, po);
                                propertyList.Add(key, dictionaryLink);
                            }
                            else
                            {
                                ((ListLink)propertyList[key]).IsDeleted = false;
                                ((ListLink) propertyList[key]).IsModified = false;
                            }
                        }


                        /*
                         * Alle nicht validen Objekte zur Löschung freigeben 
                         */
                        foreach (var entry in propertyList)
                        {
                            var dictionaryLink = (ListLink) entry.Value;
                            if (dictionaryLink.IsDeleted)
                                dictionaryLink.SetLinkedObject(null, hash, mapper);
                        }

                        return;
                    }

                    /*
                    * Handelt es sich bei dem Feld um eine Instanz von Fieldvalue Object?
                    */
                    if (property.PropertyType.IsValueObjectType())
                    {
                        /*
                        * Wenn ja, dann als nestedVO setzen und in die Linkliste mit aufnehmen
                        */
                        var tempVo = (IValueObject) fieldObject;
                        PersistentObject po = mapper.PrivateSave(tempVo, hash, HierarchyLevel.DecLevel(hierarchyLevel),
                                                                 globalParameter, recursionTest, ref secondStepUpdate);

                        /*
                        * Existiert das Property bereits, dann nur die Verknüpfung aktualisieren ? 
                        */
                        IModification field;
                        if (properties.FieldProperties.TryGetValue(columnName, out field) && !(field is UnmatchedField))
                        {
                            var link = field as Link;
                            if (link != null)
                                link.SetLinkedObject(po, hash, mapper);
                            else
                            {
                                var specializedLink = field as SpecializedLink;
                                if (specializedLink != null)
                                    specializedLink.SetLinkedObject(po, hash, mapper);
                            }
                        }
                        else
                        {
                            IModification link;
                            FieldDescription fdesc = ReflectionHelper.GetStaticFieldTemplate(
                                propertyCustomInfo.ComponentType, propertyCustomInfo.MetaInfo.PropertyName);

                            if (propertyCustomInfo.MetaInfo.IsGeneralLinked)
                            {
                                link = po == null
                                           ? new Link(fdesc)
                                           : new Link(fdesc, po.Id, po.ObjectType.FullName);
                            }
                            else
                            {
                                link = po == null
                                           ? new SpecializedLink(fdesc)
                                           : new SpecializedLink(fdesc, po.Id);
                            }

                            if (field != null)
                                properties.FieldProperties.Remove(columnName);
                            properties.FieldProperties = properties.FieldProperties.Add(columnName, link);
                        }

                        return;
                    }
                }
            }
            else
            {
                /*
				 * Die anderen Punkte überspringen beim Flat Load, wenn es sich um eine Liste oder Dictionary handelt
				 */
                if (property.PropertyType.IsComplexType())
                    return;
            }

            /*
             * Check the hierarchy level, if it's 1 - than return without doing anything
             * 
             * Explanation: 
             *  -> Fields are only taken if the hirarchy level is greater 1 or the level is 0
             *     This is because we must define if only the link, or the complete must be updated.
             * 
             *      0 -> only the flat object with it's properties
             *      1 -> the flat object and the first link level
             *      2 -> the flat object, the first link level and the dependend object
             *      3 -> the flat object, the first link level, the dependend object, the second link level 
             *      4 -> ...
             *      
             *  Also the PostUpdate execution is not allowed to change fields
             */
            if (HierarchyLevel.StoreOnlyLinks(hierarchyLevel) || postUpdate)
                return;

            /*
		 	 * Wenn nein, dann in die reguläre Feldliste aufnehmen
			 */
            if (virtualLinkCustomInfo == null)
            {
                /*
			 	 * Existiert das Property bereits, dann nur den Feldwert ändern
				 */
                IModification modificatedField;
                Field field;
                if (properties.FieldProperties.TryGetValue(columnName, out modificatedField))
                {
                    field = modificatedField as Field;
                    if (field != null)
                        field.Value = fieldObject;

                    return;
                }

                field = new Field(
                    new FieldDescription(columnName, ObjectType, typeof (Field), property.PropertyType,
                                         propertyCustomInfo, false), fieldObject);
                properties.FieldProperties = properties.FieldProperties.Add(columnName, field);
            }
            else
            {
                if (!properties.FieldProperties.Contains(columnName))
                {
                    /*
				 	 * Es handelt sich um einen virtuellen Link
					 */
                    var field = new VirtualField(
                        new VirtualFieldDescription(projection.ProjectedType, columnName, property.PropertyType, propertyCustomInfo,
                                                    virtualLinkCustomInfo),
                        fieldObject);
                    properties.FieldProperties = properties.FieldProperties.Add(columnName, field);
                }
            }
        }

        /// <summary>
        /// Updates the references.
        /// </summary>
        /// <param name="updateVo">The update vo.</param>
        public void UpdateAutoincrementedIds(IValueObject updateVo)
        {
            var templates = projection.GetFieldTemplates(true).Union(projection.GetFieldTemplates(false));

            foreach (var template in templates)
            {
                Property propertyCustomInfo = template.Value.CustomProperty;

                if ((propertyCustomInfo == null) || (!propertyCustomInfo.MetaInfo.IsAccessible()))
                    continue;

                /*
                 * If it's a value object
                 */
                if (propertyCustomInfo.PropertyType.IsValueObjectType())
                {
                    /*
                     * Update reference
                     */
                    string propertyName = propertyCustomInfo.MetaInfo.ColumnName;
                    var referencedVo = (IValueObject) propertyCustomInfo.GetValue(updateVo);

                    IModification field;
                    if (properties.FieldProperties.TryGetValue(propertyName, out field) && !(field is UnmatchedField))
                    {
                        var link = field as Link;
                        if (link != null)
                            link.UpdateAutoincrementedId(referencedVo);
                        else
                        {
                            var specializedLink = field as SpecializedLink;
                            if (specializedLink != null)
                                specializedLink.UpdateAutoincrementedId(referencedVo);
                        }
                    }

                    continue;
                }

                /*
                 * If it's a list type
                 */
                if (propertyCustomInfo.PropertyType.IsListType())
                {
                    /*
                     * Update reference(s)
                     */
                    string propertyName = propertyCustomInfo.MetaInfo.ColumnName;

                    Dictionary<string, IModification> propertyList;
                    properties.ListProperties.TryGetValue(propertyName, out propertyList);
                    var referencedList = (IList) propertyCustomInfo.GetValue(updateVo);

                    if ((referencedList == null) || (propertyList == null))
                        continue;

                    foreach (IValueObject vo in referencedList)
                    {
                        if (vo.Id != null)
                        {
                            string key = ConstraintSaveList.CalculateKey(vo.GetType(), vo.Id);

                            if (propertyList.ContainsKey(key))
                            {
                                var listLink = propertyList[key] as ListLink;
                                if (listLink != null)
                                    listLink.UpdateAutoincrementedId(vo);

                                continue;
                            }
                        }

                        if (vo.InternalId != Guid.Empty)
                        {
                            string key = ConstraintSaveList.CalculateKey(vo.GetType(), vo.InternalId);

                            if (propertyList.ContainsKey(key))
                            {
                                var listLink = propertyList[key] as ListLink;
                                if (listLink != null)
                                    listLink.UpdateAutoincrementedId(vo);

                                continue;
                            }
                        }
                    }

                    continue;
                }

                /*
                 * If it's a dictionary type
                 */
                if (propertyCustomInfo.PropertyType.IsDictionaryType())
                {
                    /*
                     * Update reference(s)
                     */
                    string propertyName = propertyCustomInfo.MetaInfo.ColumnName;
                    Dictionary<object, IModification> propertyList;
                    properties.DictProperties.TryGetValue(propertyName, out propertyList);
                    var referencedList = (IDictionary) propertyCustomInfo.GetValue(updateVo);

                    if ((referencedList == null) || (propertyList == null))
                        continue;

                    foreach (DictionaryEntry de in referencedList)
                        ((ListLink) propertyList[de.Key]).UpdateAutoincrementedId((IValueObject) de.Value);

                    continue;
                }
            }
        }

        /// <summary>
        /// Updates all properties of the Persistent Object with the values taken from the Value Object
        /// </summary>
        private void PrivateUpdate(
            IValueObject pVo,
            ObjectMapper mapper,
            ObjectHash hash,
            int hierarchyLevel,
            IDictionary globalParameter,
            ConstraintSaveList recursionTest,
            ref bool secondStepUpdate,
            bool postUpdate
            )
        {
            projection = projection ?? ReflectionHelper.GetProjection(pVo.GetType(), mapper.MirroredLinqProjectionCache);
            Id = pVo.Id;

            /*
			 * Setzt das Flag, wenn das Persistenzobjekt nur Felder und keine Verknüpfungen enthält.
			 * Bei dieser Kombination darf das Persistenzobjekt nicht in den primären Hash
			 * gemerged werden.
			 */
            isFlatLoaded = HierarchyLevel.IsFlatLoaded(hierarchyLevel);

            /*
			 * Alle Attribute auslesen
			 */
            Dictionary<PropertyInfo, Property>.Enumerator counter =
                Property.GetPropertyInstances(pVo.GetType()).GetEnumerator();

            while (counter.MoveNext())
            {
                PropertyInfo property = counter.Current.Key;
                Property propertyCustomInfo = counter.Current.Value;
                VirtualLinkAttribute virtualLinkCustomInfo = ReflectionHelper.GetVirtualLinkInstance(property);

                /*
				 * Eventuell die Eigenschaft überlesen, wenn der Zugriff verweigert wird
				 */
                if (propertyCustomInfo != null)
                {
                    if (!propertyCustomInfo.MetaInfo.IsAccessible(((ITransactionContext) mapper).DatabaseMajorVersion,
                                                                  ((ITransactionContext) mapper).DatabaseMinorVersion))
                        continue;

                    object fieldObject = null;
                    try
                    {
                        fieldObject = propertyCustomInfo.GetValue(pVo);
                    }
                    catch (NullReferenceException)
                    {
                    }

                    InternalUpdateField(
                        property,
                        propertyCustomInfo,
                        virtualLinkCustomInfo,
                        fieldObject,
                        mapper,
                        hash,
                        hierarchyLevel,
                        globalParameter,
                        recursionTest,
                        ref secondStepUpdate, 
                        postUpdate);
                }
            }
        }


        /// <summary>
        /// Creates a new value object based on the current persistent object.
        /// </summary>
        /// <param name="mapperObj">Database Mapper</param>
        /// <param name="objectFactory">Object Factory that creates a new object instance</param>
        /// <param name="hash">Object hash for caching algorithm</param>
        /// <param name="hierarchyLevel">Maximum hierarchie level for creating the object</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns></returns>
        public object CreateVO(ObjectMapper mapperObj, IObjectFactory objectFactory, ObjectHash hash, int hierarchyLevel,
                               IDictionary globalParameter)
        {
            object created = projection.Constructor == null || projection.Constructor.GetParameters().Length == 0
                ? privateCreateVO(objectFactory, hash) : null;
            var visitedProperties = new Dictionary<string, object>();
            var templates = projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel));

            foreach (var template in templates)
            {
                var columnName = template.Key;
                var fd = template.Value;

                if (visitedProperties.ContainsKey(columnName))
                    continue;

                Property propertyCustomInfo = fd != null ? fd.CustomProperty : null;

                /*
				 * Eventuell die Eigenschaft überlesen, wenn der Zugriff verweigert wird
				 */
                if (propertyCustomInfo == null ||
                    (!propertyCustomInfo.MetaInfo.IsAccessible(((ITransactionContext) mapperObj).DatabaseMajorVersion,
                                                               ((ITransactionContext) mapperObj).DatabaseMinorVersion)))
                    continue;

                IModification itCurrent;
                if (properties.FieldProperties.TryGetValue(columnName, out itCurrent)
                    && string.Equals(itCurrent.PropertyName, propertyCustomInfo.Name))
                {
                    var field = itCurrent as Field;
                    if (field != null)
                    {
                        if (created == null)
                            visitedProperties.Add(columnName, mapperObj.Persister.TypeMapper.ConvertToType(propertyCustomInfo.PropertyType, field.Value));
                        else
                            propertyCustomInfo.SetValue(created, mapperObj.Persister.TypeMapper.ConvertToType(propertyCustomInfo.PropertyType, field.Value));
                        continue;
                    }

                    var virtualField = itCurrent as VirtualField;
                    if (virtualField != null)
                    {
                        if (created == null)
                            visitedProperties.Add(columnName, mapperObj.Persister.TypeMapper.ConvertToType(propertyCustomInfo.PropertyType, virtualField.Value));
                        else
                            propertyCustomInfo.SetValue(created, mapperObj.Persister.TypeMapper.ConvertToType(propertyCustomInfo.PropertyType, virtualField.Value));
                        continue;
                    }

                    /*
                     * Unmatched Fields may be used to select nested object, even without a hierarchy level
                     */
                    var unmatchedField = itCurrent as UnmatchedField;
                    if (unmatchedField != null)
                    {
                        visitedProperties.Add(columnName, unmatchedField.Fieldvalue);
                        continue;
                    }

                    /*
                     * Bei einer flachen Ladeoperation nur die Felder laden, nicht die Links
                     */
                    if (HierarchyLevel.IsFlatLoaded(hierarchyLevel) || ObjectType.IsReadOnlyType())
                        continue;

                    /*
                     * Hat das Objekt verschachtelte VOs ? 
                     */
                    var link = itCurrent as Link;
                    if (link != null)
                    {
                        /*
                         * Muss ein Objekt geladen werden ? 
                         */
                        if (link.LinkedTo.Value == null)
                        {
                            if (created == null)
                                visitedProperties.Add(columnName, null);
                            else
                                propertyCustomInfo.SetValue(created, null);
                        }
                        else
                        {
                            ProjectionClass linkedProjection = ReflectionHelper.GetProjection(
                                mapperObj.ObjectFactory.Create((string) link.LinkedTo.Value).GetType(),
                                mapperObj.MirroredLinqProjectionCache);

                            object loadedVo =
                                mapperObj.PrivateLoad(
                                    linkedProjection,
                                    link.Property.Value, hash, HierarchyLevel.DecLevel(hierarchyLevel), globalParameter);
                            if (created == null)
                                visitedProperties.Add(columnName, loadedVo);
                            else
                                propertyCustomInfo.SetValue(created, loadedVo);
                        }

                        continue;
                    }

                    /*
                     * Hat das Objekt verschachtelte VOs ? 
                     */
                    var oneToManyLink = itCurrent as OneToManyLink;
                    var specializedLink = oneToManyLink == null ? itCurrent as SpecializedLink : null;
                    if (specializedLink != null)
                    {
                        if (specializedLink.Property.Value == null)
                        {
                            if (created == null)
                                visitedProperties.Add(columnName, null);
                            else
                                propertyCustomInfo.SetValue(created, null);
                        }
                        else
                        {
                            ProjectionClass linkedProjection = ReflectionHelper.GetProjection(
                                propertyCustomInfo.MetaInfo.LinkTarget,
                                mapperObj.MirroredLinqProjectionCache);

                            object loadedVo = mapperObj.PrivateLoad(
                                linkedProjection,
                                specializedLink.Property.Value, hash,
                                HierarchyLevel.DecLevel(hierarchyLevel), globalParameter);

                            if (created == null)
                                visitedProperties.Add(columnName, loadedVo);
                            else
                                propertyCustomInfo.SetValue(created, loadedVo);
                        }

                        continue;
                    }
                }

                if (HierarchyLevel.IsFlatLoaded(hierarchyLevel))
                    continue;

                /*
				 * Handelt es sich um eine Liste (Collection) mit OneToMany Verknüpfung?
				 */
                IList resultPOs;
                if (propertyCustomInfo.PropertyType.IsListType() && propertyCustomInfo.MetaInfo.IsOneToManyAssociation)
                {
                    IList propertyValue = null;
                    if (created != null)
                        propertyValue = (IList)propertyCustomInfo.GetValue(created);

                    if (propertyValue == null) // ... wenn nein, dann anlegen
                    {
                        propertyValue = (IList)mapperObj.ObjectFactory.Create(propertyCustomInfo.PropertyType);
                        if (created == null)
                            visitedProperties.Add(columnName, propertyValue);
                        else
                            propertyCustomInfo.SetValue(created, propertyValue);
                    }

                    /*
					 * Die Objekte wurden bereits geladen und befinden sich im Speicher
					 */
                    Dictionary<string, IModification> listDictionary;
                    if (properties.ListProperties.TryGetValue(columnName, out listDictionary))
                    {
                        resultPOs = listDictionary.Values.ToList();

                        Type resultType = propertyCustomInfo.MetaInfo.LinkTarget;
                        var resultProjection = ReflectionHelper.GetProjection(resultType, mapperObj.MirroredLinqProjectionCache);

                        /*
						 * Die Liste durchlaufen und die VOs holen
						 */
                        foreach (OneToManyLink oneToMany in resultPOs)
                        {
                            object lvo = hash.GetVO(resultType, oneToMany.Property.Value)
                                         ??
                                         mapperObj.PrivateLoad(resultProjection, oneToMany.Property.Value, hash,
                                                               HierarchyLevel.DecLevel(hierarchyLevel),
                                                               globalParameter);

                            propertyValue.Add(lvo);
                        }
                    }
                    else
                    {
                        object linkKey = ((Field)properties.FieldProperties.Get(projection.GetPrimaryKeyDescription().Name)).Value;

                        ICondition condition = new AndCondition(propertyCustomInfo.MetaInfo.LinkTarget,
                            propertyCustomInfo.MetaInfo.LinkedTargetProperty, QueryOperator.Equals, linkKey);

                        resultPOs = mapperObj.PrivateSelect(
                            propertyCustomInfo.MetaInfo.LinkTarget,
                            condition, null, hash, HierarchyLevel.DecLevel(hierarchyLevel), globalParameter, false
                            );

                        /*
                         * Die Liste durchlaufen und die VOs holen
                         */
                        var propertyList = new Dictionary<string, IModification>(resultPOs.Count);
                        Properties.ListProperties = Properties.ListProperties.Add(columnName, propertyList);
                        foreach (PersistentObject po in resultPOs)
                        {
                            var linkVo = po.GetTemporaryCreated() ?? hash.GetVO(po.ObjectType, po.Id);
                            propertyValue.Add(linkVo);
                            var listLink = new OneToManyLink(new FieldDescription(propertyCustomInfo.Name, propertyCustomInfo.PropertyType, propertyCustomInfo.MetaInfo.LinkTarget, false), linkVo);
                            string key = ConstraintSaveList.CalculateKey(linkVo);
                            propertyList.Add(key, listLink);
                        }
                    }

                    continue;
                }

                /*
				 * Handelt es sich um eine Liste (Collection)
				 */
                if (propertyCustomInfo.PropertyType.IsListType())
                {
                    object primaryKey = ((Field) properties.FieldProperties.Get(projection.GetPrimaryKeyDescription().Name)).Value;
                    string linkTable = Table.GetTableInstance(ObjectType).DefaultName + "_" + columnName;

                    IList propertyValue = null;
                    if (created != null)
                        propertyValue = (IList) propertyCustomInfo.GetValue(created);

                    if (propertyValue == null) // ... wenn nein, dann anlegen
                    {
                        propertyValue = (IList)mapperObj.ObjectFactory.Create(propertyCustomInfo.PropertyType);
                        if (created == null)
                            visitedProperties.Add(columnName, propertyValue);
                        else
                            propertyCustomInfo.SetValue(created, propertyValue);
                    }

                    /*
					 * Die Objekte wurden bereits geladen und befinden sich im Speicher
					 */
                    Dictionary<string, IModification> listDictionary;
                    if (properties.ListProperties.TryGetValue(columnName, out listDictionary))
                    {
                        resultPOs = listDictionary.Values.ToList();

                        /*
						 * Die Liste durchlaufen und die VOs holen
						 */
                       foreach (ListLink listLink in resultPOs)
                       {
                           Type resultType = mapperObj.ObjectFactory.GetType((string) listLink.LinkedTo.Value);
                           ProjectionClass resultProjection = ReflectionHelper.GetProjection(resultType, mapperObj.MirroredLinqProjectionCache);

                           object lvo = hash.GetVO(resultType, listLink.Property.Value)
                                        ??
                                        mapperObj.PrivateLoad(resultProjection, listLink.Property.Value, hash,
                                                              HierarchyLevel.DecLevel(hierarchyLevel),
                                                              globalParameter);

                           propertyValue.Add(lvo);
                       }
                    }
                    else
                    {
                        if ((propertyCustomInfo.MetaInfo.LinkTarget != null) &&
                            (!(mapperObj.Persister is Xml.XmlPersister)))
                        {
                            ICondition condition = new CollectionParentCondition(ObjectType, propertyCustomInfo.Name,
                                                                                 propertyCustomInfo.MetaInfo.LinkTarget,
                                                                                 primaryKey);

                            resultPOs = mapperObj.PrivateSelect(
                                propertyCustomInfo.MetaInfo.LinkTarget,
                                condition, null, hash, HierarchyLevel.DecLevel(hierarchyLevel), globalParameter, false
                                );

                            /*
							 * Die Liste durchlaufen und die VOs holen
							 */
                            var propertyList = new Dictionary<string, IModification>(resultPOs.Count);
                            Properties.ListProperties = Properties.ListProperties.Add(columnName, propertyList);
                            foreach (PersistentObject po in resultPOs)
                            {
                                var linkVo = po.GetTemporaryCreated() ?? hash.GetVO(po.ObjectType, po.Id);
                                propertyValue.Add(linkVo);
                                var listLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget,
                                                            po.ObjectType.FullName, ObjectType, primaryKey, po.Id)
                                                   {IsModified = false};
                                string key = ConstraintSaveList.CalculateKey(linkVo);
                                propertyList.Add(key, listLink);
                            }
                        }
                        else
                        {
                            IList fields = mapperObj.Persister.LoadListChilds(ObjectType, linkTable, primaryKey,
                                                                              propertyCustomInfo.MetaInfo.
                                                                                  LinkedPrimaryKeyType,
                                                                              propertyCustomInfo.MetaInfo.LinkTarget);
                            IEnumerator dicEnum = fields.GetEnumerator();

                            /*
                             * Die Liste durchlaufen und die VOs holen
                             */
                            var propertyList = new Dictionary<string, IModification>(fields.Count);
                            Properties.ListProperties = Properties.ListProperties.Add(columnName, propertyList);
                            while (dicEnum.MoveNext())
                            {
                                var dictionaryLink = dicEnum.Current as ListLink;
                                if (dictionaryLink == null)
                                    continue;

                                Type resultType = mapperObj.ObjectFactory.GetType((string) dictionaryLink.LinkedTo.Value);
                                var resultProjection = ReflectionHelper.GetProjection(resultType, mapperObj.MirroredLinqProjectionCache);

                                var linkVo = (IValueObject) mapperObj.PrivateLoad(
                                                                resultProjection,
                                                                dictionaryLink.Property.Value, hash,
                                                                HierarchyLevel.DecLevel(hierarchyLevel), globalParameter);
                                propertyValue.Add(linkVo);
                                PersistentObject po = hash.Get(resultType, dictionaryLink.Property.Value);
                                if (po != null)
                                {
                                    var listLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget,
                                                                po.ObjectType.FullName, ObjectType, primaryKey, po.Id);
                                    listLink.IsModified = false;

                                    string key = ConstraintSaveList.CalculateKey(linkVo);
                                    propertyList.Add(key, listLink);
                                }
                            }
                        }
                    }

                    continue;
                }

                /*
				 * Handelt es sich um eine Hashtable (Dictionary)
				 */
                if (propertyCustomInfo.PropertyType.IsDictionaryType())
                {
                    object primaryKey = ((Field) properties.FieldProperties.Get(projection.GetPrimaryKeyDescription().Name)).Value;
                    string linkTable = Table.GetTableInstance(ObjectType).DefaultName + "_" + columnName;

                    IDictionary propertyValue = null;
                    if (created != null)
                        propertyValue = (IDictionary) propertyCustomInfo.GetValue(created);

                    if (propertyValue == null) // ... wenn nein, dann anlegen
                    {
                        propertyValue = (IDictionary)mapperObj.ObjectFactory.Create(propertyCustomInfo.PropertyType);
                        if (created == null)
                            visitedProperties.Add(columnName, propertyValue);
                        else
                            propertyCustomInfo.SetValue(created, propertyValue);
                    }

                    /*
					 * Die Objekte wurden bereits geladen und befinden sich im Speicher
					 */
                    Dictionary<object, IModification> resultDictionaryPOs;
                    if (Properties.DictProperties.TryGetValue(columnName, out resultDictionaryPOs))
                    {
                        /*
						 * Die Liste durchlaufen und die VOs holen
						 */
                        if (resultDictionaryPOs != null)
                        {
                            var resultEnum = resultDictionaryPOs.GetEnumerator();
                            while (resultEnum.MoveNext())
                            {
                                var listLink = resultEnum.Current.Value as ListLink;
                                Type resultType = mapperObj.ObjectFactory.GetType((string) listLink.LinkedTo.Value);
                                var resultProjection = ReflectionHelper.GetProjection(resultType, mapperObj.MirroredLinqProjectionCache);
                                object lvo = hash.GetVO(resultType, listLink.Property.Value)
                                             ??
                                             mapperObj.PrivateLoad(resultProjection, listLink.Property.Value, hash,
                                                                   HierarchyLevel.DecLevel(hierarchyLevel),
                                                                   globalParameter);

                                propertyValue.Add(resultEnum.Current.Key, lvo);
                            }
                        }
                    }
                    else
                    {
                        if ((propertyCustomInfo.MetaInfo.LinkTarget != null) &&
                            (!(mapperObj.Persister is Xml.XmlPersister)))
                        {
                            ICondition condition = new HashCondition(ObjectType, propertyCustomInfo.Name,
                                                                     propertyCustomInfo.MetaInfo.LinkTarget, primaryKey);

                            resultPOs = mapperObj.PrivateSelect(
                                propertyCustomInfo.MetaInfo.LinkTarget,
                                condition, null, hash, HierarchyLevel.DecLevel(hierarchyLevel), globalParameter, false
                                );

                            var dictList = new Dictionary<object, IModification>(resultPOs.Count);
                            Properties.DictProperties = Properties.DictProperties.Add(columnName, dictList);
                            foreach (PersistentObject po in resultPOs)
                            {
                                string joinField = ReflectionHelper.GetJoinField(ObjectType, po.ObjectType);
                                object linkId = ((UnmatchedField) po.Properties.FieldProperties.Get(joinField)).Fieldvalue;

                                propertyValue.Add(linkId, po.GetTemporaryCreated() ?? hash.GetVO(po.ObjectType, po.Id));
                                var listLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget, linkId, ObjectType,
                                                            primaryKey, po.Id, po.ObjectType.FullName)
                                                   {IsModified = false};
                                dictList.Add(linkId, listLink);
                            }
                        }
                        else
                        {
                            IDictionary fields = mapperObj.Persister.LoadHashChilds
                                (ObjectType, linkTable, primaryKey,typeof (string),
                                propertyCustomInfo.MetaInfo.LinkedPrimaryKeyType,
                                propertyCustomInfo.MetaInfo.LinkTarget);

                            var dicEnum = fields.GetEnumerator();
                            var dictList = new Dictionary<object, IModification>(fields.Count);
                            Properties.DictProperties = Properties.DictProperties.Add(columnName, dictList);
                            while (dicEnum.MoveNext())
                            {
                                var dictionaryLink = dicEnum.Value as ListLink;
                                object linkId = dicEnum.Key;

                                Type resultType = mapperObj.ObjectFactory.GetType((string) dictionaryLink.LinkedTo.Value);
                                var resultProjection = ReflectionHelper.GetProjection(resultType, mapperObj.MirroredLinqProjectionCache);
                                propertyValue.Add(linkId, mapperObj.PrivateLoad(
                                                              resultProjection,
                                                              dictionaryLink.Property.Value, hash,
                                                              HierarchyLevel.DecLevel(hierarchyLevel), globalParameter));
                                PersistentObject po = hash.Get(resultType, dictionaryLink.Property.Value);
                                var listLink = new ListLink(propertyCustomInfo.MetaInfo.LinkTarget, linkId, ObjectType,
                                                            primaryKey, po.Id, po.ObjectType.FullName)
                                                   {IsModified = false};
                                dictList.Add(linkId, listLink);
                            }
                        }
                    }
                }
            }

            /*
             * Set values to object
             */
            if (created == null)
            {
                /*
                 * Create Object with constructor arguments
                 */
                object[] arguments = projection.CopyOfConstructorParameters;
                for (int x = 0; arguments != null && x < arguments.Length; x++)
                {
                    // The argument is a simple plain property
                    var argument = arguments[x] as Property;
                    if (argument != null)
                    {
                        // Set the property itself, if it's not a value object
                        if (argument.PropertyType.IsValueObjectType())
                            arguments[x] = argument.PropertyType;
                                // if the property is a value object, than go deeper into it
                        else if (visitedProperties.ContainsKey(argument.MetaInfo.ColumnName))
                        {
                            arguments[x] = visitedProperties[argument.MetaInfo.ColumnName];
                            continue;
                        }
                    }

                    // The argument is at least one value object, or a list of it
                    var type = arguments[x] as Type;
                    if (type != null)
                    {
                        // Check, if perhaps the Join is an outer join / left join and not fullfilled at all
                        IModification outerJoinField;
                        var parameterName = projection.NewExpression.GetParameterName(x);
                        parameterName = mapperObj.Persister.TypeMapper.DoCasing(parameterName);
                        if (properties.FieldProperties.TryGetValue(parameterName, out outerJoinField))
                        {
                            if (Convert.ToInt32(((UnmatchedField)outerJoinField).Fieldvalue) == 0)
                            {
                                arguments[x] = null;
                                continue;
                            }
                        }

                        // Create the object and fill it
                        var nestedObject = new PersistentObject(ReflectionHelper.GetProjection(type.RevealType(),mapperObj.MirroredLinqProjectionCache),
                                                                HierarchyLevel.IsFlatLoaded(hierarchyLevel),
                                                                (PersistentProperties) properties.Clone(), null);
                        nestedObject.UpdateProperties(projection.ComplexTypeColumnMapping[x]);

                        object vo = nestedObject.CreateVO(mapperObj, objectFactory, hash, hierarchyLevel,
                                                          globalParameter);
                        if (!type.IsListType())
                            arguments[x] = vo;
                        else
                        {
                            arguments[x] = objectFactory.Create(type);
                            ((IList) arguments[x]).Add(vo);
                        }
                        continue;
                    }
                }
                
                /*
                 * Create the object
                 */
                temporaryCreated = created = privateCreateVO(arguments, hash);

                /*
                 * Now initialize the bindings
                 */
                foreach (var binding in projection.MemberBindings.Keys)
                {
                    if (!visitedProperties.ContainsKey(binding.MetaInfo.ColumnName)) 
                        continue;

                    binding.SetValue(created, visitedProperties[binding.MetaInfo.ColumnName]);
                }
            }
            else
                temporaryCreated = created;

            return created;
        }

        /// <summary>
        /// Updates the properties.
        /// </summary>
        private void UpdateProperties(IEnumerable<ColumnDeclaration> columnMapping)
        {
            var templates = Projection.GetFieldTemplates(isFlatLoaded);

            foreach (var template in templates)
            {
                string key = template.Key;

                // Go trough all the complex type mappings and rebind the key to the correct result
                if (columnMapping != null)
                    foreach (ColumnDeclaration mapping in columnMapping)
                    {
                        var pe = mapping.Expression as PropertyExpression;
                        if (pe == null || template.Value.PropertyName != pe.PropertyName) continue;
                        key = mapping.Alias.Name;
                        break;
                    }

                FieldDescription fd = template.Value;
                
                IModification unmatchedModification;
                properties.FieldProperties.TryGetValue(key, out unmatchedModification);

                object fieldValue = null;
                
                var unmatchedField = unmatchedModification as UnmatchedField;
                if (unmatchedField != null)
                    fieldValue = unmatchedField.Fieldvalue;
                else
                {
                    // In some cases, the field is re-used, and therefore not unmatched. 
                    var matchedField = unmatchedModification as Field;
                    if (matchedField != null) 
                        fieldValue = matchedField.Value;
                }

                properties.FieldProperties.Remove(key);

                // Try to convert unmatched field into fields
                if (fd.FieldType == typeof (Field))
                {
                    var field = new Field(fd, fieldValue);
                    if (unmatchedModification != null)
                        properties.FieldProperties.Remove(template.Key);
                    properties.FieldProperties = properties.FieldProperties.Add(template.Key, field);
                    continue;
                }

                if (fd.FieldType == typeof (SpecializedLink))
                {
                    var specializedLink = new SpecializedLink(fd, fieldValue);
                    if (unmatchedModification != null)
                        properties.FieldProperties.Remove(template.Key);
                    properties.FieldProperties = properties.FieldProperties.Add(template.Key, specializedLink);
                }
            }
        }

        /// <summary>
        /// Creates the VO
        /// </summary>
        private object privateCreateVO(object[] arguments, ObjectHash hash)
        {
            object created = projection.Constructor.Invoke(arguments);
            return InitializeValueObject(created, hash);
        }

        /// <summary>
        /// Creates the value object 
        /// </summary>
        /// <param name="objectFactory"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        private object privateCreateVO(IObjectFactory objectFactory, ObjectHash hash)
        {
            if (ObjectType.IsReadOnlyType())
                return null;

            /*
			 * Ein neues VO anlegen und die Daten setzen
			 */
            var created = objectFactory.Create(ObjectType);
            return InitializeValueObject(created, hash);
        }

        /// <summary>
        /// Initializes the value object.
        /// </summary>
        /// <param name="valueObject">The value object.</param>
        /// <param name="initialValues">The initial values.</param>
        /// <returns></returns>
        private object InitializeValueObject(object valueObject, ObjectHash initialValues)
        {
            initialValues.AddLoad(this, valueObject);

            var ivo = valueObject as IValueObject;
            if (ivo != null)
                ivo.IsNew = false;
            else
                Id = valueObject.GetHashCode();

            /*
			 * Prüfen ob das VO den passenden Typ besitzt.
			 */
            if (!ObjectType.IsGenericType || ObjectType.GetGenericTypeDefinition() != typeof (IQueryable<>))
                if (!ObjectType.IsAssignableFrom(valueObject.GetType()))
                    throw new WrongTypeException(valueObject.GetType(), ObjectType);
            return valueObject;
        }

        /// <summary>
        /// Zugriffsmethode den Objekttyp 
        /// </summary>
        public Type ObjectType
        {
            [DebuggerStepThrough]
            get { return projection.ProjectedType; }
        }

        /// <summary> Gets the projection. </summary>
        public ProjectionClass Projection
        {
            get { return projection; }
        }

        /// <summary>
        /// Zugriffsmethode für die ID
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// true, wenn es sich um ein neu angelegtes Persistenzobjekt handelt
        /// </summary>
        public bool IsNew
        {
            [DebuggerStepThrough]
            get { return isNew; }
            set
            {
                isNew = value;
                properties.IsNew = value;
            }
        }

        /// <summary>
        /// Löscht nicht mehr verwendete Links aus dem Hash
        /// </summary>
        public void ClearUnusedLinks()
        {
            properties.ClearUnusedLinks();
        }

        private delegate void DeleteDelegate(IEnumerator enumerator);

        /// <summary>
        /// Löscht nicht mehr verwendete Links aus dem Hash
        /// </summary>
        public void DeleteAllLinks(ObjectMapper mapper, int hierarchyLevel, ObjectHash tempHash, bool postUpdateDelete)
        {
            /*
             * Create Anonymous Delete Delegate
             */
            DeleteDelegate deleteAll = delegate(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current;

                    if (!postUpdateDelete)
                    {
                        var oneToMany = value as OneToManyLink;
                        if (oneToMany != null)
                        {
                            mapper.DeleteRecursive((IValueObject) oneToMany.Property.Value, HierarchyLevel.DecLevel(hierarchyLevel));
                            continue;
                        }
                    }
                    else
                    {
                        var oneToMany = value as OneToManyLink;
                        if (oneToMany != null)
                            continue;

                        var link = value as ListLink;
                        if (link != null)
                        {
                            PersistentObject deletePOLink = link.SetLinkedObject(null, tempHash, mapper);

                            if (deletePOLink != null)
                            {
                                if (deletePOLink.isDeleted)
                                    deletePOLink.DeleteAllLinks(mapper, hierarchyLevel, tempHash, postUpdateDelete);

                                /*
                                 * Shall linked objects also be deleted?
                                 * But only, if the object type is not marked as [StaticData]
                                 */
                                if (!HierarchyLevel.IsFlatLoaded(hierarchyLevel)
                                    && !Table.GetTableInstance(deletePOLink.ObjectType).IsStatic)
                                    mapper.PrivateDelete(deletePOLink, HierarchyLevel.DecLevel(hierarchyLevel), tempHash);
                            }
                        }

                        /*
                         * Bei einem Link
                         */
                        if (value is Link)
                        {
                            PersistentObject deletePOLink = ((Link) value).SetLinkedObject(null, tempHash, mapper);

                            /*
                             * Shall linked objects also be deleted?
                             * But only, if the object type is not marked as [StaticData]
                             */
                            if (!HierarchyLevel.IsFlatLoaded(hierarchyLevel)
                                && deletePOLink != null
                                && !Table.GetTableInstance(deletePOLink.ObjectType).IsStatic)
                                mapper.PrivateDelete(deletePOLink, HierarchyLevel.DecLevel(hierarchyLevel), tempHash);

                            continue;
                        }

                        /*
                         * Bei einem Link
                         */
                        if (value is SpecializedLink)
                        {
                            PersistentObject deletePOLink = ((SpecializedLink) value).SetLinkedObject(null, tempHash,
                                                                                                      mapper);

                            /*
                             * Sollen auch die gelinkten Objekte gelöscht werden?
                             */
                            if (!HierarchyLevel.IsFlatLoaded(hierarchyLevel)
                                && deletePOLink != null
                                && !Table.GetTableInstance(deletePOLink.ObjectType).IsStatic)
                                mapper.PrivateDelete(deletePOLink, HierarchyLevel.DecLevel(hierarchyLevel), tempHash);

                            continue;
                        }
                    }
                }
            };

            // All Elements except fields can contain links
            if (properties.FieldProperties != null && postUpdateDelete)             // only visit fields, on postupdate
                deleteAll(properties.FieldProperties.Values().GetEnumerator());

            if (properties.ListProperties != null)                                  // only Lists can contain OneToMany Links, that may of 
            {                                                                       // interesset in preupdate (!postupdate)
                var enumerator = properties.ListProperties.GetEnumerator();
                while (enumerator.MoveNext())
                    deleteAll(enumerator.Current.Value.Values.GetEnumerator());
            }

            if (properties.DictProperties != null && postUpdateDelete)              // only visit dictionaries, on postupdate
            {
                var enumerator = properties.DictProperties.GetEnumerator();
                while (enumerator.MoveNext())
                    deleteAll(enumerator.Current.Value.Values.GetEnumerator());
            }
        }

        /// <summary>
        /// Returns the propertyname
        /// </summary>
        string IModification.PropertyName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gibt true zurück, wenn das Objekt gespeichert werden muss
        /// </summary>
        /// <returns></returns>
        public bool IsModified
        {
            get
            {
                return properties.IsModifiedOrNew();
            }

            set
            {
                properties.IsModified = value;
            }
        }

        private bool isDeleted;

        /// <summary>
        /// Returns true, if the persistent object is marked as deleted
        /// </summary>
        public bool IsDeleted
        {
            [DebuggerStepThrough]
            get { return isDeleted; }
            set { isDeleted = value; }
        }


        /// <summary>
        /// Gibt die Hashtable der Propertys zurück
        /// </summary>
        public PersistentProperties Properties
        {
            [DebuggerStepThrough]
            get { return properties; }
        }

        /// <summary>
        /// Gibt TRUE zurück, wenn das Persistenzobjekt flach geladen wurde
        /// </summary>
        public bool IsFlatLoaded
        {
            [DebuggerStepThrough]
            get { return isFlatLoaded; }
            set { isFlatLoaded = value; }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }
}