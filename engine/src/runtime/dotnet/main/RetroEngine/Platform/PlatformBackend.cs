// // @file PlatformBackend.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Platform;

[NativeMarshalling(typeof(PlatformBackendMarshaller))]
public sealed partial class PlatformBackend : IDisposable
{
    internal IntPtr Handle { get; private set; }

    public PlatformBackend(PlatformBackendKind kind, PlatformInitFlags flags)
    {
        Handle = CreatePlatform(kind, flags, out var errorMessage);
        errorMessage.ThrowIfError();
    }

    ~PlatformBackend()
    {
        Dispose();
    }

    public Window CreateWindow(ReadOnlySpan<char> title, int width, int height, WindowFlags flags = WindowFlags.None)
    {
        var nativeWindow = NativeCreateWindow(Handle, title, title.Length, width, height, flags, out var errorMessage);
        errorMessage.ThrowIfError();
        return new Window(nativeWindow);
    }

    public async ValueTask<Window> CreateWindowAsync(
        ReadOnlyMemory<char> title,
        int width,
        int height,
        WindowFlags flags = WindowFlags.None,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<IntPtr>(cancellationToken);
        var tcsHandle = GCHandle.Alloc(tcs);
        try
        {
            unsafe
            {
                NativeCreateWindowAsync(
                    Handle,
                    title.Span,
                    title.Length,
                    width,
                    height,
                    flags,
                    GCHandle.ToIntPtr(tcsHandle),
                    &OnWindowCreated,
                    &OnWindowError
                );
            }
        }
        finally
        {
            tcsHandle.Free();
        }

        return new Window(await tcs.Task);
    }

    [UnmanagedCallersOnly]
    private static void OnWindowCreated(IntPtr userData, IntPtr windowId)
    {
        var tcs = (TaskCompletionSource<IntPtr>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetResult(windowId);
    }

    [UnmanagedCallersOnly]
    private static void OnWindowError(IntPtr userData, NativeInteropError errorMessage)
    {
        var tcs = (TaskCompletionSource<IntPtr>)GCHandle.FromIntPtr(userData).Target!;
        var managedError = InteropErrorMarshaller.NativeToManaged.ConvertToManaged(errorMessage);
        var exception = managedError.ToException() ?? new InvalidOperationException("No exception occurred.");
        tcs.TrySetException(exception);
    }

    public void Dispose()
    {
        if (Handle == IntPtr.Zero)
            return;

        NativeDestroy(Handle);
        Handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_create")]
    private static partial IntPtr CreatePlatform(
        PlatformBackendKind kind,
        PlatformInitFlags flags,
        out InteropError errorMessage
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_destroy")]
    private static partial void NativeDestroy(IntPtr handle);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_create_window")]
    private static partial IntPtr NativeCreateWindow(
        IntPtr backend,
        ReadOnlySpan<char> title,
        int titleLength,
        int width,
        int height,
        WindowFlags flags,
        out InteropError errorMessage
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_create_window")]
    private static unsafe partial IntPtr NativeCreateWindowAsync(
        IntPtr backend,
        ReadOnlySpan<char> title,
        int titleLength,
        int width,
        int height,
        WindowFlags flags,
        IntPtr userData,
        delegate* unmanaged<IntPtr, IntPtr, void> onWindowCreated,
        delegate* unmanaged<IntPtr, NativeInteropError, void> onError
    );
}

[CustomMarshaller(typeof(PlatformBackend), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToNative))]
public static class PlatformBackendMarshaller
{
    public static class ManagedToNative
    {
        public static IntPtr ConvertToUnmanaged(PlatformBackend backend)
        {
            return backend.Handle;
        }
    }
}
