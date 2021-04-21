using AdFactum.Data.Util;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Northwind.Tests
{
    /// <summary>
    /// Base class for the northwind testing
    /// </summary>
    public class NorthwindBase  : ObjectMapperTest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NorthwindBase()
            :base("NW")
        {
            OBM.CurrentSqlTracer = new ConsoleTracer();
            OBM.CurrentObjectFactory = new UniversalFactory();
        }
    }
}
