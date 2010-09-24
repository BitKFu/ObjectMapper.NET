using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// That's a abstract class for all microsoft based persisters
    /// </summary>
    public abstract class MicrosoftBasedPersister : BasePersister
    {

        /// <summary>
        /// Retrieves the last auto increment Id
        /// </summary>
        /// <returns></returns>
        protected override int SelectLastAutoId(string tableName)
        {
            int autoId = -1;
            IDbCommand command = CreateCommand();
            command.CommandText = "SELECT @@IDENTITY";

            IDataReader reader = ExecuteReader(command);
            if (reader.Read())
            {
                object lastId = reader.GetValue(0);
                if (lastId != DBNull.Value)
                    autoId = (int)ConvertSourceToTargetType(reader.GetValue(0), typeof(Int32));
            }
            reader.Close();
            command.Dispose();

            return autoId;
        }

        /// <summary>
        /// Rewrites the Linq Expression
        /// </summary>
        /// <returns></returns>
        public override Expression RewriteExpression(Expression expression, out ExpressionVisitorBackpack backpack, out List<PropertyTupel> groupings, out int level)
        {
            backpack = new ExpressionVisitorBackpack(TypeMapper);
            var boundExp = PartialEvaluator.Eval(expression);

            boundExp = QueryBinder.Evaluate(boundExp, out groupings, backpack, out level);
            boundExp = MemberBinder.Evaluate(boundExp, backpack);

            //// move aggregate computations so they occur in same select as group-by
            //!!! NOT NEEDED ANYMORE !!! boundExp = AggregateRewriter.Rewrite(boundExp, dynamicCache);

            //// Bind Relationships ( means to solve access to class members, that means to insert a join if necessary)
            //!!! NOT NEEDED ANYMORE !!! boundExp = RelationshipBinder.Bind(boundExp, dynamicCache);

            //// These bundle of Rewriters are all used to get paging mechism in place
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache);
            boundExp = SkipToRowNumberRewriter.Rewrite(boundExp, backpack);

            //// At last, the correct alias can be set.
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            //// Now Check every OrderBy, and move them up into the sql stack, if necessary
            boundExp = SqlOrderByRewriter.Rewrite(boundExp, backpack);

            //// Now have a deep look to the Cross Apply Joins. Because perhaps they aren't valid anymore.
            //// This can be, due removal of selects and replacement with the native table expressions. A INNER JOIN / or CROSS JOIN
            //// is the result of that.
            boundExp = CrossApplyRewriter.Rewrite(boundExp, backpack);

            //// Attempt to rewrite cross joins as inner joins
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = CrossJoinRewriter.Rewrite(boundExp, backpack);

            ///// Remove unused columns
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! NOT NEEDED ANYMORE !!! boundExp = UnusedColumnRemover.Rewrite(boundExp, dynamicCache);

            //// Do Final
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache );
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = RedundantJoinRemover.Remove(boundExp, backpack);
            boundExp = SqlIdRewriter.Rewrite(boundExp, backpack);
            boundExp = AliasReWriter.Rewrite(boundExp, backpack);

            boundExp = UpdateProjection.Rebind(boundExp, backpack);

            return boundExp;
        }

    }
}