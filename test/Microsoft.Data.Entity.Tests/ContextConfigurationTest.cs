﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.InMemory;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextConfigurationTest
    {
        [Fact]
        public void Throws_if_required_services_not_configured()
        {
            RequiredServiceTest(c => c.Services.ActiveIdentityGenerators);
            RequiredServiceTest(c => c.Services.ModelSource);
            RequiredServiceTest(c => c.Services.EntityKeyFactorySource);
            RequiredServiceTest(c => c.Services.ClrPropertyGetterSource);
            RequiredServiceTest(c => c.Services.ClrPropertySetterSource);
            RequiredServiceTest(c => c.Services.StateManager);
            RequiredServiceTest(c => c.Services.ContextEntitySets);
            RequiredServiceTest(c => c.Services.StateEntryNotifier);
            RequiredServiceTest(c => c.Services.StateEntryFactory);
        }

        private void RequiredServiceTest<TService>(Func<ContextConfiguration, TService> test)
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(TService)),
                Assert.Throws<InvalidOperationException>(() => test(CreateEmptyConfiguration())).Message);
        }

        [Fact]
        public void Optional_multi_services_return_empty_list_when_not_registered()
        {
            Assert.Empty(CreateEmptyConfiguration().Services.EntityStateListeners);
        }

        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = CreateDefaultProvider();
            var configuration1 = TestHelpers.CreateContextConfiguration(provider);
            var configuration2 = TestHelpers.CreateContextConfiguration(provider);

            Assert.Same(configuration1.Services.ActiveIdentityGenerators, configuration2.Services.ActiveIdentityGenerators);
            Assert.Same(configuration1.Services.ModelSource, configuration2.Services.ModelSource);
            Assert.Same(configuration1.Services.EntityKeyFactorySource, configuration2.Services.EntityKeyFactorySource);
            Assert.Same(configuration1.Services.ClrPropertyGetterSource, configuration2.Services.ClrPropertyGetterSource);
            Assert.Same(configuration1.Services.ClrPropertySetterSource, configuration2.Services.ClrPropertySetterSource);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = CreateDefaultProvider();
            var configuration = TestHelpers.CreateContextConfiguration(provider);

            Assert.Same(configuration.Services.StateManager, configuration.Services.StateManager);
            Assert.Same(configuration.Services.ContextEntitySets, configuration.Services.ContextEntitySets);
            Assert.Same(configuration.Services.StateEntryNotifier, configuration.Services.StateEntryNotifier);
            Assert.Same(configuration.Services.StateEntryFactory, configuration.Services.StateEntryFactory);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = CreateDefaultProvider();
            var configuration1 = TestHelpers.CreateContextConfiguration(provider);
            var configuration2 = TestHelpers.CreateContextConfiguration(provider);

            Assert.NotSame(configuration1.Services.StateManager, configuration2.Services.StateManager);
            Assert.NotSame(configuration1.Services.ContextEntitySets, configuration2.Services.ContextEntitySets);
            Assert.NotSame(configuration1.Services.StateEntryNotifier, configuration2.Services.StateEntryNotifier);
            Assert.NotSame(configuration1.Services.StateEntryFactory, configuration2.Services.StateEntryFactory);
        }

        private static IServiceProvider CreateDefaultProvider()
        {
            return new ServiceCollection()
                .AddEntityFramework(s => s.AddInMemoryStore())
                .BuildServiceProvider();
        }

        private static ContextConfiguration CreateEmptyConfiguration()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            return new ContextConfiguration()
                .Initialize(
                    provider,
                    new EntityConfiguration(provider, null, new ConfigurationAnnotations(), null),
                    Mock.Of<EntityContext>());
        }
    }
}