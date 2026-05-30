// @file TemplateUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace AutoViewModel.SourceGenerator.Utils;

using System.Reflection;

public static class TemplateLoader
{
    private const string TemplateNamespace = "AutoViewModel.SourceGenerator.Templates";
    private static readonly ConcurrentDictionary<string, string> Templates = new();

    public static string LoadTemplate(string name)
    {
        var resourceName = $"{TemplateNamespace}.{name}.mustache";
        return Templates.GetOrAdd(
            resourceName,
            key =>
            {
                var asm = Assembly.GetExecutingAssembly();
                using var stream =
                    asm.GetManifestResourceStream(key)
                    ?? throw new InvalidOperationException($"Missing resource: {key}");

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        );
    }
}
