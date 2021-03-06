﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDataStoreTest
    {
        [Fact]
        public async Task SaveChangesAsync_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();

            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddSingleton<FakeRelationalDataStore>()
                .BuildServiceProvider();

            var relationalDataStore = serviceProvider.GetRequiredService<FakeRelationalDataStore>();

            var stateEntries = new List<StateEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(stateEntries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        [Fact]
        public void SaveChanges_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();

            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddSingleton<FakeRelationalDataStore>()
                .BuildServiceProvider();

            var relationalDataStore = serviceProvider.GetRequiredService<FakeRelationalDataStore>();

            var stateEntries = new List<StateEntry>();

            relationalDataStore.SaveChanges(stateEntries);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries));
            batchExecutorMock.Verify(be => be.Execute(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object));
        }

        private class FakeRelationalDataStore : RelationalDataStore
        {
            public FakeRelationalDataStore(
                StateManager stateManager,
                LazyRef<IModel> model,
                EntityKeyFactorySource entityKeyFactorySource,
                EntityMaterializerSource entityMaterializerSource,
                ClrCollectionAccessorSource collectionAccessorSource,
                ClrPropertySetterSource propertySetterSource,
                RelationalConnection connection,
                CommandBatchPreparer batchPreparer,
                BatchExecutor batchExecutor,
                ILoggerFactory loggerFactory)
                : base(stateManager, model, entityKeyFactorySource, entityMaterializerSource,
                    collectionAccessorSource, propertySetterSource, connection, batchPreparer, batchExecutor, loggerFactory)
            {
            }
        }
    }
}
