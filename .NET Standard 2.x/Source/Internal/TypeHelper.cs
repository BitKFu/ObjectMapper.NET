using System;
using System.Diagnostics;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
	/// <summary>
	/// This is a helper class for checking types
	/// </summary>
	public static class TypeHelper
	{
		/// <summary>
		/// Checks if the string is not null or empty
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		public static bool IsNotNullOrEmpty(this string test)
		{
			return !string.IsNullOrEmpty(test);
		}

		/// <summary>
		/// Returns a condition string for the condition operator
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string Operator(this ConditionOperator type)
		{
			string result = string.Empty;

			switch (type)
			{
				case ConditionOperator.AND:
					result = " AND ";
					break;

				case ConditionOperator.OR:
					result = " OR ";
					break;

				case ConditionOperator.ANDNOT:
					result = " AND NOT ";
					break;

				case ConditionOperator.ORNOT:
					result = " OR NOT ";
					break;

				default:
					Debug.Assert(false, "Unkown Condition Type " + type);
					break;
			}

			return result;
		}

		/// <summary>
		/// String Replace method
		/// </summary>
		/// <param name="text"></param>
		/// <param name="replaceThis"></param>
		/// <param name="replaceWith"></param>
		public static string ReplaceFirst(this string text, string replaceThis, string replaceWith)
		{
			var index = text.IndexOf(replaceThis);

			if (index >= 0)
			{
				text = text.Remove(index, replaceThis.Length);
				text = text.Insert(index, replaceWith);
			}
		    
            return text;
		}

		/// <summary>
		/// String Replace method
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startAtIndex"></param>
		/// <param name="replaceThis"></param>
		/// <param name="replaceWith"></param>
		public static string ReplaceFirst(this string text, int startAtIndex, string replaceThis, string replaceWith)
		{
            var index = text.IndexOf(replaceThis, startAtIndex);

			if (index >= 0)
			{
				text = text.Remove(index, replaceThis.Length);
				text = text.Insert(index, replaceWith);
			}
		    
            return text;
		}

        /// <summary>
        /// Gets the type of the base.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetBaseType(Type type)
        {
            /*
             * Get mapping type for value objects
             */
            if (type.IsValueObjectType())
            {
                try
                {
                    var projection = ReflectionHelper.GetProjection(type, null);
                    var desc = projection.GetPrimaryKeyDescription();
                    type = desc != null ? desc.ContentType : typeof (Guid);
                }
                catch (NoPrimaryKeyFoundException)
                {
                    type = typeof(Guid); // Use Guid as an default    
                }
            }

            /*
             * Check the nullables
             */
            if (type.IsNullableType())
                type = Nullable.GetUnderlyingType(type);

            return type;
        }

        /// <summary>
        /// Determines whether [is nullable type] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if [is nullable type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
	    public static bool IsNullableType(this Type type)
	    {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

    }
}