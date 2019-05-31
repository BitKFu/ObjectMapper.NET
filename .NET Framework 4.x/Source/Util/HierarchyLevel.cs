using System;
using System.Text;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// The hierarchy helper class offers const values that can be used 
    /// to fill the hierarchy level parameter of the ObjectMapper .NET methods.
    /// </summary>
    public class HierarchyLevel
    {
        /// <summary>
        /// Returns true, if the hierarchy level is lesser or equal 0
        /// </summary>
        /// <param name="hierarchyLevel"></param>
        /// <returns></returns>
        internal static bool IsFlatLoaded (int hierarchyLevel)
        {
            return hierarchyLevel <= 0;
        }
        
        /// <summary>
        /// Returns true, if the hierarchy level is lesser or equal -1
        /// That means that only the links of an object shall be stored, but not the properties of the object itself.
        /// </summary>
        /// <param name="hierarchyLevel"></param>
        /// <returns></returns>
        internal static bool StoreOnlyLinks(int hierarchyLevel)
        {
            return hierarchyLevel <= -1;
        }
        
        /// <summary>
        /// Decrements the current level
        /// </summary>
        /// <param name="hierarchyLevel"></param>
        /// <returns></returns>
        internal static int DecLevel (int hierarchyLevel)
        {
            return hierarchyLevel - 2;
        }

        /// <summary>
        /// Can be used to load or save a flat object
        /// </summary>
        public const int FlatObject = 0;

        /// <summary>
        /// Stores the flat object with links
        /// </summary>
        public const int FlatObjectWithLinks = 1;

        /// <summary>
        /// Stores the dependend objects 1st level 
        /// </summary>
        public const int Dependend1stLvl = 2;

        /// <summary>
        /// Stores the dependend objects in the first level including the links
        /// </summary>
        public const int Dependend1stLvlWithLinks = 3;

        /// <summary>
        /// Stores the dependend objects up to the second level
        /// </summary>
        public const int Dependend2ndLvl = 4;

        /// <summary>
        /// Stores the dependend objects up to the second level including the links
        /// </summary>
        public const int Dependend2ndLvlWithLinks = 5;
        
        /// <summary>
        /// Stores all dependend objects
        /// </summary>
        public const int AllDependencies = int.MaxValue;
    }
}

