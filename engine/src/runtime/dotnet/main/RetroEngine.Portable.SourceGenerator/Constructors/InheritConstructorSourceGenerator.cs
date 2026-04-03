// // @file InhertitConstructorSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Portable.SourceGenerator.Common;
using RetroEngine.Portable.SourceGenerator.Common.CodeAnalyzing;
using RetroEngine.Portable.SourceGenerator.Common.CodeGenerating;
using RetroEngine.Portable.Utils;
using TypeDefinition = RetroEngine.Portable.SourceGenerator.Common.CodeGenerating.TypeDefinition;
using TypeInfo = RetroEngine.Portable.SourceGenerator.Common.CodeAnalyzing.TypeInfo;
using TypeKind = RetroEngine.Portable.SourceGenerator.Common.CodeGenerating.TypeKind;
using TypeName = RetroEngine.Portable.SourceGenerator.Common.CodeAnalyzing.TypeName;

namespace RetroEngine.Portable.SourceGenerator.Constructors;

[Generator]
public class InheritConstructorSourceGenerator : IIncrementalGenerator
{
    private readonly Dictionary<ITypeSymbol, TypeInfo> _typesCache = new(SymbolEqualityComparer.Default);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(InheritConstructorsAttribute).FullName!,
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
        );

        context.RegisterSourceOutput(
            classProvider,
            (ctx, targetType) =>
            {
                var codeWriter = CodeWriter.CreateWithDefaultLines();
                codeWriter.AppendLine($"namespace {targetType.ContainingNamespace};");
                codeWriter.AppendLine();

                var typeDefinition = CreateTypeDefinition(targetType);
                TypeCodeWriter.WriteType(typeDefinition, codeWriter);
                ctx.AddSource($"{targetType.Name}.g.cs", codeWriter.ToString());
            }
        );
    }

    private TypeDefinition CreateTypeDefinition(INamedTypeSymbol typeSymbol)
    {
        return new TypeDefinition
        {
            IsPartial = true,
            Name = typeSymbol.Name,
            Kind = TypeKind.Class(false, false),
            Constructors = CreateConstructors(typeSymbol),
        };
    }

    private ImmutableArray<ConstructorDefinition> CreateConstructors(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        if (baseType is null)
            return [];

        return
        [
            .. baseType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m =>
                    m.MethodKind == MethodKind.Constructor
                    && !m.IsStatic
                    && IsAccessibleByDerivedType(m, typeSymbol)
                    && !m.HasAttribute<ObsoleteAttribute>()
                )
                .Select(GenerateConstructorDefinition),
        ];
    }

    private static bool IsAccessibleByDerivedType(IMethodSymbol methodSymbol, INamedTypeSymbol derivedType)
    {
        var owningType = methodSymbol.ContainingType;
        return methodSymbol.DeclaredAccessibility switch
        {
            Accessibility.NotApplicable or Accessibility.Private => false,
            Accessibility.ProtectedAndInternal or Accessibility.Internal => SymbolEqualityComparer.Default.Equals(
                owningType.ContainingAssembly,
                derivedType.ContainingAssembly
            ),
            Accessibility.Protected or Accessibility.ProtectedOrInternal or Accessibility.Public => true,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private ConstructorDefinition GenerateConstructorDefinition(IMethodSymbol constructorSymbol)
    {
        var targetAccessibility = constructorSymbol.DeclaredAccessibility switch
        {
            Accessibility.ProtectedAndInternal or Accessibility.Internal => Accessibility.Internal,
            Accessibility.Protected or Accessibility.ProtectedOrInternal or Accessibility.Public =>
                Accessibility.Public,
            _ => throw new ArgumentOutOfRangeException(),
        };
        return new ConstructorDefinition
        {
            Accessibility = targetAccessibility,
            Parameters = [.. constructorSymbol.Parameters.Select(GenerateMethodParameter)],
            OtherConstructorCall = new OtherConstructorCall(
                OtherConstructorKind.Base,
                [.. constructorSymbol.Parameters.Select(x => x.Name)]
            ),
        };
    }

    private MethodParameter GenerateMethodParameter(IParameterSymbol parameterSymbol)
    {
        var parameterType = GetOrCreateTypeDefinition(parameterSymbol.Type);
        var isNullable =
            parameterSymbol.Type is { IsValueType: false, NullableAnnotation: NullableAnnotation.Annotated };
        var typeName = new TypeName(parameterType, isNullable);
        MethodParameterModifier? parameterModifier = parameterSymbol.RefKind switch
        {
            RefKind.None => null,
            RefKind.Ref => MethodParameterModifier.Ref,
            RefKind.Out => MethodParameterModifier.Out,
            RefKind.In or RefKind.RefReadOnlyParameter => MethodParameterModifier.In,
            _ => throw new ArgumentOutOfRangeException(),
        };
        return new MethodParameter(typeName, parameterSymbol.Name, parameterModifier);
    }

    private TypeInfo GetOrCreateTypeDefinition(ITypeSymbol typeSymbol)
    {
        if (_typesCache.TryGetValue(typeSymbol, out var definition))
        {
            return definition;
        }

        definition = TypeInfoCollector.CreateTypeInfo(typeSymbol);
        _typesCache[typeSymbol] = definition;
        return definition;
    }
}
