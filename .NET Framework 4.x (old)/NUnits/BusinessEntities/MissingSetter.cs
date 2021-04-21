using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// This is an uncomplete value object that throws a missing setter exception
    /// </summary>
    public class MissingSetter : ValueObject
    {
        private string missing = string.Empty;

        /// <summary>
        /// Gets the missing.
        /// </summary>
        /// <value>The missing.</value>
        public string Missing
        {
            get { return missing; }
        }
    }
}
