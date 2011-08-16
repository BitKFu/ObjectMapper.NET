using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// This class is used to handle expression override for compiled queries.
    /// With this it is possible to exchange sqls in runtime, if it's necessary.
    /// Eventually for a hot fix deployment.
    /// </summary>
    public class ExpressionOverride
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly Cache<string, SelectReplacement> Replacements = new Cache<string, SelectReplacement>("Expression Overrides");

        /// <summary>
        /// Rewrites an expression with the given sqlId
        /// </summary>
        /// <param name="sqlId">The SQL id.</param>
        /// <param name="originalSql">The original SQL.</param>
        /// <returns></returns>
        public static string Rewrite(string sqlId, string originalSql)
        {
            SelectReplacement replacement;
            if (Replacements.TryGetValue(sqlId, out replacement))
            {
                // Perhaps the complete SQL must be exchanged
                if (!string.IsNullOrEmpty(replacement.OverrideSql))
                    return replacement.OverrideSql;

                // Perhaps only the Hint must be placed in
                if (!string.IsNullOrEmpty(replacement.OverrideHint))
                {
                    string commentedSqlId = "/* "+sqlId+ " */";
                    return originalSql.Replace(commentedSqlId, commentedSqlId + " /*+ " + replacement.OverrideHint + "*/");
                }
            }

            return originalSql;
        }
    }
}
