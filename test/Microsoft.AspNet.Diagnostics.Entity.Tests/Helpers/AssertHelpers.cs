﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.Diagnostics.Entity.Tests.Helpers
{
    public static class AssertHelpers
    {
        public static void DisplaysScaffoldFirstMigration(Type contextType, string content)
        {
            Assert.Contains(StringsHelpers.GetResourceString("DatabaseErrorPage_NoDbOrMigrationsTitle", contextType.Name), content);
        }

        public static void NotDisplaysScaffoldFirstMigration(Type contextType, string content)
        {
            Assert.DoesNotContain(StringsHelpers.GetResourceString("DatabaseErrorPage_NoDbOrMigrationsTitle", contextType.Name), content);
        }

        public static void DisplaysApplyMigrations(Type contextType, string content)
        {
            Assert.Contains(StringsHelpers.GetResourceString("DatabaseErrorPage_PendingMigrationsTitle", contextType.Name), content);
        }

        public static void NotDisplaysApplyMigrations(Type contextType, string content)
        {
            Assert.DoesNotContain(StringsHelpers.GetResourceString("DatabaseErrorPage_PendingMigrationsTitle", contextType.Name), content);
        }

        public static void DisplaysScaffoldNextMigraion(Type contextType, string content)
        {
            Assert.Contains(StringsHelpers.GetResourceString("DatabaseErrorPage_PendingChangesTitle", contextType.Name), content);
        }

        public static void NotDisplaysScaffoldNextMigraion(Type contextType, string content)
        {
            Assert.DoesNotContain(StringsHelpers.GetResourceString("DatabaseErrorPage_PendingChangesTitle", contextType.Name), content);
        }
    }
}