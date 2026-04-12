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
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Generators;

[Generator]
public class ArchivableSourceGenerator : IIncrementalGenerator
{
    private readonly HandlebarsTemplate<object?, object?> _archivableTemplate;
    private readonly HandlebarsTemplate<object?, object?> _unionTemplate;
    private readonly HandlebarsTemplate<object?, object?> _unionFormatterTemplate;

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
        _unionTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("Union"));
        _unionFormatterTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("UnionFormatter"));
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var archivableTypes = context
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

        var unionFormatters = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                typeof(ArchivableUnionFormatterAttribute).FullName!,
                (node, _) => node is ClassDeclarationSyntax,
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
            archivableTypes,
            (ctx, t) =>
            {
                Execute(ctx, t.Syntax, t.Symbol!, t.SemanticModel);
            }
        );
        context.RegisterSourceOutput(
            unionFormatters,
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

        if (typeSymbol.HasAttribute<ArchivableUnionFormatterAttribute>())
        {
            context.AddSource($"{typeSymbol.Name}.g.cs", _unionFormatterTemplate(typeMetadata));
        }
        else
        {
            switch (typeMetadata.GenerateType)
            {
                case GenerateType.Object:
                case GenerateType.VersionTolerant:
                case GenerateType.CircularReference:
                case GenerateType.Custom:
                    context.AddSource($"{typeSymbol.Name}.g.cs", _archivableTemplate(typeMetadata));
                    break;
                case GenerateType.Collection:
                    break;
                case GenerateType.NoGenerate:
                    break;
                case GenerateType.Union:
                    context.AddSource($"{typeSymbol.Name}.g.cs", _unionTemplate(typeMetadata));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
