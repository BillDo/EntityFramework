// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class TransactionTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : RelationalTestStore
        where TFixture : TransactionFixtureBase<TTestStore>, new()
    {
        [Fact]
        public void SaveChanges_implicitly_starts_transaction()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                context.ChangeTracker.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }

            AssertStoreInitialState();
        }

        [Fact]
        public async Task SaveChangesAsync_implicitly_starts_transaction()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                context.ChangeTracker.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void SaveChanges_uses_explicit_transaction_without_committing()
        {
            using (var context = CreateContext())
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_without_committing()
        {
            using (var context = CreateContext())
            {
                using (await context.Database.AsRelational().Connection.BeginTransactionAsync())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void SaveChanges_uses_explicit_transaction_and_does_not_rollback_on_failure()
        {
            using (var context = CreateContext())
            {
                using (var transaction = context.Database.AsRelational().Connection.BeginTransaction())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                    }

                    Assert.NotNull(transaction.DbTransaction.Connection);
                }
            }
        }

        [Fact]
        public async Task SaveChangesAsync_uses_explicit_transaction_and_does_not_rollback_on_failure()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().Last()).State = EntityState.Added;

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                    }

                    Assert.NotNull(transaction.DbTransaction.Connection);
                }
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_commited()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
            }

            using (var context = CreateContext())
            {
                Assert.Equal(Fixture.Customers.Count - 1, context.Set<TransactionCustomer>().Count());
            }
        }

        [Fact]
        public async Task RelationalTransaction_can_be_rolled_back()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                    transaction.Rollback();

                    AssertStoreInitialState();
                }
            }
        }

        [Fact]
        public void Query_uses_explicit_transaction()
        {
            using (var context = CreateContext())
            {
                using (var transaction = context.Database.AsRelational().Connection.BeginTransaction())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    context.SaveChanges();

                    using (var innerContext = CreateContext())
                    {
                        using (innerContext.Database.AsRelational().Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            Assert.Equal(Fixture.Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                        }

                        if (SnapshotSupported)
                        {
                            using (innerContext.Database.AsRelational().Connection.BeginTransaction(IsolationLevel.Snapshot))
                            {
                                Assert.Equal(Fixture.Customers, innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToList());
                            }
                        }
                    }

                    using (var innerContext = CreateContext((DbConnection)context.Database.AsRelational().Connection.DbConnection))
                    {
                        innerContext.Database.AsRelational().Connection.UseTransaction(transaction.DbTransaction);
                        Assert.Equal(Fixture.Customers.Count - 1, innerContext.Set<TransactionCustomer>().Count());
                    }
                }
            }
        }

        [Fact]
        public async Task QueryAsync_uses_explicit_transaction()
        {
            using (var context = CreateContext())
            {
                using (var transaction = await context.Database.AsRelational().Connection.BeginTransactionAsync())
                {
                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();

                    using (var innerContext = CreateContext())
                    {
                        using (await innerContext.Database.AsRelational().Connection.BeginTransactionAsync(IsolationLevel.ReadUncommitted))
                        {
                            Assert.Equal(Fixture.Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                        }

                        if (SnapshotSupported)
                        {
                            using (await innerContext.Database.AsRelational().Connection.BeginTransactionAsync(IsolationLevel.Snapshot))
                            {
                                Assert.Equal(Fixture.Customers, await innerContext.Set<TransactionCustomer>().OrderBy(c => c.Id).ToListAsync());
                            }
                        }
                    }

                    using (var innerContext = CreateContext((DbConnection)context.Database.AsRelational().Connection.DbConnection))
                    {
                        innerContext.Database.AsRelational().Connection.UseTransaction(transaction.DbTransaction);
                        Assert.Equal(Fixture.Customers.Count - 1, await innerContext.Set<TransactionCustomer>().CountAsync());
                    }
                }
            }
        }

        [Fact]
        public async Task Can_use_open_connection_with_started_transaction()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext(TestDatabase.Connection))
                {
                    context.Database.AsRelational().Connection.UseTransaction(transaction);

                    context.ChangeTracker.Entry(context.Set<TransactionCustomer>().First()).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                }
            }

            AssertStoreInitialState();
        }

        [Fact]
        public void UseTransaction_throws_if_mismatched_connection()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext())
                {
                    Assert.Throws<InvalidOperationException>(() =>
                        context.Database.AsRelational().Connection.UseTransaction(transaction))
                        .ValidateMessage(typeof(RelationalConnection), "TransactionAssociatedWithDifferentConnection");
                }
            }
        }

        [Fact]
        public void UseTransaction_throws_if_another_transaction_started()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext())
                {
                    using (context.Database.AsRelational().Connection.BeginTransaction())
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                            context.Database.AsRelational().Connection.UseTransaction(transaction))
                            .ValidateMessage(typeof(RelationalConnection), "TransactionAlreadyStarted");
                    }
                }
            }
        }

        [Fact]
        public void UseTransaction_will_not_dispose_external_transaction()
        {
            using (var transaction = TestDatabase.Connection.BeginTransaction())
            {
                using (var context = CreateContext(TestDatabase.Connection))
                {
                    context.Database.AsRelational().Connection.UseTransaction(transaction);

                    context.Database.AsRelational().Connection.Dispose();

                    Assert.NotNull(transaction.Connection);
                }
            }
        }

        protected virtual void AssertStoreInitialState()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(Fixture.Customers, context.Set<TransactionCustomer>().OrderBy(c => c.Id));
            }
        }

        #region Helpers

        protected TransactionTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestDatabase = Fixture.CreateTestStore();
        }

        protected TTestStore TestDatabase { get; set; }
        protected TFixture Fixture { get; set; }

        public void Dispose()
        {
            TestDatabase.Dispose();
        }

        protected abstract bool SnapshotSupported { get; }
        
        protected DbContext CreateContext()
        {
            return Fixture.CreateContext(TestDatabase);
        }

        protected DbContext CreateContext(DbConnection connection)
        {
            return Fixture.CreateContext(connection);
        }

        #endregion
    }
}
