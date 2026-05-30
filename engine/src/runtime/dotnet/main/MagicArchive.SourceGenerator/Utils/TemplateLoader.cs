// @file TemplateUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace MagicArchive.SourceGenerator.Utils;

public static class TemplateLoader
{
    private const string TemplateNamespace = "MagicArchive.SourceGenerator.Templates";
    private static readonly ConcurrentDictionary<string, string> Templates = new();

    public static string LoadTemplate(string name)
    {
        var resourceName = $"{TemplateNamespace}.{name}.mustache";
        return Templates.GetOrAdd(
            name,
            key =>
            {
                var asm = Assembly.GetExecutingAssembly();
                using var stream =
                    asm.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Missing resource: {resourceName}");

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        );
    }
}
