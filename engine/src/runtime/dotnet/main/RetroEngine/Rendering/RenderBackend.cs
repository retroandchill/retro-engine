// // @file RenderBackend.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;
using RetroEngine.Platform;

namespace RetroEngine.Rendering;

[NativeMarshalling(typeof(RenderBackendMarshaller))]
public sealed partial class RenderBackend : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    internal RenderBackend(PlatformBackend platformBackend, RenderBackendType platformBackendType)
    {
        NativeHandle = NativeCreate(platformBackend, platformBackendType, out var error);
        error.ThrowIfError();
    }

    ~RenderBackend()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_backend_create")]
    private static partial IntPtr NativeCreate(
        PlatformBackend platformBackend,
        RenderBackendType renderBackendType,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_backend_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}

[CustomMarshaller(typeof(RenderBackend), MarshalMode.ManagedToUnmanagedIn, typeof(RenderBackendMarshaller))]
public static class RenderBackendMarshaller
{
    public static IntPtr ConvertToUnmanaged(RenderBackend? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
