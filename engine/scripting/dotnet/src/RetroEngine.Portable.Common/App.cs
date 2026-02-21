// // @file EngineContext.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable;

public enum BuildConfiguration : byte
{
    Unknown,
    Debug,
    DebugGame,
    Development,
    Shipping,
    Test,
}

public static class App
{
    public static bool IsEditor { get; set; }

    public static BuildConfiguration BuildConfiguration { get; set; } = BuildConfiguration.Debug;

    public static bool IsGame => false;
}
