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

    [CliOption(Description = "The name for the C++ module interface")]
    public string ModuleName { get; set; } = "retro.scripting";

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

            var moduleParameters = new CppModuleInterface
            {
                ModuleName = ModuleName,
                CppNamespace = info.CppNamespace ?? "retro",
                ManagedName = info.Name,
                CppName = info.Name.ToSnakeCase(),
                Imports = info.Imports,
                Methods =
                [
                    .. info.Methods.Select(m => new CppBindsMethod
                    {
                        ManagedName = m.Name,
                        CppName = m.Name.ToSnakeCase(),
                        CppReturnType = m.CppReturnType,
                        CppParameters = string.Join(
                            ", ",
                            m.Parameters.Select(p => $"{p.CppType} {p.Name}")
                        ),
                    }),
                ],
            };

            var interfaceSource = template(moduleParameters);
            await File.WriteAllTextAsync(
                $"{OutputDirectory}/{moduleParameters.CppName}.generated.ixx",
                interfaceSource
            );
        }
    }
}
