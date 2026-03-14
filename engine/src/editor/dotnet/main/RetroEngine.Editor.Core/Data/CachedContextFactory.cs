// // @file CachedContextFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RetroEngine.Editor.Core.Data;

public sealed class CachedDbContextFactory : IDesignTimeDbContextFactory<CachedDbContext>
{
    public CachedDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CachedDbContext>();
        CachedDbContext.ApplyDbSettings(options);
        return new CachedDbContext(options.Options);
    }
}
