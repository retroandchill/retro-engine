// // @file PlatformBackend.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Text;

namespace RetroEngine.Platform;

public enum PlatformBackendKind : byte
{
    SDL3,
}

[Flags]
public enum PlatformInitFlags : uint
{
    None = 0,
    Audio = 1 << 0,
    Video = 1 << 1,
    Joystick = 1 << 2,
    Haptic = 1 << 3,
    Gamepad = 1 << 4,
    Events = 1 << 5,
    Sensors = 1 << 6,
    Camera = 1 << 7,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PlatformBackendInfo(PlatformBackendKind Kind, PlatformInitFlags Flags);

public sealed partial class PlatformBackend : IDisposable
{
    public const string NativeLibraryName = "retro_platform";

    public PlatformBackendInfo Info { get; }
    private IntPtr _nativeHandle;
    private bool IsDisposed => _nativeHandle == IntPtr.Zero;

    public PlatformBackend(
        PlatformBackendKind kind = PlatformBackendKind.SDL3,
        PlatformInitFlags flags = PlatformInitFlags.None
    )
    {
        Info = new PlatformBackendInfo(kind, flags);

        Span<byte> errorBuffer = stackalloc byte[1024];
        _nativeHandle = NativeCreate(Info, errorBuffer, errorBuffer.Length);
        if (_nativeHandle == IntPtr.Zero)
        {
            throw new PlatformNotSupportedException(
                $"Failed to create platform backend: {Encoding.UTF8.GetString(errorBuffer)}"
            );
        }
    }

    public void Dispose()
    {
        if (_nativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(_nativeHandle);
        _nativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraryName, EntryPoint = "retro_platform_backend_create")]
    private static partial IntPtr NativeCreate(PlatformBackendInfo info, Span<byte> errorBuffer, int errorBufferSize);

    [LibraryImport(NativeLibraryName, EntryPoint = "retro_platform_backend_destroy")]
    private static partial void NativeDestroy(IntPtr handle);
}
