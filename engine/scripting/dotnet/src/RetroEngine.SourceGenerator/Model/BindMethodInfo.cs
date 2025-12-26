using System.Collections.Immutable;

namespace RetroEngine.SourceGenerator.Model;

public record BindMethodParameter
{
    public string Name { get; init; } = "";
    public string ManagedType { get; init; } = "";
    public string CppType { get; init; } = "";
}

public record BindMethodInfo
{
    public required string Name { get; init; }
    public required string ManagedReturnType { get; init; }
    public required string CppReturnType { get; init; }
    public required bool ReturnsVoid { get; init; }
    public required ImmutableArray<BindMethodParameter> Parameters { get; init; }
}