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
    internal IntPtr NativeHandle { get; private set; }

    public PlatformBackend(PlatformBackendKind kind, PlatformInitFlags flags)
    {
        NativeHandle = NativeCreate(kind, flags, out var error);
        error.ThrowIfError();
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_create")]
    private static partial IntPtr NativeCreate(
        PlatformBackendKind kind,
        PlatformInitFlags flags,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_platform_backend_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}

[CustomMarshaller(typeof(PlatformBackend), MarshalMode.ManagedToUnmanagedIn, typeof(PlatformBackendMarshaller))]
public static class PlatformBackendMarshaller
{
    public static IntPtr ConvertToUnmanaged(PlatformBackend? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
