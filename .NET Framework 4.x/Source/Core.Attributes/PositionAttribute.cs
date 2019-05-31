using System;
using System.Collections.Generic;
using System.Text;

namespace AdFactum.Data.Core.Attributes
{
    /// <summary>
    /// Base class for all attributes that needs a positioning within the sql clause.
    /// </summary>
    [Serializable]
    public abstract class PositionAttribute : Attribute
    {
        private readonly int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionAttribute"/> class.
        /// </summary>
        /// <param name="positionParameter">The position parameter.</param>
        protected PositionAttribute(int positionParameter)
        {
            position = positionParameter;
        }

        /// <summary>
        /// Gets the position of the attribute within the sql or the grouping.
        /// </summary>
        /// <value>The order in key group.</value>
        public int Position
        {
            get { return position; }
        }
    }
}
