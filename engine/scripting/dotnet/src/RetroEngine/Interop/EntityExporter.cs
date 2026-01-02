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
