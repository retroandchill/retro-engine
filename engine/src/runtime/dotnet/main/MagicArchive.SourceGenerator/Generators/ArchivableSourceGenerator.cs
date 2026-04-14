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
    private readonly TemplateSource _templates = new();

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

        string? debugInfo;
        if (
            typeMetadata.GenerateType
            is GenerateType.Object
                or GenerateType.VersionTolerant
                or GenerateType.CircularReference
        )
        {
            var debugInfoArgs = new
            {
                XmlDocument = true,
                typeMetadata.IsBlittable,
                GenerateType = typeMetadata.GenerateType.ToString(),
                typeMetadata.Symbol,
                typeMetadata.Members,
            };
            debugInfo = _templates.DebugInfoTemplate(debugInfoArgs);
        }
        else
        {
            debugInfo = null;
        }

        var templateArgs = new
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            DebugInfo = debugInfo,
            ClassBody = typeSymbol.HasAttribute<ArchivableUnionFormatterAttribute>()
                ? typeMetadata.EmitUnionFormatterTemplate(_templates, context)
                : typeMetadata.Emit(_templates, context),
        };

        context.AddSource($"{typeSymbol.Name}.g.cs", _templates.CommonTemplate(templateArgs));
    }
}
