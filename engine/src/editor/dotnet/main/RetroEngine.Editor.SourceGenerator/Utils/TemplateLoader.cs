// // @file TemplateUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Editor.SourceGenerator.Utils;

using System.Reflection;

public static class TemplateLoader
{
    private const string TemplateNamespace = "RetroEngine.Editor.SourceGenerator.Templates";
    private static readonly Dictionary<string, string> Templates = new();

    public static string LoadTemplate(string name)
    {
        var resourceName = $"{TemplateNamespace}.{name}.mustache";
        if (Templates.TryGetValue(resourceName, out var template))
            return template;

        var asm = Assembly.GetExecutingAssembly();
        using var stream =
            asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing resource: {resourceName}");

        using var reader = new StreamReader(stream);
        var templateText = reader.ReadToEnd();
        Templates.Add(resourceName, templateText);
        return templateText;
    }
}
