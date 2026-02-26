// // @file WindowBackend.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Platform;

public enum WindowBackend : byte
{
    SDL3,
}

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
