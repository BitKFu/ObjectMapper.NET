using System;
using System.Collections;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Repository
{
	/// <summary>
	/// This class is used to store the meta information within the database repository.
	/// </summary>
	[Table("OMRE_ORM_ENTITIES")]
	public class EntityInfo : MarkedValueObject
	{
		private VersionInfo versionInfo;
		private string entityName;
		private string tableName;
		private string shortName;
		private Type   objectType;
		private string application;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInfo"/> class.
        /// </summary>
        public EntityInfo()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="currentShortNames">The current short names.</param>
        public EntityInfo(VersionInfo version, string tableName, ArrayList currentShortNames)
        {
            VersionInfo = version;
            EntityName = string.Empty;
            TableName = tableName;
            ObjectType = typeof(Table);

            ShortName = CreateShortName(currentShortNames);
            currentShortNames.Add(ShortName);
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="currentShortNames">The current short names.</param>
		public EntityInfo(VersionInfo version, Type objectType, ArrayList currentShortNames)
		{
			VersionInfo = version;
			EntityName = objectType.FullName;
			TableName = Table.GetTableInstance(objectType).DefaultName;
			ObjectType = objectType;

			ShortName = CreateShortName (currentShortNames);
			currentShortNames.Add(ShortName);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="currentShortNames">The current short names.</param>
		public EntityInfo(VersionInfo version, Type objectType, string columnName, ArrayList currentShortNames)
		{
			VersionInfo = version;
			EntityName = objectType.FullName;
            TableName = Table.GetTableInstance(objectType).DefaultName + "_" + columnName;
			ObjectType = objectType;
			
			ShortName = CreateShortName (currentShortNames);
			currentShortNames.Add(ShortName);
		}

		/// <summary>
		/// Returns the entity name
		/// </summary>
		/// <value>The name of the entity.</value>
		[PropertyName("ENTITY_NAME")]
		public string EntityName
		{
			get { return entityName; }
			set { entityName = value; }
		}

		/// <summary>
		/// Returns the entity name
		/// </summary>
		/// <value>The name of the table.</value>
		[PropertyName("TABLE_NAME")]
		public string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		/// <summary>
		/// Gets or sets the version info.
		/// </summary>
		/// <value>The version info.</value>
		[PropertyName("VERSION_ID")]
		public VersionInfo VersionInfo
		{
			get { return versionInfo; }
			set
			{
				versionInfo = value;
				Application = versionInfo.Application;
			}
		}

		/// <summary>
		/// Short name for the table
		/// </summary>
		[PropertyLength(6)]
		[PropertyName("SHORT_NAME")]
		public string ShortName
		{
			get { return shortName.ToUpper(); }
			set { shortName = value.ToUpper(); }
		}

		/// <summary>
		/// Gets or sets the type of the object.
		/// </summary>
		/// <value>The type of the object.</value>
		[Ignore]
		public Type ObjectType
		{
			get { return objectType; }
			set { objectType = value; }
		}

		/// <summary>
		/// Gets or sets the application.
		/// </summary>
		/// <value>The application.</value>
		public string Application
		{
			get { return application; }
			set { application = value; }
		}

		/// <summary>
		/// Searches the high character.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private static int SearchUpperCharacter (string value)
		{
			int result = -1;

			for (int counter=0; counter < value.Length; counter++)
				if (Char.IsUpper(value[counter]))
				{
					result = counter;
					break;
				}

			return result;
		}

		/// <summary>
		/// Splitts the upper string.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private static string[] SplittUpperString (string value)
		{
			ArrayList result = new ArrayList();
			string    part   = value;
			int  end;

			do 
			{
				end = SearchUpperCharacter(part.Substring(1));
			
				if (end >= 0)
				{
					if (end>=2)
						result.Add(part.Substring(0, end+1));
					part = part.Substring(end+1);
				}
				else
					result.Add(part);

			} while (end >= 0);

			return (string[]) result.ToArray(typeof(string));
		}

		/// <summary>
		/// Creates the name of the short.
		/// </summary>
		/// <param name="names">The names.</param>
		private string CreateShortName(ArrayList names)
		{
			string result = "";
			bool linkTable = TableName.IndexOf("_") > 0;

		    /*
			 * Use Tablename if the length <= 6
			 */
			if (TableName.Length <= 6)
				result = TableName.ToUpper();

			/*
			 * Use ObjectType name if the length <= 6
			 */
			if ((ObjectType.Name.Length <= 6) && (names.Contains(result) || result.Equals("")))
				result = ObjectType.Name.ToUpper();
					
			/*
			 * Try to retrieve the splitting name
			 */
			if (names.Contains(result) || result.Equals(""))
			{
			    string[] nameParts;
			    if (linkTable)
					nameParts = TableName.Split('_');
				else
					nameParts = SplittUpperString(ObjectType.Name);

				/*
				 * Try the combinations
				 */
				if (nameParts.Length>1)
				{
                    for (int outerloop = 0; outerloop < nameParts.Length - 1; outerloop++)
                    {
                        int combineWithPart = 0;

                        do
                        {
                            if (outerloop != combineWithPart)
                            {
                                string outerPart = nameParts[outerloop].Length >= 3
                                                       ? nameParts[outerloop].Substring(0, 3)
                                                       : nameParts[outerloop];

                                string combinePart = nameParts[combineWithPart].Length >= 3
                                                       ? nameParts[combineWithPart].Substring(0, 3)
                                                       : nameParts[combineWithPart];
                                result = string.Concat(outerPart,combinePart).ToUpper();
                            }

                            combineWithPart++;
                        } while (names.Contains(result) && combineWithPart < nameParts.Length);

                        if (names.Contains(result) == false)
                            break;
                    }
				}
			}

			/*
			 * Check if there are name conflicts, than try to use the first 3 and the last 3 letters
			 */
			if (names.Contains(result) || result.Equals(""))
			{
				result = string.Concat(
					TableName.Substring(0,3),
					TableName.Substring(TableName.Length-3)).ToUpper();
			}

            /*
             * If there are still conflicts enumerate from 0 to 9 with the 5 first characters
             */
		    int nameLength = 6;
		    do
		    {
		        nameLength--;
		        string formatter = new StringBuilder().Append('0', 6 - nameLength).ToString();

		        if (names.Contains(result) || result.Equals(""))
		        {
		            int counter = 0;

                    if (tableName.Length >= nameLength)
		                do
		                {
                            result = string.Concat(TableName.Substring(0, nameLength), counter.ToString(formatter)).ToUpper();
		                    counter++;
		                } while (names.Contains(result) && (counter < Math.Pow(10, 6-nameLength)));
		        }

		        /*
                 * If there are still conflicts enumerate from 0 to 9 with the 5 last characters
                 */
		        if (names.Contains(result) || result.Equals(""))
		        {
		            int counter = 0;

                    if (tableName.Length >= nameLength)
		                do
		                {
		                    result =
                                string.Concat(TableName.Substring(tableName.Length - nameLength, nameLength), counter.ToString(formatter)).ToUpper();
		                    counter++;
                        } while (names.Contains(result) && (counter < Math.Pow(10, 6 - nameLength)));
		        }
		    } while ((names.Contains(result) || result.Equals("")) && nameLength > 1);
             
            /*
             * Throw an exception if there are still classes for that a short name can't be evaluated
             */
            if (names.Contains(result) || result.Equals(""))
				throw new ArgumentException("Short name can't be evaluated.");

			return result;
		}

	}
}