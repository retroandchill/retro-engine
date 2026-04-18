// // @file ArchivableSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Model;
using MagicArchive.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Generators;

[Generator]
public class ArchivableSourceGenerator : IIncrementalGenerator
{
    private readonly TemplateSource _templates = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var archivableTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(ArchivableAttribute).FullName!,
            (syntaxNode, _) =>
                syntaxNode
                    is ClassDeclarationSyntax
                        or StructDeclarationSyntax
                        or RecordDeclarationSyntax
                        or InterfaceDeclarationSyntax,
            (ctx, _) => (TypeDeclarationSyntax)ctx.TargetNode
        );

        var unionFormatters = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(ArchivableUnionFormatterAttribute).FullName!,
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => (TypeDeclarationSyntax)ctx.TargetNode
        );

        context.RegisterSourceOutput(
            archivableTypes.Combine(context.CompilationProvider),
            (ctx, t) =>
            {
                var (typeDeclaration, compilation) = t;
                Execute(ctx, typeDeclaration, compilation);
            }
        );
        context.RegisterSourceOutput(
            unionFormatters.Combine(context.CompilationProvider),
            (ctx, t) =>
            {
                var (typeDeclaration, compilation) = t;
                Execute(ctx, typeDeclaration, compilation);
            }
        );
    }

    private void Execute(SourceProductionContext context, TypeDeclarationSyntax syntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

        if (
            ModelExtensions.GetDeclaredSymbol(semanticModel, syntax, context.CancellationToken)
            is not INamedTypeSymbol typeSymbol
        )
            return;
        try
        {
            var generateType = typeSymbol.TryGetArchivableType(out var g, out _) ? g : GenerateType.Union;
            if (!IsPartial(syntax) && generateType != GenerateType.NoGenerate)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MustBePartial,
                        syntax.Identifier.GetLocation(),
                        typeSymbol.Name
                    )
                );
                return;
            }

            if (IsNested(syntax) && !IsNestedContainingTypesPartial(syntax))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.NestedContainingTypesMustBePartial,
                        syntax.Identifier.GetLocation(),
                        typeSymbol.Name
                    )
                );
                return;
            }

            var referenceSymbols = new ReferenceSymbols(compilation, semanticModel);
            INamedTypeSymbol? unionSymbol = null;
            if (typeSymbol.TypeKind is TypeKind.Class && typeSymbol.TryGetArchivableUnionFormatterInfo(out var info))
            {
                unionSymbol = info.Type as INamedTypeSymbol;
            }

            var typeMetadata = new TypeMetadata(typeSymbol, referenceSymbols);
            if (unionSymbol is not null)
            {
                typeMetadata.Symbol = unionSymbol;
            }

            if (unionSymbol is null && typeMetadata is { IsUnion: false, GenerateType: GenerateType.NoGenerate })
                return;

            if (!typeMetadata.Validate(syntax, context, unionSymbol is not null))
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
                ClassBody = unionSymbol is not null
                    ? typeMetadata.EmitUnionFormatterTemplate(_templates, typeSymbol)
                    : typeMetadata.Emit(_templates, context),
            };

            context.AddSource($"{typeSymbol.Name}.g.cs", _templates.CommonTemplate(templateArgs));
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnknownError,
                    typeSymbol.Locations.First(),
                    typeSymbol.Name,
                    e.Message
                )
            );
        }
    }

    private static bool IsPartial(TypeDeclarationSyntax syntax)
    {
        return syntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));
    }

    private static bool IsNestedContainingTypesPartial(TypeDeclarationSyntax typeDeclaration)
    {
        var parent = typeDeclaration.Parent;
        while (parent is TypeDeclarationSyntax parentTypeSyntax)
        {
            if (!IsPartial(parentTypeSyntax))
                return false;

            parent = parentTypeSyntax.Parent;
        }

        return true;
    }

    private static bool IsNested(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Parent is TypeDeclarationSyntax;
    }
}
