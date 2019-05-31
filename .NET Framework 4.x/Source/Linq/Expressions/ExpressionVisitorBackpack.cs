using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq.Expressions
{
    /// <summary>
    /// This class is used to carry some Fields used by several visitors through the process of Rewriting the Linq Expression tree
    /// </summary>
    public class ExpressionVisitorBackpack
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="usedTypeMapper">The used TypeMapper</param>
        public ExpressionVisitorBackpack(ITypeMapper usedTypeMapper)
        {
            typeMapper = usedTypeMapper;
            projectionCache = new Cache<Type, ProjectionClass>("Linq Dynamic Cache");
        }

        /// <summary>
        /// This field is used to give the expression visitors access to the used TypeMapper
        /// </summary>
        private readonly ITypeMapper typeMapper;

        /// <summary>
        /// This field is used to map parameter expressions to already pre-evaluated expression trees
        /// </summary>
        private readonly Dictionary<ParameterExpression, MappingStruct> parameterMapping =
            new Dictionary<ParameterExpression, MappingStruct>();

        /// <summary>
        /// This field is used to access the projection cache.
        /// </summary>
        private readonly Cache<Type, ProjectionClass> projectionCache;

        /// <summary>
        /// This field is used to enable the column Exchange in order to fix the ReferredColumn in the PropertyExpression.
        /// When the column must be updated, all referredColumn Properties must be updated too.
        /// </summary>
        private readonly Dictionary<ColumnDeclaration, ColumnDeclaration> columnExchange
            = new Dictionary<ColumnDeclaration, ColumnDeclaration>();

        /// <summary>
        /// Gain access to the parameter mapping field.
        /// </summary>
        public Dictionary<ParameterExpression, MappingStruct> ParameterMapping
        {
            get { return parameterMapping; }
        }

        /// <summary>
        /// Gain access to the used TypeMapper
        /// </summary>
        public ITypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        /// <summary>
        /// Gain access to the used Projection cache
        /// </summary>
        public Cache<Type, ProjectionClass> ProjectionCache
        {
            get { return projectionCache; }
        }

        /// <summary>
        /// Gain access to the Column Exchange Dictionary which is used to update the ReferredColumn field within the PropertyExpression
        /// </summary>
        public Dictionary<ColumnDeclaration, ColumnDeclaration> ColumnExchange
        {
            get { return columnExchange; }
        }
    }
}
