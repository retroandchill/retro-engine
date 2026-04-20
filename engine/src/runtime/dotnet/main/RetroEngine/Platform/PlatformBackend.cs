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

    public void Dispose()
    {
        if (Handle == IntPtr.Zero)
            return;

        DestroyPlatform(Handle);
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
    private static partial void DestroyPlatform(IntPtr handle);
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
