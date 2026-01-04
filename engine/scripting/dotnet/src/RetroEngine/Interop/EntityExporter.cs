// @file $EntityExporter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using RetroEngine.Binds;
using RetroEngine.Core.Math;
using RetroEngine.Scene;

namespace RetroEngine.Interop;

[BindExport("retro")]
internal static partial class EntityExporter
{
    public static partial ref readonly Transform GetEntityTransform(EntityId entityId);

    public static partial void SetEntityTransform(EntityId entityId, in Transform transform);

    public static partial EntityId CreateNewEntity(in Transform transform);

    public static partial void RemoveEntityFromScene(EntityId entityId);
}
