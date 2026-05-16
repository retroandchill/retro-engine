// @file ScriptTypeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using RetroEngine.Scripting.Compiler;

namespace RetroEngine.Scripting.Model;

public abstract partial class ScriptTypeDefinition : INamedScriptType
{
    [GeneratedRegex(@"^[a-zA-Z_]\w*(?:\.[a-zA-Z_]\w*)*$")]
    private static partial Regex ValidNamespaceRegex { get; }

    [GeneratedRegex(@"^[a-zA-Z_]\w*$")]
    private static partial Regex ValidTypeNameRegex { get; }

    public string Namespace { get; }
    public string Name { get; }
    public string FullName => $"{Namespace}.{Name}";
    public virtual string FullCodeName => FullName;
    public virtual string FullCodeNameUnbound => FullName;

    protected ScriptTypeDefinition(string @namespace, string name)
    {
        if (!ValidNamespaceRegex.IsMatch(@namespace))
            throw new ArgumentException($"Invalid namespace: {@namespace}", nameof(@namespace));

        if (!ValidTypeNameRegex.IsMatch(name))
            throw new ArgumentException($"Invalid type name: {name}", nameof(name));

        Namespace = @namespace;
        Name = name;
    }

    public virtual void Emit(CodeWriter writer)
    {
        writer.AppendLine("#nullable enable");
        writer.AppendLine();
        writer.Append("namespace ");
        writer.Append(Namespace);
        writer.AppendLine(";");
        writer.AppendLine();
    }
}
