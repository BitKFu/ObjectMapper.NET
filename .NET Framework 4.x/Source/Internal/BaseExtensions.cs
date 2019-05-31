using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdFactum.Data.Internal
{
    public static class BaseExtensions
    {
        /// <summary>
        /// Returns the sizeof a value type
        /// </summary>
        /// <param name="value">value to retrieve the size</param>
        /// <returns>size of the value</returns>
        public static int SizeOf(this object value)
        {
            return value != null && value is string ? ((string) value).Length : 0;
        }
    }
}
