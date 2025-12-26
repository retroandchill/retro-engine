using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Strings.Interop;

[BindExport("retro")]
internal static unsafe partial class NameExporter
{
    public static partial Name Lookup([CppType(IsConst = true)] char* name, int length, FindName findType);

    public static partial NativeBool IsValid(Name name);

    public static partial NativeBool Equals(Name lhs, [CppType(IsConst = true)] char* rhs, int length);

    public static partial int ToString(Name name, char* buffer, int bufferSize);
}
