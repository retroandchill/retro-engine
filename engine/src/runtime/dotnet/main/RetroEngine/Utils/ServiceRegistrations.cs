// // @file ServiceRegistrations.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Interop;
using RetroEngine.Rendering;

namespace RetroEngine.Utils;

internal static partial class ServiceRegistrations
{
    [RegisterServices]
    public static void AddFileSystem(this IServiceCollection services)
    {
        services
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton(_ => RenderPipeline.Create(CreateGeometryPipeline))
            .AddSingleton(_ => RenderPipeline.Create(CreateSpritePipeline));
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_pipeline_create_geometry")]
    private static partial IntPtr CreateGeometryPipeline(out InteropError error);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_pipeline_create_sprite")]
    private static partial IntPtr CreateSpritePipeline(out InteropError error);
}
