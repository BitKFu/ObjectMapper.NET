﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Util;

namespace AdFactum.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILinqPersister
    {
        /// <summary>
        /// Returns the type of the used linq expression writer
        /// </summary>
        Type LinqExpressionWriter { get; }

        /// <summary>
        /// Rewrites the expression
        /// </summary>
        Expression RewriteExpression(Expression expression, out ExpressionVisitorBackpack backpack, out List<PropertyTupel> groupings, out int level);

        /// <summary>
        /// Gets the type mapper.
        /// </summary>
        /// <value>The type mapper.</value>
        ITypeMapper TypeMapper { get; }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateCommand();

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="numberOfParameter">The number of parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">Type of the value object</param>
        /// <param name="metaInfo">property meta information</param>
        /// <returns></returns>
        IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, PropertyMetaInfo metaInfo);

        /// <summary>
        /// Creates a named the parameter.
        /// </summary>
        IDbDataParameter CreateParameter(string parameterName, Type type, object value, PropertyMetaInfo metaInfo);

        /// <summary>
        /// Creates a named parameter from an existing parameter as a copy
        /// </summary>
        IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value);

        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        string GetParameterString(IDbDataParameter parameter);

        /// <summary> Returns the database schema </summary>
        string DatabaseSchema { get; }


    }
}
