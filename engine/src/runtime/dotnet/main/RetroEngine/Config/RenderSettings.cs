// // @file RenderSettings.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Platform;

namespace RetroEngine.Config;

public sealed record RenderingSettings
{
    public bool AutoAssignViewports { get; init; } = true;

    public RenderBackendType RenderBackend { get; init; } = RenderBackendType.Vulkan;
}
