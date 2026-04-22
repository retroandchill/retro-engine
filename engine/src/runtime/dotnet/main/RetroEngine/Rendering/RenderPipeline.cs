// // @file RenderPipeline.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Interop;

namespace RetroEngine.Rendering;

public delegate IntPtr RenderPipelineFactory(out InteropError error);

public sealed partial class RenderPipeline : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    private RenderPipeline(IntPtr nativeHandle)
    {
        NativeHandle = nativeHandle;
    }

    public static RenderPipeline Create(RenderPipelineFactory factory)
    {
        var nativeHandle = factory(out var error);
        error.ThrowIfError();
        return nativeHandle != IntPtr.Zero
            ? new RenderPipeline(nativeHandle)
            : throw new InvalidOperationException("Failed to create render pipeline");
    }

    ~RenderPipeline()
    {
        NativeDestroy(NativeHandle);
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_backend_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}
