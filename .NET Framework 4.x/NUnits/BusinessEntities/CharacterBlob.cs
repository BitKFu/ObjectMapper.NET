using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Defines a blob as a stream
    /// </summary>
    [Table("Char_Blob")]
    public class CharacterBlob : BaseVO, ICreateObject
    {
        private string name;
        private string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        public CharacterBlob()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        public CharacterBlob(string contentData)
        {
            content = contentData;
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        [PropertyLength(int.MaxValue)]
        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new CharacterBlob();
        }
    }
}