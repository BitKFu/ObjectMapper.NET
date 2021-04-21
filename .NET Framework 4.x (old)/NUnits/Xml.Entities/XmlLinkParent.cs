using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.Xml.Entities
{
    /// <summary>
    /// Xml Parent class for linking test
    /// </summary>
    public class XmlLinkParent : ValueObject
    {
        private string name = "parent";
        
        private XmlLinkChild child1 = new XmlLinkChild();
        private XmlLinkChild child2 = new XmlLinkChild();

        private List<XmlLinkChild> childList = new List<XmlLinkChild>();
        private Dictionary<string, XmlLinkChild> childDictionary = new Dictionary<string, XmlLinkChild>();

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
        /// Gets or sets the child.
        /// </summary>
        /// <value>The child.</value>
        [GeneralLink]
        public XmlLinkChild Child1
        {
            get { return child1; }
            set { child1 = value; }
        }

        /// <summary>
        /// Gets or sets the child2.
        /// </summary>
        /// <value>The child2.</value>
        public XmlLinkChild Child2
        {
            get { return child2; }
            set { child2 = value; }
        }

        /// <summary>
        /// Gets or sets the child list.
        /// </summary>
        /// <value>The child list.</value>
        public List<XmlLinkChild> ChildList
        {
            get { return childList; }
            set { childList = value; }
        }

        public Dictionary<string, XmlLinkChild> ChildDictionary
        {
            get { return childDictionary; }
            set { childDictionary = value; }
        }
    }
}
