// // @file ArchivableSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Formatters;
using MagicArchive.SourceGenerator.Model;
using MagicArchive.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicArchive.SourceGenerator.Generators;

[Generator]
public class ArchivableSourceGenerator : IIncrementalGenerator
{
    private readonly HandlebarsTemplate<object?, object?> _archivableTemplate;

    public ArchivableSourceGenerator()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.Configuration.FormatterProviders.Add(new ClassTypeFormatter());
        handlebars.RegisterHelper("MemberWriter", Helpers.MemberWriter);
        handlebars.RegisterHelper("MemberReader", Helpers.MemberReader);
        handlebars.RegisterHelper("MemberRefReader", Helpers.MemberRefReader);
        handlebars.RegisterHelper("ConstructorParameters", Helpers.ConstructorParameters);
        _archivableTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("Archivable"));
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                typeof(ArchivableAttribute).FullName!,
                (syntaxNode, _) =>
                    syntaxNode
                        is ClassDeclarationSyntax
                            or StructDeclarationSyntax
                            or RecordDeclarationSyntax
                            or InterfaceDeclarationSyntax,
                (ctx, _) =>
                {
                    var type = (TypeDeclarationSyntax)ctx.TargetNode;
                    return (
                        Syntax: type,
                        Symbol: ctx.SemanticModel.GetDeclaredSymbol(type) as INamedTypeSymbol,
                        ctx.SemanticModel
                    );
                }
            )
            .Where(x => x.Symbol is not null);

        context.RegisterSourceOutput(
            provider,
            (ctx, t) =>
            {
                Execute(ctx, t.Syntax, t.Symbol!, t.SemanticModel);
            }
        );
    }

    private void Execute(
        SourceProductionContext context,
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol typeSymbol,
        SemanticModel semanticModel
    )
    {
        var referenceSymbols = new ReferenceSymbols(semanticModel.Compilation, semanticModel);
        var typeMetadata = new TypeMetadata(typeSymbol, referenceSymbols);
        if (typeMetadata.GenerateType == GenerateType.NoGenerate)
            return;

        if (!typeMetadata.Validate(syntax, context))
            return;

        context.AddSource($"{typeSymbol.Name}.g.cs", _archivableTemplate(typeMetadata));
    }
}
