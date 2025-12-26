using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using CaseConverter;
using DotMake.CommandLine;
using HandlebarsDotNet;
using RetroEngine.NativeBindsGenerator.Model;
using RetroEngine.NativeBindsGenerator.Properties;
using RetroEngine.SourceGenerator.Model;

namespace RetroEngine.NativeBindsGenerator;

[CliCommand(Description = "Generates C++ bindings for RetroEngine")]
public class RootCommand
{
    [CliOption(Description = "The path to generate the module interfaces to", Required = true)]
    public IList<string> SourceAssemblies { get; set; } = [];

    [CliOption(Description = "The path to generate the module interfaces to", Required = true)]
    public string OutputDirectory { get; set; } = "";

    [CliOption(Description = "The name for the C++ module interface", Required = false)]
    public string ModuleName { get; set; } = "retro.scripting";

    [CliOption(Description = "The prefix for the module fragment", Required = false)]
    public string? FragmentPrefix { get; set; }

    public async Task RunAsync()
    {
        var assemblies = SourceAssemblies.Select(Assembly.LoadFrom).ToList();
        var metadataType = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t is { IsClass: true, IsAbstract: true, IsSealed: true } // static class
                && t.Namespace == "RetroEngine.Binds.Generated"
                && t.Name.EndsWith("BindMetadata", StringComparison.Ordinal)
            );

        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;

        var template = handlebars.Compile(SourceTemplates.CppInterface);

        foreach (var t in metadataType)
        {
            var jsonField = t.GetField(
                "Json",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );
            if (jsonField?.GetValue(null) is not string json)
                continue;

            var info = JsonSerializer.Deserialize<ExporterClassInfo>(json);
            if (info is null)
                continue;

            var imports = info.Imports;
            if (ModuleName != "retro.scripting")
            {
                imports =
                [
                    .. imports
                        .Add(new CppImport("retro.scripting"))
                        .Distinct()
                        .OrderBy(x => x.Name),
                ];
            }

            var cppNamespace = info.CppNamespace ?? "retro";
            var cppName = info.Name.ToSnakeCase();
            var moduleParameters = new CppModuleInterface
            {
                ModuleName = ModuleName,
                CppNamespace = cppNamespace,
                FragmentName = FragmentPrefix is not null ? $"{FragmentPrefix}.{cppName}" : cppName,
                ManagedName = info.Name,
                CppName = cppName,
                Imports = imports,
                Methods =
                [
                    .. info.Methods.Select(m => new CppBindsMethod
                    {
                        ManagedName = m.Name,
                        CppName = m.Name.ToSnakeCase(),
                        CppReturnType = m.CppReturnType.TrimStart($"{cppNamespace}::").ToString(),
                        CppParameters = string.Join(
                            ", ",
                            m.Parameters.Select(p =>
                                $"{p.CppType.TrimStart($"{cppNamespace}::").ToString()} {p.Name}"
                            )
                        ),
                    }),
                ],
            };

            var interfaceSource = template(moduleParameters);
            await File.WriteAllTextAsync(
                $"{OutputDirectory}/{moduleParameters.CppName}.ixx",
                interfaceSource
            );
        }
    }
}
