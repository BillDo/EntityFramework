﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class QueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo MaterializationQueryMethod
        {
            get { return _executeMaterializationQueryExpressionMethodInfo; }
        }

        public virtual MethodInfo ProjectionQueryMethod
        {
            get { return _executeProjectionQueryExpressionMethodInfo; }
        }

        private static readonly MethodInfo
            _executeMaterializationQueryExpressionMethodInfo =
                typeof(QueryMethodProvider).GetTypeInfo()
                    .GetDeclaredMethod("ExecuteMaterializationQueryExpression");

        [UsedImplicitly]
        private static IEnumerable<TEntity> ExecuteMaterializationQueryExpression<TEntity>(
            QueryContext queryContext, RedisQuery redisQuery)
            where TEntity : class, new()
        {
            var redisQueryContext = (RedisQueryContext)queryContext;

            return redisQueryContext
                .GetResultsEnumerable(redisQuery)
                .Select(objectArray
                    => (TEntity)redisQueryContext.QueryBuffer
                        .GetEntity(redisQuery.EntityType, new ObjectArrayValueReader(objectArray)));
        }

        private static readonly MethodInfo _executeProjectionQueryExpressionMethodInfo =
            typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("ExecuteProjectionQueryExpression");

        [UsedImplicitly]
        private static IEnumerable<IValueReader> ExecuteProjectionQueryExpression(
            QueryContext queryContext, RedisQuery redisQuery)
        {
            var redisQueryContext = (RedisQueryContext)queryContext;

            return redisQueryContext
                .GetResultsEnumerable(redisQuery)
                .Select(objectArray => new ObjectArrayValueReader(objectArray));
        }
    }
}
