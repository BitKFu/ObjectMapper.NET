using System;
using AdFactum.Data.Core.Attributes;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
    public enum DeleteCascade
    {
        No,
        Yes
    }

    /// <summary>
    /// The foreign key attribute can be used to force logical keys.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    [Serializable]
    public class ForeignKeyAttribute : KeyGroupAttribute
    {
        private readonly Type foreignKeyType;
        private string foreignKeyTable;

        private readonly string foreignKeyProperty;
        private string foreignKeyColumn;

        private DeleteCascade deleteCascade;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        internal ForeignKeyAttribute()
            :base(0,0)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="foreignKeyTableParam">The foreign key table param.</param>
        /// <param name="foreignKeyColumnParam">The foreign key column param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(string foreignKeyTableParam, string foreignKeyColumnParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            :base(0,0)
        {
            foreignKeyType = null;
            foreignKeyTable = foreignKeyTableParam;
            
            foreignKeyProperty = null;
            foreignKeyColumn = foreignKeyColumnParam;

            deleteCascade = deleteCascadeParam;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="foreignKeyTableParam">The foreign key table param.</param>
        /// <param name="foreignKeyColumnParam">The foreign key column param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(int keyGroupParameter, string foreignKeyTableParam, string foreignKeyColumnParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            : this(keyGroupParameter, 0, foreignKeyTableParam, foreignKeyColumnParam, deleteCascadeParam)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="orderInKeyGroupParameter">The order in key group parameter.</param>
        /// <param name="foreignKeyTableParam">The foreign key table param.</param>
        /// <param name="foreignKeyColumnParam">The foreign key column param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(int keyGroupParameter, int orderInKeyGroupParameter, string foreignKeyTableParam, string foreignKeyColumnParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            : base(keyGroupParameter, orderInKeyGroupParameter)
        {
            foreignKeyType = null;
            foreignKeyTable = foreignKeyTableParam;

            foreignKeyProperty = null;
            foreignKeyColumn = foreignKeyColumnParam;
            deleteCascade = deleteCascadeParam;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="foreignKeyTypeParam">The foreign key type param.</param>
        /// <param name="foreignKeyPropertyParam">The foreign key property param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(Type foreignKeyTypeParam, string foreignKeyPropertyParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            : base(0,0)
        {
            foreignKeyType = foreignKeyTypeParam;
            foreignKeyTable = string.Empty;
            foreignKeyColumn = string.Empty;

            foreignKeyProperty = foreignKeyPropertyParam;
            deleteCascade = deleteCascadeParam;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="foreignKeyTypeParam">The foreign key type param.</param>
        /// <param name="foreignKeyPropertyParam">The foreign key property param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(int keyGroupParameter, Type foreignKeyTypeParam, string foreignKeyPropertyParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            : this(keyGroupParameter, 0, foreignKeyTypeParam, foreignKeyPropertyParam, deleteCascadeParam)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class.
        /// </summary>
        /// <param name="keyGroupParameter">The key group parameter.</param>
        /// <param name="orderInKeyGroupParameter">The order in key group parameter.</param>
        /// <param name="foreignKeyTypeParam">The foreign key type param.</param>
        /// <param name="foreignKeyPropertyParam">The foreign key property param.</param>
        /// <param name="deleteCascadeParam">true, if the related data shall be deleted too</param>
        public ForeignKeyAttribute(int keyGroupParameter, int orderInKeyGroupParameter, Type foreignKeyTypeParam, string foreignKeyPropertyParam, DeleteCascade deleteCascadeParam = DeleteCascade.No)
            : base(keyGroupParameter, orderInKeyGroupParameter)
        {
            foreignKeyType = foreignKeyTypeParam;
            foreignKeyTable = string.Empty;
            foreignKeyColumn = string.Empty;

            foreignKeyProperty = foreignKeyPropertyParam;
            deleteCascade = deleteCascadeParam;
        }


        /// <summary>
        /// Gets the local columns.
        /// </summary>
        /// <returns></returns>
        public DeleteCascade DeleteCascade => deleteCascade;

        /// <summary>
        /// Gets the local columns.
        /// </summary>
        /// <returns></returns>
        public string ForeignKeyColumn
        {
            get
            {
                if (foreignKeyColumn == string.Empty)
                    foreignKeyColumn =
                        Property.GetPropertyInstance(foreignKeyType.GetPropertyInfo(foreignKeyProperty)).MetaInfo.
                            ColumnName;

                return DBConst.DoGlobalCasing(foreignKeyColumn);
            }
        }

        /// <summary>
        /// Gets the foreign table.
        /// </summary>
        /// <returns></returns>
        public string ForeignKeyTable
        {
            get
            {
                if (foreignKeyTable == string.Empty)
                    foreignKeyTable = Table.GetTableInstance(foreignKeyType).DefaultName;

                return DBConst.DoGlobalCasing(foreignKeyTable);
            }  
        }

        /// <summary>
        /// Gets the type of the foreign.
        /// </summary>
        /// <returns></returns>
        public Type GetForeignType()
        {
            return foreignKeyType;
        }
    }
}