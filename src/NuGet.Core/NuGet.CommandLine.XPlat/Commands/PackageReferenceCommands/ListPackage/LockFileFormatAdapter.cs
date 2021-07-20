// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.ProjectModel;

namespace NuGet.CommandLine.XPlat
{
    internal class LockFileFormatAdapter : ILockFileFormatAdapter
    {
        public LockFileFormat Create() => new LockFileFormat();
    }
}
