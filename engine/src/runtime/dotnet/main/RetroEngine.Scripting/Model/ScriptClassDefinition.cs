// // @file ScriptClassDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using RetroEngine.Scripting.Compiler;

namespace RetroEngine.Scripting.Model;

public sealed class ScriptClassDefinition(string @namespace, string name) : ScriptTypeDefinition(@namespace, name)
{
    public INamedScriptType? BaseType { get; set; }
    public List<INamedScriptType> Interfaces { get; } = [];

    public IEnumerable<INamedScriptType> AllBaseTypes
    {
        get
        {
            if (BaseType is not null)
                yield return BaseType;

            foreach (var @interface in Interfaces)
            {
                yield return @interface;
            }
        }
    }

    // TODO: When we start adding members we can add to this.
    public bool IsEmpty => true;

    public override void Emit(CodeWriter writer)
    {
        base.Emit(writer);
        writer.Append("public class ");
        writer.Append(Name);

        if (BaseType is not null || Interfaces.Count > 0)
        {
            writer.Append(" : ");

            foreach (var (i, type) in AllBaseTypes.Index())
            {
                if (i > 0)
                    writer.Append(", ");

                writer.Append(type.FullCodeName);
            }
        }

        if (IsEmpty)
        {
            writer.AppendLine(';');
            return;
        }

        writer.AppendLine();
        using var indent = writer.EnterBlockScope();
    }
}
