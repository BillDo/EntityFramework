// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class IntKeysPoco
    {
        public int PartitionID { get; set; }
        public int RowID { get; set; }

        public static EntityType EntityType(Model model)
        {
            var entityType = model.AddEntityType(typeof(IntKeysPoco));
            entityType.GetOrAddProperty("PartitionID", typeof(int)).AzureTableStorage().Column = "PartitionKey";
            entityType.GetOrAddProperty("RowID", typeof(int)).AzureTableStorage().Column = "RowKey";
            return entityType;
        }
    }

    public class NullablePoco
    {
        public int? NullInt { get; set; }
        public double? NullDouble { get; set; }

        public static EntityType EntityType(Model model)
        {
            var entityType = model.AddEntityType(typeof(NullablePoco));
            entityType.GetOrAddProperty("NullInt", typeof(int?));
            entityType.GetOrAddProperty("NullDouble", typeof(double?));
            return entityType;
        }
    }

    public class ClrPoco
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class GuidKeysPoco
    {
        public Guid PartitionGuid { get; set; }
        public Guid RowGuid { get; set; }
    }

    public class ClrPocoWithProp : ClrPoco
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
    }
}
