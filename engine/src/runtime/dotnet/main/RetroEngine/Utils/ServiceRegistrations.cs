// @file ServiceRegistrations.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Interop;
using RetroEngine.Rendering;
using Testably.Abstractions;

namespace RetroEngine.Utils;

internal static partial class ServiceRegistrations
{
    [RegisterServices]
    public static void AddFileSystem(this IServiceCollection services)
    {
        services
            .AddSingleton<IFileSystem, RealFileSystem>()
            .AddSingleton(_ => RenderPipeline.Create(CreateGeometryPipeline))
            .AddSingleton(_ => RenderPipeline.Create(CreateSpritePipeline))
            .AddSingleton(provider =>
            {
                var renderBackend = provider.GetRequiredService<RenderBackend>();
                return RenderPipeline.Create(renderBackend, CreateTextBlockPipeline);
            });
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_render_pipeline_create_geometry")]
    private static partial IntPtr CreateGeometryPipeline(out InteropError error);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_render_pipeline_create_sprite")]
    private static partial IntPtr CreateSpritePipeline(out InteropError error);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_render_pipeline_create_text_block")]
    private static partial IntPtr CreateTextBlockPipeline(RenderBackend renderBackend, out InteropError error);
}
