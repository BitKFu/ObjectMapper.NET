using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This class offers translations for a region within a country.
    /// Therefore we must ensure that the country exists in database.
    /// </summary>
    [Table("REGIONS")]
    public class CountryRegion : Translation
    {
        private string countryKey;

        /// <summary>
        /// Gets or sets the country key.
        /// </summary>
        /// <value>The country key.</value>
        [PropertyLength(32)]
        [ForeignKey(1, typeof(Country), "Key")]
        public string CountryKey
        {
            get { return countryKey; }
            set { countryKey = value; }
        }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        /// <value>The locale.</value>
        [PropertyLength(5)]
        [Unique(1, 2)]             // Key Group 1 has been set
        [ForeignKey(1, typeof(Country), "Locale")]
        public new string Locale
        {
            get { return base.Locale; }
            set { base.Locale = value; }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Country"/> class.
        /// </summary>
        public CountryRegion ()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Country"/> class.
        /// </summary>
        /// <param name="countryKeyParam">The country key param.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="locale">The locale.</param>
        public CountryRegion(string countryKeyParam, string key, string value, string locale)
            : base(key, value, locale)
        {
            countryKey = countryKeyParam;    
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public override IValueObject CreateNewObject()
        {
            return new CountryRegion();
        }
    }
}
