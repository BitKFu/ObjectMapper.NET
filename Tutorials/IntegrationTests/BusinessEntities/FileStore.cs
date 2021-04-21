using System;
using AdFactum.Data;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Store file information
    /// </summary>
    public class FileStore : BaseVO
    {
        private string fileName;
        private int fileSize;
        private DateTime creationDate;
        private DateTime modificationDate;

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [Unique]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public int FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>The creation date.</value>
        public DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        /// <value>The modification date.</value>
        public DateTime ModificationDate
        {
            get { return modificationDate; }
            set { modificationDate = value; }
        }
    }
}
