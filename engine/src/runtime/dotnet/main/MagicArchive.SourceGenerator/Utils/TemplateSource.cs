// // @file TemplateSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Formatters;

namespace MagicArchive.SourceGenerator.Utils;

public sealed class TemplateSource
{
    public HandlebarsTemplate<object?, object?> CommonTemplate { get; }
    public HandlebarsTemplate<object?, object?> DebugInfoTemplate { get; }
    public HandlebarsTemplate<object?, object?> ArchivableTemplate { get; }
    public HandlebarsTemplate<object?, object?> UnionTemplate { get; }
    public HandlebarsTemplate<object?, object?> UnionFormatterTemplate { get; }

    public TemplateSource()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.Configuration.FormatterProviders.Add(new ClassTypeFormatter());
        handlebars.RegisterHelper("Escaped", Helpers.Escaped);
        handlebars.RegisterHelper("Joined", Helpers.Joined);
        handlebars.RegisterHelper("Indexed", Helpers.Indexed);
        handlebars.RegisterHelper("Add", Helpers.Add);
        handlebars.RegisterHelper("SerializeMembers", Helpers.SerializeMembers);
        handlebars.RegisterHelper("DeserializeMembers", Helpers.DeserializeMembers);
        handlebars.RegisterHelper("MemberRefReader", Helpers.MemberRefReader);
        handlebars.RegisterHelper("ConstructorParameters", Helpers.ConstructorParameters);
        handlebars.RegisterHelper("MethodBody", Helpers.MethodBody);
        CommonTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("Common"));
        DebugInfoTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("DebugInfo"));
        ArchivableTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("Archivable"));
        UnionTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("Union"));
        UnionFormatterTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("UnionFormatter"));
    }
}
