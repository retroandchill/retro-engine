// // @file UnionSourceGeneratorBootstrapper.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public static class UnionSourceGeneratorBootstrapper
{
    private static readonly DiagnosticDescriptor ExceptionDiagnosticDescriptor = new(
        "REP0001",
        "Exception",
        "An unexpected error occurred: {0}",
        "error",
        DiagnosticSeverity.Error,
        true
    );

    public static void Bootstrap(
        IncrementalGeneratorInitializationContext context,
        IUnionCodeGenerator unionCodeGenerator
    )
    {
        var unionsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(UnionAttribute).FullName!,
            (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
            (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
        );

        context.RegisterSourceOutput(
            unionsProvider,
            (ctx, typeSymbol) =>
            {
                try
                {
                    var unionInfo = UnionInfoCollector.Collect(typeSymbol!);
                    if (unionInfo.Cases.Length == 0)
                    {
                        return;
                    }

                    var code = unionCodeGenerator.GenerateCode(unionInfo, typeSymbol);
                    if (string.IsNullOrEmpty(code))
                    {
                        return;
                    }

                    var typeFileName = typeSymbol.ToDisplayString().Replace('<', '(').Replace('>', ')');
                    ctx.AddSource($"{typeFileName}.RetroEngine.{unionCodeGenerator.Name}.g.cs", code!);
                }
                catch (Exception e)
                {
                    ReportException(ctx, e);
                }
            }
        );
    }

    private static void ReportException(SourceProductionContext context, Exception e, string? message = null)
    {
        var exceptionString = e.ToString().Replace("\r", string.Empty).Replace('\n', ' ');

        context.ReportDiagnostic(Diagnostic.Create(ExceptionDiagnosticDescriptor, null, message ?? exceptionString));
    }
}
