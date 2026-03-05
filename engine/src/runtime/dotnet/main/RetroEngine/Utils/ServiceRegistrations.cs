// // @file ServiceRegistrations.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace RetroEngine.Utils;

internal static class ServiceRegistrations
{
    [RegisterServices]
    public static void AddFileSystem(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
    }
}
