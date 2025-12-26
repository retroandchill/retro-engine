using System.Collections.Immutable;
using RetroEngine.SourceGenerator.Model;

namespace RetroEngine.NativeBindsGenerator.Model;

public record CppBindsMethod
{
    public string ManagedName { get; init; } = "";
    public string CppName { get; init; } = "";
    public string CppReturnType { get; init; } = "";
    public string CppParameters { get; init; } = "";
}

public record CppModuleInterface
{
    public required string ModuleName { get; init; }

    public required string CppNamespace { get; init; }

    public required string ManagedName { get; init; }

    public required string FragmentName { get; init; }

    public required string CppName { get; init; }

    public required ImmutableArray<CppImport> Imports { get; init; }

    public required ImmutableArray<CppBindsMethod> Methods { get; init; }
};
