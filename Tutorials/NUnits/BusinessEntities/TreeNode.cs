using System.Collections.Generic;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("Nodes")]
    public class TreeNodeGuid : MarkedValueObject, ICreateObject, ITreeNode
    {
        private string name;
        private List<ITreeNode> childNodes = new List<ITreeNode>();
        private ITreeNode parentNode;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeGuid"/> class.
        /// </summary>
        public TreeNodeGuid ()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeGuid"/> class.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        public TreeNodeGuid (string nodeName)
        {
            name = nodeName;    
        }
        
        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        [PropertyLength(250)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>The child nodes.</value>
        [BindPropertyTo(typeof(TreeNodeGuid))]
        public List<ITreeNode> ChildNodes
        {
            get { return childNodes; }
            set { childNodes = value; }
        }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>The parent node.</value>
        [BindPropertyTo(typeof(TreeNodeGuid))]
        public ITreeNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new TreeNodeGuid();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Table("NodesAutoInc")]
    public class TreeNodeAutoInc : AutoIncValueObject, ICreateObject, ITreeNode
    {
        private string name;
        private List<ITreeNode> childNodes = new List<ITreeNode>();
        private ITreeNode parentNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeAutoInc"/> class.
        /// </summary>
        public TreeNodeAutoInc()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeAutoInc"/> class.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        public TreeNodeAutoInc(string nodeName)
        {
            name = nodeName;
        }

        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        [PropertyLength(250)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>The child nodes.</value>
        [BindPropertyTo(typeof(TreeNodeAutoInc))]
        public List<ITreeNode> ChildNodes
        {
            get { return childNodes; }
            set { childNodes = value; }
        }


        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>The parent node.</value>
        [BindPropertyTo(typeof(TreeNodeAutoInc))]
        public ITreeNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new TreeNodeAutoInc();
        }
    }

}
