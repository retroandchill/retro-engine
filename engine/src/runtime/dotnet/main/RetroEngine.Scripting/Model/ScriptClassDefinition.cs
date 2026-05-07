// // @file ScriptClassDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using RetroEngine.Scripting.Compiler;

namespace RetroEngine.Scripting.Model;

public enum ScriptClassVisibility
{
    Public,
    Internal,
}

public sealed partial class ScriptClassDefinition
{
    public ScriptClassVisibility Visibility { get; set; } = ScriptClassVisibility.Public;
    public string Name { get; }

    // TODO: When we start adding members we can add to this.
    public bool IsEmpty => true;

    public ScriptClassDefinition(string name)
    {
        Name = name;
        if (!ValidClassRegex.IsMatch(name))
            throw new ArgumentException("Class name is invalid.", nameof(name));
    }

    public void Emit(CodeWriter writer)
    {
        switch (Visibility)
        {
            case ScriptClassVisibility.Public:
                writer.Append("public ");
                break;
            case ScriptClassVisibility.Internal:
                writer.Append("internal ");
                break;
            default:
                throw new InvalidOperationException("Unknown visibility: " + Visibility);
        }
        writer.Append("class ");
        writer.Append(Name);
        if (IsEmpty)
        {
            writer.AppendLine(';');
            return;
        }

        using var indent = writer.EnterBlockScope();
    }

    [GeneratedRegex("^[a-zA-Z_]\\w*$")]
    private static partial Regex ValidClassRegex { get; }
}
