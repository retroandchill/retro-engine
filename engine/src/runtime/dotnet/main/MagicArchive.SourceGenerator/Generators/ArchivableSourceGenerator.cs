// // @file ArchivableSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
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
                    return ctx.SemanticModel.GetDeclaredSymbol(type) as INamedTypeSymbol;
                }
            )
            .Where(x => x is not null);

        context.RegisterSourceOutput(provider, Execute!);
    }

    private void Execute(SourceProductionContext context, INamedTypeSymbol typeSymbol)
    {
        var (type, _) = typeSymbol.GetArchivableInfo();
        if (type == GenerateType.NoGenerate)
            return;

        var templateParams = new
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            ClassType = GetClassType(typeSymbol),
            typeSymbol.Name,
            NullableName = typeSymbol.IsValueType ? typeSymbol.Name : $"{typeSymbol.Name}?",
            IsStruct = typeSymbol.IsValueType,
            IsCustom = type == GenerateType.Custom,
        };
        context.AddSource($"{typeSymbol.Name}.g.cs", _archivableTemplate(templateParams));
    }

    private static string GetClassType(INamedTypeSymbol symbol)
    {
        return symbol switch
        {
            { IsRecord: true, IsValueType: true } => "record struct ",
            { IsRecord: true, IsValueType: false } => "record ",
            { IsRecord: false, IsValueType: true } => "struct ",
            { IsRecord: false, IsValueType: false } => "class ",
            _ => throw new InvalidOperationException("Unexpected type"),
        };
    }
}
