using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Test class for enum persistence
    /// </summary>
    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    /// <summary>
    /// This entity combines all types in one type mapping test class
    /// </summary>
    public class TypeMapping : AutoIncValueObject, ICreateObject
    {
        private bool        typeBoolean;
        private byte        typeByte;
        private DateTime    typeDateTime;
        private decimal     typeDecimal;
        private double      typeDouble;
        private Guid        typeGuid;
        private short       typeInt16;
        private int         typeInt32;
        private long        typeInt64;
        private float       typeSingle;
        private string      typeString;
        private TimeSpan    typeTimespan;
        private TestEnum    typeEnum;
        private char        typeChar;

        /// <summary>
        /// Gets the reference class.
        /// </summary>
        /// <value>The reference class.</value>
        [Ignore]
        public static TypeMapping ReferenceClass
        {
            get
            {
                Random rnd = new Random();

                TypeMapping reference       = new TypeMapping();
                reference.TypeBoolean       = true;
                reference.TypeByte          = 255;
                reference.TypeDateTime      = DateTime.Today;
                reference.TypeDecimal       = new decimal(1.234);
                reference.TypeDouble        = 1.567782;
                reference.TypeGuid          = Guid.NewGuid();
                reference.TypeInt16         = (short) rnd.Next(Int16.MaxValue);
                reference.TypeInt32         = rnd.Next(Int32.MaxValue);
                reference.TypeInt64         = rnd.Next(Int32.MaxValue);
                reference.TypeSingle        = (float)1.567782;
                reference.TypeString        = "asdf";
                reference.TypeTimespan      = TimeSpan.FromHours(5);
                reference.TypeEnum          = TestEnum.Value2;
                reference.TypeChar          = 'A';
                return reference;
            }            
        }

        /// <summary>
        /// Gets or sets a value indicating whether [type boolean].
        /// </summary>
        /// <value><c>true</c> if [type boolean]; otherwise, <c>false</c>.</value>
        public bool TypeBoolean
        {
            get { return typeBoolean; }
            set { typeBoolean = value; }
        }

        /// <summary>
        /// Gets or sets the type byte.
        /// </summary>
        /// <value>The type byte.</value>
        public byte TypeByte
        {
            get { return typeByte; }
            set { typeByte = value; }
        }

        /// <summary>
        /// Gets or sets the type date time.
        /// </summary>
        /// <value>The type date time.</value>
        public DateTime TypeDateTime
        {
            get { return typeDateTime; }
            set { typeDateTime = value; }
        }

        /// <summary>
        /// Gets or sets the type decimal.
        /// </summary>
        /// <value>The type decimal.</value>
        public decimal TypeDecimal
        {
            get { return typeDecimal; }
            set { typeDecimal = value; }
        }

        /// <summary>
        /// Gets or sets the type double.
        /// </summary>
        /// <value>The type double.</value>
        public double TypeDouble
        {
            get { return typeDouble; }
            set { typeDouble = value; }
        }

        /// <summary>
        /// Gets or sets the type GUID.
        /// </summary>
        /// <value>The type GUID.</value>
        public Guid TypeGuid
        {
            get { return typeGuid; }
            set { typeGuid = value; }
        }

        /// <summary>
        /// Gets or sets the type int16.
        /// </summary>
        /// <value>The type int16.</value>
        public short TypeInt16
        {
            get { return typeInt16; }
            set { typeInt16 = value; }
        }

        /// <summary>
        /// Gets or sets the type int32.
        /// </summary>
        /// <value>The type int32.</value>
        public int TypeInt32
        {
            get { return typeInt32; }
            set { typeInt32 = value; }
        }

        /// <summary>
        /// Gets or sets the type int64.
        /// </summary>
        /// <value>The type int64.</value>
        public long TypeInt64
        {
            get { return typeInt64; }
            set { typeInt64 = value; }
        }

        /// <summary>
        /// Gets or sets the type single.
        /// </summary>
        /// <value>The type single.</value>
        public float TypeSingle
        {
            get { return typeSingle; }
            set { typeSingle = value; }
        }

        /// <summary>
        /// Gets or sets the type string.
        /// </summary>
        /// <value>The type string.</value>
        public string TypeString
        {
            get { return typeString; }
            set { typeString = value; }
        }

        /// <summary>
        /// Gets or sets the type timespan.
        /// </summary>
        /// <value>The type timespan.</value>
        public TimeSpan TypeTimespan
        {
            get { return typeTimespan; }
            set { typeTimespan = value; }
        }

        /// <summary>
        /// Gets or sets the type enum.
        /// </summary>
        /// <value>The type enum.</value>
        public TestEnum TypeEnum
        {
            get { return typeEnum; }
            set { typeEnum = value; }
        }

        /// <summary>
        /// Gets or sets the type char.
        /// </summary>
        /// <value>The type char.</value>
        public char TypeChar
        {
            get { return typeChar; }
            set { typeChar = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new TypeMapping();
        }
    }
}
