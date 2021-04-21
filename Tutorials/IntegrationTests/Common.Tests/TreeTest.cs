using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class TreeTest : ObjectMapperTest
    {
        private const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED_RIGHT_NOW";

        /// <summary>
        /// Try to load the parent via Linq 
        /// </summary>
        [Test, Category(NOT_IMPLEMENTED)]
        public void LinqTreeLoadParent()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                // Create a parent-child relation
                bool nested = OBM.BeginTransaction(mapper);
                TreeNodeGuid parent = new TreeNodeGuid(Guid.NewGuid().ToString());
                TreeNodeGuid child = new TreeNodeGuid(Guid.NewGuid().ToString()) {ParentNode = parent};
                parent.ChildNodes.Add(child);
                mapper.Save(parent);
                OBM.Commit(mapper, nested);

                // Query the parent through the child
                var tree = mapper.Query<TreeNodeGuid>();
                TreeNodeGuid loadedParent = (from node in tree where node == child
                                            from node2 in tree where node2 == node.ParentNode
                                            select node2).SingleOrDefault();

                // Assert Parent Id
                Assert.IsNotNull(parent);
                Assert.AreEqual(parent.Id, loadedParent.Id);
            }
        }

        /// <summary>
        /// Simples the tree store GUID.
        /// </summary>
        [Test]
        public void SimpleTreeStoreGuid()
        {
            Type type = typeof(TreeNodeGuid);
            ITreeNode root = StoreSimpleTree(type);
            StoreUpdatedTree(root, type);
        }

        /// <summary>
        /// Simples the tree store auto inc.
        /// </summary>
        [Test]
        public void SimpleTreeStoreAutoInc()
        {
            Type type = typeof (TreeNodeAutoInc);
            ITreeNode root = StoreSimpleTree(type);
            StoreUpdatedTree(root, type);
        }

        /// <summary>
        /// Simples the tree store GUID.
        /// </summary>
        [Test]
        public void MultiLevelTreeStoreGuid()
        {
            Type type = typeof(TreeNodeGuid);
            ITreeNode root = StoreMultiLevelTree(type);

            DeleteTreeRecursive(root);
        }

        /// <summary>
        /// Resets the parent nodes.
        /// </summary>
        /// <param name="root">The root.</param>
        private static void ResetParentNodes (ITreeNode root)
        {
            root.ParentNode = null;
            foreach (ITreeNode child in root.ChildNodes)
                ResetParentNodes(child);
        }

        /// <summary>
        /// Deletes the tree recursive.
        /// </summary>
        /// <param name="root">The root.</param>
        private void DeleteTreeRecursive(ITreeNode root)
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                ResetParentNodes(root);

                bool nested = OBM.BeginTransaction(mapper);
                
                mapper.Save(root);
                OBM.Flush(mapper);

                mapper.DeleteRecursive(root, int.MaxValue);
                OBM.Commit(mapper, nested);
            }
        }

        /// <summary>
        /// Simples the tree store auto inc.
        /// </summary>
        [Test]
        public void MultiLevelStoreAutoInc()
        {
            Type type = typeof(TreeNodeAutoInc);
            ITreeNode root = StoreMultiLevelTree(type);

            DeleteTreeRecursive(root);
        }

        /// <summary>
        /// Stores the updated tree.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="type">The type.</param>
        private void StoreUpdatedTree (ITreeNode root, Type type)
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Modify tree
                 */
                root.ChildNodes.RemoveAt(2);

                ITreeNode replacement = (ITreeNode) Activator.CreateInstance(type);
                replacement.Name = "Replacement";
                replacement.ParentNode = root;

                root.ChildNodes.Insert(2, replacement);

                /*
                 * Store Tree
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(root);
                OBM.Commit(mapper, nested);

                /*
                 * Load Tree
                 */
                ITreeNode loaded = (ITreeNode) mapper.Load(type, root.Id);
                Assert.AreEqual(root.ChildNodes.Count, loaded.ChildNodes.Count, "Child count is mismatch");
            }
        }

            /// <summary>
        /// Stores the directory as tree.
        /// </summary>
        private ITreeNode StoreMultiLevelTree(Type type)
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create Tree in Memory
                 */
                ITreeNode root = CreateDirectoryStructure(type, Environment.SystemDirectory, 2);

                /*
                 * Store Tree
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(root);
                OBM.Commit(mapper, nested);

                return root;
            }
        }

        /// <summary>
        /// Stores the directory as tree.
        /// </summary>
        private ITreeNode StoreSimpleTree(Type type)
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                /*
                 * Create Tree in Memory
                 */
                ITreeNode root = CreateDirectoryStructure(type, @"C:\", 1);

                /*
                 * Store Tree
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(root);
                OBM.Commit(mapper, nested);

                /*
                 * Try to load the childs of parent, using the GetNestedCollection Method
                 */
                List<ITreeNode> childNodes01 = new List<ITreeNode>(new ListAdapter<ITreeNode>(
                    mapper.GetNestedCollection(type,"ChildNodes", root.Id,HierarchyLevel.FlatObject)));

                Assert.AreEqual(root.ChildNodes.Count, childNodes01.Count, "Load childs with GetNestedCollection failed");
                /*
                 * Load the childs, using the parent property
                 */
                List<ITreeNode> childNodes02 = new List<ITreeNode>(new ListAdapter<ITreeNode>(
                                    mapper.FlatSelect(type, new AndCondition(type, "ParentNode", root.Id))));

                Assert.AreEqual(root.ChildNodes.Count, childNodes02.Count, "Load childs by Parent Property failed");

                return root;
            }
        }

        /// <summary>
        /// Stores the directories.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private static ITreeNode CreateDirectoryStructure(Type type, string directory, int level)
        {
            ITreeNode node = (ITreeNode) Activator.CreateInstance(type);
            node.Name = directory;
            Console.WriteLine(node.Name);

            try
            {
                if (level > 0)
                    foreach (string dir in Directory.GetDirectories(directory))
                    {
                        ITreeNode child = CreateDirectoryStructure(type, dir, level - 1);
                        child.ParentNode = node;
                        node.ChildNodes.Add(child);
                    }
            }
            catch (UnauthorizedAccessException)
            {}

            return node;
        }
        

    }
}
