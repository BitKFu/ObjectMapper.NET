using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// The persisted properties are used to store the data of the PersistentObject
    /// </summary>
    [Serializable]
    public class PersistentProperties : ICloneable
    {
        /// <summary>
        /// Simple Properties stores data of direct attached fields that are mapped to database columns
        /// </summary>
        public UltraFastDictionary<string, IModification> FieldProperties { get; set;}
        
        /// <summary> 
        /// List Properties stores the data of associations
        /// </summary>
        public UltraFastDictionary<string, Dictionary<string, IModification>> ListProperties { get; set; }

        /// <summary>
        /// Dictionary Properties are different to List Properties, because their key might not be a string value
        /// </summary>
        public UltraFastDictionary<string, Dictionary<object, IModification>> DictProperties { get; set; }


        /// <summary>
        /// Delete all unsed Links, that are marked as Deleted
        /// </summary>
        public void ClearUnusedLinks()
        {
            if (DictProperties != null)
            {
                var enumerator = DictProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var deletedKeys = enumerator.Current.Value.Where(link => link.Value.IsDeleted).Select(link => link.Key).ToList();

                    foreach (var key in deletedKeys)
                        enumerator.Current.Value.Remove(key);
                }
            }

            if (ListProperties != null)
            {
                var enumerator1 = ListProperties.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    var deletedKeys =
                        enumerator1.Current.Value.Where(link => link.Value.IsDeleted).Select(link => link.Key).ToList();

                    foreach (var key in deletedKeys)
                        enumerator1.Current.Value.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clones the persistent properties, using a deep clone algorithm
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var copy = new PersistentProperties();

            // Copy Fields
            if (FieldProperties != null)
            foreach (var property in FieldProperties)
                copy.FieldProperties = copy.FieldProperties.Add(property.Key, (IModification)property.Value.Clone());

            // Copy List Properties
            if (ListProperties != null)
            foreach (var property in ListProperties)
            {
                var innerDict = new Dictionary<string, IModification>();
                foreach (var element in property.Value) 
                    innerDict.Add(element.Key, (IModification)element.Value.Clone());

                copy.ListProperties = copy.ListProperties.Add(property.Key, innerDict);
            }

            // Copy Dict Properties
            if (DictProperties != null)
            foreach (var property in DictProperties)
            {
                var innerDict = new Dictionary<object, IModification>();
                foreach (var element in property.Value)
                    innerDict.Add(element.Key, (IModification)element.Value.Clone());

                copy.DictProperties = copy.DictProperties.Add(property.Key, innerDict);
            }

            return copy;
        }

        /// <summary>
        /// Returns true, if the Container is empty
        /// </summary>
        public bool AreEmpty
        {
            get
            {
                return FieldProperties == null && ListProperties == null && DictProperties == null;
            }
        }

        /// <summary>
        /// Returns true, if the Container is not empty
        /// </summary>
        public bool AreNotEmpty
        {
            get
            {
                return FieldProperties != null || ListProperties != null || DictProperties != null;
            }
        }

        /// <summary>
        /// Is New For alle elements
        /// </summary>
        public bool IsNew
        {
            set
            {
                if (FieldProperties != null)
                {
                    var enumerator = FieldProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                        enumerator.Current.Value.IsNew = value;
                }

                if (ListProperties != null)
                {
                    var enumerator = ListProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                        while (innerEnumerator.MoveNext())
                            innerEnumerator.Current.Value.IsNew = value;
                    }
                }

                if (DictProperties != null)
                {
                    var enumerator = DictProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                        while (innerEnumerator.MoveNext())
                            innerEnumerator.Current.Value.IsNew = value;
                    }
                }
            }
        }

        /// <summary>
        /// Is New For alle elements
        /// </summary>
        public bool IsModified
        {

            set
            {
                if (FieldProperties != null)
                {
                    var enumerator = FieldProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                        enumerator.Current.Value.IsModified = value;
                }

                if (ListProperties != null)
                {
                    var enumerator = ListProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                        while (innerEnumerator.MoveNext())
                            innerEnumerator.Current.Value.IsModified = value;
                    }
                }

                if (DictProperties != null)
                {
                    var enumerator = DictProperties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                        while (innerEnumerator.MoveNext())
                            innerEnumerator.Current.Value.IsModified = value;
                    }
                }
            }
        }

        /// <summary>
        /// Return true, if the fields are modified or new
        /// </summary>
        /// <returns></returns>
        public bool IsModifiedOrNew()
        {
            if (FieldProperties != null)
            {
                var enumerator = FieldProperties.GetEnumerator();
                while (enumerator.MoveNext())
                    if (enumerator.Current.Value.IsModified || enumerator.Current.Value.IsNew)
                        return true;
            }

            if (ListProperties != null)
            {
                var enumerator = ListProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                    while (innerEnumerator.MoveNext())
                        if (innerEnumerator.Current.Value.IsModified || innerEnumerator.Current.Value.IsNew)
                            return true;
                }
            }

            if (DictProperties != null)
            {
                var enumerator = DictProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var innerEnumerator = enumerator.Current.Value.GetEnumerator();
                    while (innerEnumerator.MoveNext())
                        if (innerEnumerator.Current.Value.IsModified || innerEnumerator.Current.Value.IsNew)
                            return true;
                }
            }

            return false;
        }
    }
}
