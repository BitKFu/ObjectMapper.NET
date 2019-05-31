using System;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using System.Globalization;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// This class is used for ordering select results
	/// </summary>
	[Serializable]
    public class OrderBy
	{
		private readonly Type objectType;
		private readonly string property;
		private readonly Ordering ordering;

		private readonly string manualColumn;
		private readonly string manualTable;
		private readonly string tableName;

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="objectType">Defines the object type</param>
		/// <param name="property">Name of the property which shall be ordered in a selection.</param>
		/// <param name="ordering">Defines how the result shall be ordered.</param>
		public OrderBy(Type objectType, string property, Ordering ordering)
		{
			this.objectType = objectType;
			tableName = Table.GetTableInstance(this.objectType).DefaultName;
			this.property = property;
			this.ordering = ordering;
		}

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="objectType">Defines the object type</param>
		/// <param name="property">Name of the property which shall be ordered in a selection.</param>
		/// <param name="ordering">Defines how the result shall be ordered.</param>
		public OrderBy(Type objectType, string property, string ordering)
		{
			this.objectType = objectType;
            tableName = Table.GetTableInstance(this.objectType).DefaultName;
			this.property = property;
			this.ordering = (Ordering) Enum.Parse(typeof (Ordering), ordering, true);
		}

		/// <summary>
		/// Constructor is used for manual ordering
		/// </summary>
		/// <param name="table">Defines the tablename</param>
		/// <param name="column">Column name which shall be ordered</param>
		/// <param name="ordering">Defines how the result shall be ordered.</param>
		public OrderBy(string table, string column, Ordering ordering)
		{
			tableName = manualTable = table;
			manualColumn = column;
			this.ordering = ordering;
		}

        /// <summary>
        /// Columnses this instance.
        /// </summary>
        /// <returns></returns>
        public virtual string Columns
        {
            get
            {
                return GetColumn(true);
            }
        }

        /// <summary>
        /// Gets the columns only.
        /// </summary>
        /// <value>The columns only.</value>
	    public virtual string ColumnsOnly
	    {
            get 
            { 
                return GetColumn(false)
                    .Replace(Condition.QUOTE_CLOSE, "")
                    .Replace(Condition.QUOTE_OPEN, "");
            }
	    }

        /// <summary>
        /// Returns the name of the column which shall be ordered
        /// </summary>
        /// <param name="withTable">if set to <c>true</c> [with table].</param>
        /// <returns></returns>
		public virtual string GetColumn(bool withTable)
		{
			var builder = new StringBuilder();

			// if we have manual columns -> massage them to be in form <tablename>.<columnname>
			if (manualTable != null && manualColumn != null)
			{
				string[] colNames = manualColumn.Split(new[] {','});
				foreach(string colName in colNames)
				{
					if(colName.IndexOf(".") > 0)
					{
                        builder.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}", (builder.Length != 0 ? "," : ""), string.Concat(Condition.QUOTE_OPEN , colName , Condition.QUOTE_CLOSE));
					} 
					else
					{
                        builder.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}.{2}", (builder.Length != 0 ? "," : ""), string.Concat(Condition.QUOTE_OPEN, TableName, Condition.QUOTE_CLOSE), string.Concat(Condition.QUOTE_OPEN, colName, Condition.QUOTE_CLOSE));
					}
				}
			}
			else 
			{
				// if we use props -> create order by string in the form <tablename>.<colname>
				string[] propNames = property.Split(new[] {','});
				foreach(string propName in propNames)
				{
                    builder.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}", (builder.Length != 0 ? "," : ""), ConstructColumnName(propName, withTable));
				}
			}
			return builder.ToString();
		}

		/// <summary>
		/// Returns the ordering of the column
		/// </summary>
		public string Ordering
		{
			get { return ordering.ToString(); }
		}

		/// <summary>
		/// Returns the property
		/// </summary>
		public string Property
		{
			get { return property; }
		}

		/// <summary>
		/// Returns the type of the object which has to be ordered
		/// </summary>
		public Type ObjectType
		{
			get { return objectType; }
		}

		/// <summary>
		/// Gets the name of the table.
		/// </summary>
		/// <value>The name of the table.</value>
		public string TableName
		{
			get { return tableName; }
		}


        /// <summary>
        /// Constructs the name of the column.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="withTable">if set to <c>true</c> [with table].</param>
        /// <returns></returns>
		private string ConstructColumnName(string propertyName, bool withTable)
		{
			string result;
			Property propertyInstance = Internal.Property.GetPropertyInstance(objectType.GetPropertyInfo(propertyName));
            if (propertyInstance.MetaInfo.IsVirtualLink || withTable == false)
                result = string.Concat(Condition.QUOTE_OPEN , propertyInstance.MetaInfo.ColumnName , Condition.QUOTE_CLOSE);
			else
                result = string.Concat(Condition.QUOTE_OPEN , TableName , Condition.QUOTE_CLOSE , "." , Condition.QUOTE_OPEN , propertyInstance.MetaInfo.ColumnName , Condition.QUOTE_CLOSE);
			return result;
		}


	}
}