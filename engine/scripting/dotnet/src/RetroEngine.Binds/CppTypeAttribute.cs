namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class CppTypeAttribute : Attribute
{
    public string? TypeName { get; init; }

    public bool UseReference { get; init; } = false;

    public bool IsConst { get; init; } = false;
}
