// // @file MetadataExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using MagicArchive.SourceGenerator.Model;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator;

internal static class MetadataExtensions
{
    public static bool TryGetArchivableType(this ITypeSymbol symbol, out GenerateType type, out SerializeLayout layout)
    {
        if (symbol is not INamedTypeSymbol namedTypeSymbol || !namedTypeSymbol.TryGetArchivableInfo(out var info))
        {
            type = GenerateType.NoGenerate;
            layout = SerializeLayout.Sequential;
            return false;
        }

        type = info.GenerateType;
        layout = info.SerializeLayout;
        return true;
    }

    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol, bool withoutOverride = true)
    {
        // Iterate Parent -> Derived
        if (symbol.BaseType is not null)
        {
            foreach (var item in symbol.BaseType.GetAllMembers())
            {
                // override item already iterated in parent type
                if (!withoutOverride || !item.IsOverride)
                {
                    yield return item;
                }
            }
        }

        foreach (var item in symbol.GetMembers().Where(item => !withoutOverride || !item.IsOverride))
        {
            yield return item;
        }
    }

    public static bool TryGetArchiveOrder(this ISymbol symbol, out int order)
    {
        switch (symbol)
        {
            case IFieldSymbol field when field.TryGetArchiveOrderInfo(out var fieldInfo):
                order = fieldInfo.Order;
                return true;
            case IPropertySymbol property when property.TryGetArchiveOrderInfo(out var propertyInfo):
                order = propertyInfo.Order;
                return true;
            default:
                order = 0;
                return false;
        }
    }

    public static bool TryGetConstructorParameter(
        this IMethodSymbol constructor,
        ISymbol member,
        [NotNullWhen(true)] out IParameterSymbol? parameter
    )
    {
        parameter = GetConstructorParameter(constructor, member.Name);
        if (parameter is null && member.Name.StartsWith("_"))
            parameter = GetConstructorParameter(constructor, member.Name.AsSpan(1));

        return parameter is not null;

        static IParameterSymbol? GetConstructorParameter(IMethodSymbol constructor, ReadOnlySpan<char> name)
        {
            foreach (var param in constructor.Parameters)
            {
                if (param.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return param;
            }

            return null;
        }
    }

    extension(INamedTypeSymbol typeSymbol)
    {
        public bool IsArchivableInterface
        {
            get
            {
                if (!typeSymbol.IsGenericType)
                    return false;

                return typeSymbol is { MetadataName: "IArchivable`1", TypeArguments.Length: 1 }
                    && typeSymbol.ContainingNamespace.Name == "MagicArchive";
            }
        }

        public bool IsNullableValueType
        {
            get
            {
                if (!typeSymbol.IsGenericType)
                    return false;

                return typeSymbol is { MetadataName: "INullable`1", TypeArguments.Length: 1, IsValueType: true }
                    && typeSymbol.ContainingNamespace.Name == "System";
            }
        }

        public bool IsListType
        {
            get
            {
                if (!typeSymbol.IsGenericType)
                    return false;

                return typeSymbol
                    is {
                        MetadataName: "List`1",
                        TypeArguments.Length: 1,
                        ContainingNamespace:
                        {
                            Name: "Generic",
                            ContainingNamespace: { Name: "Collections", ContainingNamespace.Name: "System" }
                        }
                    };
            }
        }

        public IEnumerable<ISymbol> GetParentMembers()
        {
            if (typeSymbol.BaseType is null)
                yield break;

            foreach (var member in typeSymbol.BaseType.GetMembers())
            {
                yield return member;
            }
        }

        public bool EqualsUnconstructedGenericType(INamedTypeSymbol right)
        {
            var l = typeSymbol.IsGenericType ? typeSymbol.ConstructUnboundGenericType() : typeSymbol;
            var r = right.IsGenericType ? right.ConstructUnboundGenericType() : right;
            return SymbolEqualityComparer.Default.Equals(l, r);
        }

        public IEnumerable<INamedTypeSymbol> GetAllBaseTypes()
        {
            var t = typeSymbol.BaseType;
            while (t is not null)
            {
                yield return t;
                t = t.BaseType;
            }
        }
    }

    public static string FullyQualifiedToString(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static bool WillImplementArchivableUnion(this ITypeSymbol symbol)
    {
        return symbol is INamedTypeSymbol { IsAbstract: true } typeSymbol
            && typeSymbol.HasAttribute<ArchivableUnionAttribute>();
    }

    public static bool ContainsConstructorParameter(
        this IEnumerable<MemberMetadata> members,
        IParameterSymbol constructorParameter
    ) =>
        members.Any(x =>
            x.IsConstructorParameter
            && string.Equals(constructorParameter.Name, x.ConstructorParameterName, StringComparison.OrdinalIgnoreCase)
        );

    extension<TSource>(IEnumerable<TSource> source)
    {
        public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector) =>
            DistinctBy(source, keySelector, null);

        public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            return DistinctByIterator(source, keySelector, comparer);
        }
    }

    private static IEnumerable<TSource> DistinctByIterator<TSource, TKey>(
        IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer
    )
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;
        var set = new HashSet<TKey>(comparer);
        do
        {
            var element = enumerator.Current;
            if (set.Add(keySelector(element)))
            {
                yield return element;
            }
        } while (enumerator.MoveNext());
    }
}
