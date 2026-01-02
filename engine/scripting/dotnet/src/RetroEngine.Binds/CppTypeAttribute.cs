namespace RetroEngine.Binds;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class CppTypeAttribute : Attribute
{
    public string? TypeName { get; init; }

    public string? CppModule { get; init; }

    public bool UseReference { get; init; }

    public bool IsConst { get; init; }
}
