using System;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Tree Name
    /// </summary>
    public interface ITreeNode : IValueObject
    {
        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>The child nodes.</value>
        List<ITreeNode> ChildNodes { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>The parent node.</value>
        ITreeNode ParentNode { get; set; }
    }
}
