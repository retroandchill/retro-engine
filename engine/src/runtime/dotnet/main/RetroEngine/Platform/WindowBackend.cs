// // @file WindowBackend.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Platform;

public enum WindowBackend : byte
{
    Headless,
    SDL3,
}

public readonly record struct PlatformWindowHandle(IntPtr Handle, WindowBackend Backend);

[Flags]
public enum WindowFlags : ulong
{
    None = 0,
    Resizable = 1 << 0,
    Borderless = 1 << 1,
    Hidden = 1 << 2,
    Vulkan = 1 << 3,
    HighDPI = 1 << 4,
    AlwaysOnTop = 1 << 5,
}

public enum NativeWindowType : byte
{
    Win32Hwnd,
    X11,
    WaylandSurface,
    CocoaWindow,
    CocoaView,
    Unknown = byte.MaxValue,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct NativeWindowHandle(NativeWindowType Type, IntPtr Handle);
