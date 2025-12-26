using System.Collections.Immutable;

namespace RetroEngine.SourceGenerator.Model;

public readonly record struct CppImport(string Name);

public record ExporterClassInfo
{
    public required string ManagedNamespace { get; init; }
    public required string? CppNamespace { get; init; }
    public required string Name { get; init; }
    public required ImmutableArray<CppImport> Imports { get; init; }
    public required ImmutableArray<BindMethodInfo> Methods { get; init; }
}
