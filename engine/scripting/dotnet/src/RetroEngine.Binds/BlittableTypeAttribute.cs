namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
public class BlittableTypeAttribute(string? cppName = null) : Attribute
{
    public string? CppName { get; } = cppName;

    public string? CppModule { get; init; } = null;
}
