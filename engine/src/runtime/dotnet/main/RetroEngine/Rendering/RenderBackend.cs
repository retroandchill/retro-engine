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
        return UploadTextureAsync(data).GetAwaiter().GetResult();
    }

    internal Task<Texture> UploadTextureAsync(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var tsc = new TaskCompletionSource<Texture>();
        cancellationToken.Register(() => tsc.TrySetCanceled());

        var gcHandle = GCHandle.Alloc(tsc);
        tsc.Task.ContinueWith(_ => gcHandle.Free(), TaskContinuationOptions.ExecuteSynchronously);
        unsafe
        {
            NativeUploadTexture(
                this,
                data,
                data.Length,
                &OnTextureCreated,
                &OnTextureUploadFailed,
                GCHandle.ToIntPtr(gcHandle)
            );
        }
        return tsc.Task;
    }

    [UnmanagedCallersOnly]
    private static void OnTextureCreated(
        IntPtr userData,
        IntPtr nativeHandle,
        int width,
        int height,
        TextureFormat format
    )
    {
        var tsc = (TaskCompletionSource<Texture>)GCHandle.FromIntPtr(userData).Target!;
        tsc.TrySetResult(new Texture(nativeHandle, width, height, format));
    }

    [UnmanagedCallersOnly]
    private static void OnTextureUploadFailed(IntPtr userData, NativeInteropError nativeError)
    {
        var tsc = (TaskCompletionSource<Texture>)GCHandle.FromIntPtr(userData).Target!;
        tsc.TrySetException(
            InteropErrorMarshaller.ConvertToManaged(nativeError).ToException()
                ?? new InvalidOperationException("Unknown error")
        );
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroRenderer, EntryPoint = "retro_render_backend_create")]
    private static partial IntPtr NativeCreate(
        PlatformBackend platformBackend,
        RenderBackendType renderBackendType,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroRenderer, EntryPoint = "retro_render_backend_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_render_backend_upload_texture")]
    private static unsafe partial void NativeUploadTexture(
        RenderBackend backend,
        ReadOnlySpan<byte> bytes,
        int length,
        delegate* unmanaged<IntPtr, IntPtr, int, int, TextureFormat, void> onCreated,
        delegate* unmanaged<IntPtr, NativeInteropError, void> onError,
        IntPtr userData
    );
}

[CustomMarshaller(typeof(RenderBackend), MarshalMode.ManagedToUnmanagedIn, typeof(RenderBackendMarshaller))]
public static class RenderBackendMarshaller
{
    public static IntPtr ConvertToUnmanaged(RenderBackend? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
