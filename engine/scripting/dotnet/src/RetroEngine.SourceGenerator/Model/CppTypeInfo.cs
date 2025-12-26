using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Binds;

namespace RetroEngine.SourceGenerator.Model;

[AttributeInfoType<CppTypeAttribute>]
public readonly record struct CppTypeInfo
{
    public string? TypeName { get; init; }
    
    public bool UseReference { get; init; }
    
    public bool IsConst { get; init; }
}