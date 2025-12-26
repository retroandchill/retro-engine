using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Strings.Interop;

[BindExport("NameExporter")]
internal static unsafe partial class NameExporter
{
    public static partial Name Lookup(char* name, int length, FindName findType);
    public static partial NativeBool IsValid(Name name);
    public static partial NativeBool Equals(Name lhs, char* rhs, int length);
    public static partial int ToString(Name name, char* buffer, int bufferSize);
}