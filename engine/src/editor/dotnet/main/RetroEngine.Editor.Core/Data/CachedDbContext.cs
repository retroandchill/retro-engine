// // @file CachedDbContext.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Data.Entities;

namespace RetroEngine.Editor.Core.Data;

public sealed class CachedDbContext : DbContext
{
    public DbSet<RecentProject> RecentProjects { get; init; }

    public CachedDbContext() { }

    public CachedDbContext(DbContextOptions<CachedDbContext> options)
        : base(options) { }

    [RegisterServices]
    internal static void RegisterCachedDbContext(IServiceCollection services)
    {
        services.AddDbContext<CachedDbContext>(options =>
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataPath, "RetroEngine", "Editor", "cache.db");
            options.UseSqlite($"Data Source={dbPath};").UseSnakeCaseNamingConvention();
        });
    }
}
