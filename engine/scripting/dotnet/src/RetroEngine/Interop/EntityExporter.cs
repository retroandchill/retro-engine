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
    public static partial int GetEntityTransformOffset();

    [return: CppType(TypeName = "retro::Entity", CppModule = "retro.runtime", UseReference = true)]
    public static partial IntPtr CreateNewEntity(in Transform transform, out EntityId id);

    public static partial void RemoveEntityFromScene(
        [CppType(TypeName = "retro::Entity", CppModule = "retro.runtime", IsConst = true, UseReference = true)]
            IntPtr entityPtr
    );
}
