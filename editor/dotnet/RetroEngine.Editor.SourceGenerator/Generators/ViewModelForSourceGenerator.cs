// // @file ViewModelForSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.SourceGenerator.Model;
using RetroEngine.Editor.SourceGenerator.Utils;

namespace RetroEngine.Editor.SourceGenerator.Generators;

[Generator]
public class ViewModelForSourceGenerator : IIncrementalGenerator
{
    private readonly HandlebarsTemplate<object?, object?> _viewModelTemplate;

    public ViewModelForSourceGenerator()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        _viewModelTemplate = handlebars.Compile(TemplateLoader.LoadTemplate("ViewModel"));
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var viewModels = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                typeof(ViewModelForAttribute<>).FullName!,
                (n, _) => n is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
                (ctx, _) =>
                {
                    var type = (TypeDeclarationSyntax)ctx.TargetNode;
                    return ctx.SemanticModel.GetDeclaredSymbol(type) as INamedTypeSymbol;
                }
            )
            .Where(s => s is not null);

        context.RegisterSourceOutput(viewModels, Execute!);
    }

    private void Execute(SourceProductionContext context, INamedTypeSymbol viewModelSymbol)
    {
        var info = viewModelSymbol.GetViewModelForInfo();
        var classType = viewModelSymbol switch
        {
            { IsRecord: true, IsValueType: false } => "record",
            { IsRecord: false, IsValueType: false } => "class",
            { IsRecord: false, IsValueType: true } => "struct",
            { IsRecord: true, IsValueType: true } => "record struct",
            _ => throw new InvalidOperationException("Unexpected type"),
        };

        var templateParameters = new
        {
            Namespace = viewModelSymbol.ContainingNamespace.ToDisplayString(),
            ClassType = classType,
            ClassName = viewModelSymbol.Name,
            ViewName = info.ViewType.ToDisplayString(),
        };

        context.AddSource($"{templateParameters.ClassName}.g.cs", _viewModelTemplate(templateParameters));
    }
}
