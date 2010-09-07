using System;
using System.Collections;
using System.Collections.Generic;
using AdFactum.Data.Fields;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// The object hash is used for implementing a caching strategy in the database mapper
    /// </summary>
    [Serializable]
    public class ObjectHash : IObjectHash
    {
        /// <summary>
        /// Internaly used Object Hash
        /// </summary>
        private ConstraintSaveList objectHash;

        /// <summary>
        /// Base Constructor
        /// </summary>
        public ObjectHash()
        {
            objectHash = new ConstraintSaveList();
        }

        /// <summary>
        /// Adds a persistent object to the update hash
        /// </summary>
        /// <param name="po">Persistent object</param>
        public void Add(PersistentObject po)
        {
            Add(po, null);
        }

        /// <summary>
        /// Adds the persistent object with the related value object to the object cache
        /// </summary>
        /// <param name="po">Persistent object</param>
        /// <param name="vo">Fieldvalue object</param>
        public void Add(PersistentObject po, IValueObject vo)
        {
            /*
             * Nur bei Änderung speichern
             */
            if ((po.IsDeleted || po.IsNew || po.IsModified)
                && (po.Properties.AreNotEmpty))
            {
                var entry = new HashEntry(po, vo);

                int index = objectHash.IndexOf(po.ObjectType, entry.Id);
                if (index >= 0) objectHash.RemoveAt(index);

                objectHash.Add(entry);
            }
        }

        /// <summary>
        /// Adds the object when loading
        /// </summary>
        /// <param name="po"></param>
        /// <param name="vo"></param>
        public void AddLoad(PersistentObject po, object vo)
        {
            var entry = new HashEntry(po, vo);

            int index = objectHash.IndexOf(po.ObjectType, entry.Id);
            if (index >= 0) objectHash.RemoveAt(index);

            objectHash.Add(entry);
        }

        /// <summary>
        /// Checks if the object hash contains the persistent object
        /// </summary>
        /// <param name="vo">Fieldvalue Object</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(object vo, int hierarchyLevel)
        {
            if (objectHash.Contains(vo))
            {
                HashEntry hashEntry = objectHash[ConstraintSaveList.CalculateKey(vo)];
                if (hashEntry.Po != null && // The Object must be in the objecthash

                        // if it's in hash and we want to get a flat version, we take what we get, even if it's deeploaded in it
                     ( HierarchyLevel.IsFlatLoaded(hierarchyLevel)  

                        // if it's in hash and we want to get a deep loaded version, we must ensure, that this is really a deeploaded version
                  || ( !HierarchyLevel.IsFlatLoaded(hierarchyLevel) && !hashEntry.Po.IsFlatLoaded) ))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns a persistent object from the object hash
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <returns></returns>
        public PersistentObject Get(Type type, object primaryKey)
        {
            if (objectHash.Contains(type, primaryKey))
            {
                HashEntry hashEntry = objectHash[ConstraintSaveList.CalculateKey(type, primaryKey)];
                if (hashEntry.Po != null)
                    return hashEntry.Po;
            }

            return null;
        }

        /// <summary>
        /// Returns a value object from the object hash
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <returns></returns>
        public object GetVO(Type type, object primaryKey)
        {
            if (objectHash.Contains(type, primaryKey))
            {
                HashEntry hashEntry = objectHash[ConstraintSaveList.CalculateKey(type, primaryKey)];
                if (hashEntry.Vo != null)
                    return hashEntry.Vo;
            }

            return null;
        }

        /// <summary>
        /// Returns true, if an Persistent Object is modified
        /// </summary>
        public bool IsModified
        {
            get
            {
                var enumerator = objectHash.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var po = ((HashEntry) enumerator.Current).Po;
                    if (po != null && po.IsModified) 
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// This method integrates the given mergeHash into the current object instance.
        /// </summary>
        /// <param name="mergeHash">Temporarily used load or save hash</param>
        public void MergeHash(ObjectHash mergeHash)
        {
            /*
             * Den zu mergenden Hash durchlaufen und an den eigenen anhängen
             */
            IEnumerator enumerator = mergeHash.objectHash.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current == null)
                    continue;

                /*
				 * Das Persistensobjekt ermitteln
				 */
                var entry = (HashEntry) current;
                var po = new PersistentObject(entry.Po);
                object vo = entry.Vo;

                /*
                 * Falls der Key bereits existiert, dann weglöschen
                 */
                if (objectHash.Contains(po.ObjectType, po.Id))
                    objectHash.Remove(po.ObjectType, po.Id);

                /*
                 * Wenn das Persistenzobjekt bereits gelöscht ist,
                 * dann auch nicht mehr in den Hash einhängen
                 */
                if (!po.IsDeleted)
                    objectHash.Add(new HashEntry(po, vo));
            }
        }

        /// <summary>
        /// This method stores all objects which have been modified.
        /// </summary>
        /// <param name="persister">Object persister</param>
        /// <param name="transactionContext">Object that is used for the transaction handling</param>
        public void Persist(IPersister persister, TransactionContext transactionContext)
        {
            // Make a copy and enumerate through.

            // The copy is important to make the process more stable in case an external persister
            // reloads or trys to save data
            IEnumerator enumerator = new List<HashEntry>(objectHash.InnerList).GetEnumerator();

            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current == null)
                    continue;

                bool changed = false;
                PersistentObject po = ((HashEntry) current).Po;
                object vo = ((HashEntry) current).Vo;
                var ivo = vo as IValueObject;

                ProjectionClass projection = po.Projection;

                /*
                 * Wurde das Objekt gelöscht, dann auch das PO löschen
                 */
                Dictionary<string, FieldDescription> fieldTemplates;
                if (po.IsDeleted)
                {
                    if (ivo != null) po.UpdateAutoincrementedIds(ivo);
                    fieldTemplates = projection.GetFieldTemplates(po.IsFlatLoaded); 
                    persister.Delete(Table.GetTableInstance(po.ObjectType).DefaultName, po.Id, fieldTemplates);
                    continue;
                }

                /*
                 * Wurde das Objekt verändert, dann auch das PO persistieren
                 */
                if (po.IsNew)
                {
                    if (ivo != null)
                        po.UpdateAutoincrementedIds(ivo);

                    fieldTemplates = projection.GetFieldTemplates(po.IsFlatLoaded); 
                    po.Id = persister.Insert(Table.GetTableInstance(po.ObjectType).DefaultName, po.Properties, fieldTemplates);

                    var hIvo = ((HashEntry) enumerator.Current).Vo as IValueObject;
                    if (hIvo != null)
                        hIvo.Id = po.Id;

                    changed = true;
                }
                else if (po.IsModified)
                {
                    if (ivo != null)
                        po.UpdateAutoincrementedIds(ivo);

                    fieldTemplates = projection.GetFieldTemplates(po.IsFlatLoaded); 
                    persister.Update(Table.GetTableInstance(po.ObjectType).DefaultName, po.Properties, fieldTemplates);
                    changed = true;
                }

                /*
				 * Das Update Datum ins VO übernehmen
				 */
                if ((changed) && (((HashEntry) enumerator.Current).Vo is IMarkedValueObject))
                {
                    var lastUpdate = (Field) po.Properties.FieldProperties.Get(DBConst.LastUpdateField);
                    ((IMarkedValueObject) ((HashEntry) enumerator.Current).Vo).LastUpdate = (DateTime) lastUpdate.Value;
                }

                /*
				 * Reset the changed flag
				 */
                if (changed)
                {
                    /*
                     * Reset changes
                     */
                    po.IsModified = false;
                    po.IsNew = false;
                    po.ClearUnusedLinks();
                }
            }
        }

        /// <summary>
        /// Cleans the flat loaded persistent objects out of the load hash
        /// </summary>
        public void CleanFlatLoaded()
        {
            var newList = new ConstraintSaveList();

            IEnumerator enumerator = objectHash.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current == null)
                    continue;

                PersistentObject po = ((HashEntry) current).Po;
                if ((po.IsFlatLoaded == false) || (po.IsModified) || (po.IsNew))
                    newList.Add((HashEntry) current);
            }

            objectHash = newList;
        }

        /// <summary>
        /// Determines whether [contains] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The id.</param>
        public bool Contains(Type type, object id, int hierarchyLevel)
        {
            bool contains = objectHash.Contains(type, id);
            if (contains)
            {
                HashEntry entry = objectHash[ConstraintSaveList.CalculateKey(type, id)];
                return entry.Po.IsFlatLoaded == HierarchyLevel.IsFlatLoaded(hierarchyLevel);
            }

            return false;
        }

        /// <summary>
        /// Gets the specified vo.
        /// </summary>
        /// <param name="vo">The vo.</param>
        public PersistentObject Get(IValueObject vo)
        {
            if (objectHash.Contains(vo))
            {
                HashEntry hashEntry = objectHash[ConstraintSaveList.CalculateKey(vo)];
                if (hashEntry.Po != null)
                    return hashEntry.Po;
            }

            return null;
        }

        /// <summary>
        /// Updates the ids.
        /// </summary>
        public void UpdateAutoincrementedIds()
        {
            foreach (HashEntry entry in objectHash.InnerList)
            {
                if (entry == null)
                    continue;

                var ivo = entry.Vo as IValueObject;
                if (ivo == null)
                    continue;

                // Get primary key 
                FieldDescription primary = entry.Po.Projection.GetPrimaryKeyDescription();
                var primaryField = (Field) entry.Po.Properties.FieldProperties.Get(primary.Name);

                // Update Id
                primaryField.Value = entry.Po.Id = ivo.Id;
                primaryField.IsModified = false;

                entry.Po.UpdateAutoincrementedIds(ivo);
            }
        }
    }
}