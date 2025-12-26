using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Binds;

namespace RetroEngine.SourceGenerator.Model;

[AttributeInfoType<BindExportAttribute>]
public readonly record struct BindExportInfo(string? CppNamespace);