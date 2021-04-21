using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This is a entity used for translation purpose.
    /// The example shows how to implement combined unique keys. 
    /// </summary>
    public class Translation : ValueObject, ICreateObject
    {
        private string key;     // used key to access the translated value
        private string locale;  // locale, used to identify the language
        private string value;   // translated value

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        public Translation ()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translation"/> class.
        /// </summary>
        /// <param name="keyParameter">The key parameter.</param>
        /// <param name="valueParameter">The value parameter.</param>
        /// <param name="localeParameter">The locale parameter.</param>
        public Translation(string keyParameter, string valueParameter, string localeParameter)
        {
            key = keyParameter;
            value = valueParameter;
            locale = localeParameter;
        }

        /*
         * Key Group 1 is a combination of Key and Locale
         */

        [PropertyLength(5)]
        [Unique(1,2)]             // Key Group 1 has been set
        public string Locale
        {
            get { return locale; }
            set { locale = value; }
        }

        [PropertyLength(32)]
        [Unique(1,1)]             // Key Group 1 has been set
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        [PropertyLength(255)]
        [Required]
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public virtual IValueObject CreateNewObject()
        {
            return new Translation();
        }
    }
}
