// // @file PlatformBackendInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

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

public enum RenderBackend : byte
{
    Vulkan,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PlatformBackendInfo(PlatformBackendKind Kind, PlatformInitFlags Flags);
