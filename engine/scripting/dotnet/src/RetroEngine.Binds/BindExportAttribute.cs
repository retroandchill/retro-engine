namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BindExportAttribute(string? cppNamespace = null) : Attribute
{
    public string? CppNamespace { get; } = cppNamespace;
}
