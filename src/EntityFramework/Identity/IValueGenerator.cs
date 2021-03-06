// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public interface IValueGenerator
    {
        GeneratedValue Next(
            [NotNull] IProperty property,
            [NotNull] LazyRef<DataStoreServices> dataStoreServices);

        Task<GeneratedValue> NextAsync(
            [NotNull] IProperty property,
            [NotNull] LazyRef<DataStoreServices> dataStoreServices,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
