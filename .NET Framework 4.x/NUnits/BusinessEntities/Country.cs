using System;
using AdFactum.Data;
using AdFactum.Data.Util;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class offers translation for country names
    /// </summary>
    [Table("COUNTRIES")]
    public class Country : Translation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Country"/> class.
        /// </summary>
        public Country ()
        {
            
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Country"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="locale">The locale.</param>
        public Country(string key, string value, string locale)
        :base(key, value, locale)
        {
            
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new Country();
        }
    }
}
