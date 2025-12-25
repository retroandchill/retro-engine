using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Strings.Interop;

internal static unsafe class NameExporter
{
    private static readonly delegate *unmanaged<char*, int, FindName, Name> LookupDelegate;
    private static readonly delegate *unmanaged<Name, NativeBool> IsValidDelegate;
    private static readonly delegate *unmanaged<Name, char*, int, NativeBool> EqualsDelegate;
    private static readonly delegate *unmanaged<Name, char*, int, int> ToStringDelegate;

    static NameExporter()
    {
        var lookupDelegateSize = sizeof(Name) + sizeof(char*) + sizeof(int) + sizeof(FindName);
        LookupDelegate = (delegate *unmanaged<char*, int, FindName, Name>) BindsManager.GetBoundFunction(nameof(NameExporter), nameof(Lookup), lookupDelegateSize);
        
        var isValidDelegateSize = sizeof(NativeBool) + sizeof(Name);
        IsValidDelegate = (delegate *unmanaged<Name, NativeBool>) BindsManager.GetBoundFunction(nameof(NameExporter), nameof(IsValid), isValidDelegateSize);
        
        var equalsDelegateSize = sizeof(Name) + sizeof(char*) + sizeof(int) + sizeof(NativeBool);
        EqualsDelegate = (delegate *unmanaged<Name, char*, int, NativeBool>) BindsManager.GetBoundFunction(nameof(NameExporter), nameof(Equals), equalsDelegateSize);
        
        var toStringDelegateSize = sizeof(Name) + sizeof(char*) + sizeof(int) + sizeof(int);
        ToStringDelegate = (delegate *unmanaged<Name, char*, int, int>) BindsManager.GetBoundFunction(nameof(NameExporter), nameof(ToString), toStringDelegateSize);
    }

    public static Name Lookup(char* name, int length, FindName findType) => LookupDelegate(name, length, findType);
    public static NativeBool IsValid(Name name) => IsValidDelegate(name);
    public static NativeBool Equals(Name lhs, char* rhs, int length) => EqualsDelegate(lhs, rhs, length);
    public static int ToString(Name name, char* buffer, int bufferSize) => ToStringDelegate(name, buffer, bufferSize);
}