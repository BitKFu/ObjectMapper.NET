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

                var reference       = new TypeMapping
                                          {
                                              TypeBoolean = true,
                                              TypeByte = 255,
                                              TypeDateTime = DateTime.Today,
                                              TypeDecimal = new decimal(1.234),
                                              TypeDouble = 1.56778245,
                                              TypeGuid = Guid.NewGuid(),
                                              TypeInt16 = (short) rnd.Next(Int16.MaxValue),
                                              TypeInt32 = rnd.Next(Int32.MaxValue),
                                              TypeInt64 = rnd.Next(Int32.MaxValue),
                                              TypeSingle = (float) 1.56778,
                                              TypeString = "asdf",
                                              TypeTimespan = TimeSpan.FromHours(5),
                                              TypeEnum = TestEnum.Value2,
                                              TypeChar = 'A'
                                          };
                return reference;
            }            
        }

        /// <summary>
        /// Gets or sets a value indicating whether [type boolean].
        /// </summary>
        /// <value><c>true</c> if [type boolean]; otherwise, <c>false</c>.</value>
        public bool TypeBoolean { get; set; }

        /// <summary>
        /// Gets or sets the type byte.
        /// </summary>
        /// <value>The type byte.</value>
        public byte TypeByte { get; set; }

        /// <summary>
        /// Gets or sets the type date time.
        /// </summary>
        /// <value>The type date time.</value>
        public DateTime TypeDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type decimal.
        /// </summary>
        /// <value>The type decimal.</value>
        public decimal TypeDecimal { get; set; }

        /// <summary>
        /// Gets or sets the type double.
        /// </summary>
        /// <value>The type double.</value>
        public double TypeDouble { get; set; }

        /// <summary>
        /// Gets or sets the type GUID.
        /// </summary>
        /// <value>The type GUID.</value>
        public Guid TypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the type int16.
        /// </summary>
        /// <value>The type int16.</value>
        public short TypeInt16 { get; set; }

        /// <summary>
        /// Gets or sets the type int32.
        /// </summary>
        /// <value>The type int32.</value>
        public int TypeInt32 { get; set; }

        /// <summary>
        /// Gets or sets the type int64.
        /// </summary>
        /// <value>The type int64.</value>
        public long TypeInt64 { get; set; }

        /// <summary>
        /// Gets or sets the type single.
        /// </summary>
        /// <value>The type single.</value>
        public float TypeSingle { get; set; }

        /// <summary>
        /// Gets or sets the type string.
        /// </summary>
        /// <value>The type string.</value>
        public string TypeString { get; set; }

        /// <summary>
        /// Gets or sets the type timespan.
        /// </summary>
        /// <value>The type timespan.</value>
        public TimeSpan TypeTimespan { get; set; }

        /// <summary>
        /// Gets or sets the type enum.
        /// </summary>
        /// <value>The type enum.</value>
        public TestEnum TypeEnum { get; set; }

        /// <summary>
        /// Gets or sets the type char.
        /// </summary>
        /// <value>The type char.</value>
        public char TypeChar { get; set; }

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
