using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Binds;

namespace RetroEngine.SourceGenerator.Model;

[AttributeInfoType<BlittableTypeAttribute>]
public readonly record struct BlittableTypeInfo(string? CppType)
{
    public string? CppModule { get; init; } = null;
}
