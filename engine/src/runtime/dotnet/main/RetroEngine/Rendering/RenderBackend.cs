// @file RenderBackend.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Options;
using RetroEngine.Config;
using RetroEngine.Interop;
using RetroEngine.Platform;

namespace RetroEngine.Rendering;

[NativeMarshalling(typeof(RenderBackendMarshaller))]
[RegisterSingleton]
public sealed partial class RenderBackend : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    public RenderBackend(PlatformBackend platformBackend, IOptions<RenderingSettings> renderingSettings)
    {
        NativeHandle = NativeCreate(platformBackend, renderingSettings.Value.RenderBackend, out var error);
        error.ThrowIfError();
    }

    ~RenderBackend()
    {
        Dispose();
    }

    internal Texture UploadTexture(ReadOnlySpan<byte> data)
    {
        var nativeHandle = NativeUploadTexture(
            this,
            data,
            data.Length,
            out var width,
            out var height,
            out var format,
            out var error
        );
        error.ThrowIfError();
        return new Texture(nativeHandle, width, height, format);
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_backend_upload_texture")]
    private static partial IntPtr NativeUploadTexture(
        RenderBackend backend,
        ReadOnlySpan<byte> bytes,
        int length,
        out int width,
        out int height,
        out TextureFormat format,
        out InteropError error
    );
}

[CustomMarshaller(typeof(RenderBackend), MarshalMode.ManagedToUnmanagedIn, typeof(RenderBackendMarshaller))]
public static class RenderBackendMarshaller
{
    public static IntPtr ConvertToUnmanaged(RenderBackend? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
