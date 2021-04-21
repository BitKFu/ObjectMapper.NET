using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Xml.Entities
{
    /// <summary>
    /// Xml child for linking test
    /// </summary>
    public class XmlLinkChild : ValueObject
    {
        private string name="Child";

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
