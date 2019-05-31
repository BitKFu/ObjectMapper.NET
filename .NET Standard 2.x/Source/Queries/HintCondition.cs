using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdFactum.Data.Queries
{
    public class HintCondition : ConditionList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HintCondition"/> class.
        /// </summary>
        /// <param name="hint">the hint condition</param>
        public HintCondition(string hint)
        {
           Hint = hint;
           this.ConditionClause = ConditionClause.HintClause;
        }

        /// <summary>
        /// Gets or sets the hint
        /// </summary>
        public string Hint { get; private set; }

        /// <summary>
        /// Gets the ConditionsString
        /// </summary>
        public override string ConditionString
        {
            get { return string.Concat("/*+ ", Hint, " */"); }
        }
    }
}
