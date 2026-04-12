// // @file TypeCollector.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using MagicArchive.SourceGenerator.Model;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Utils;

public sealed class TypeCollector : IEnumerable<ITypeSymbol>
{
    private readonly HashSet<ITypeSymbol> _types = new(SymbolEqualityComparer.Default);

    public void Visit(TypeMetadata typeMetadata, bool visitInterfaces)
    {
        Visit(typeMetadata.Symbol, visitInterfaces);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        foreach (var item in typeMetadata.Members.Where(x => x.Symbol is not null))
        {
            Visit(item.MemberType, visitInterfaces);
        }
    }

    private void Visit(ISymbol symbol, bool visitInterfaces)
    {
        if (symbol is not ITypeSymbol typeSymbol)
            return;

        // 7~20 is primitive
        if ((int)typeSymbol.SpecialType is >= 7 and <= 20)
        {
            return;
        }

        if (!_types.Add(typeSymbol))
        {
            return;
        }

        switch (typeSymbol)
        {
            case IArrayTypeSymbol arrayTypeSymbol:
                // ReSharper disable once TailRecursiveCall
                Visit(arrayTypeSymbol.ElementType, visitInterfaces);
                break;
            case INamedTypeSymbol namedTypeSymbol:
            {
                if (visitInterfaces)
                {
                    foreach (var item in namedTypeSymbol.AllInterfaces)
                    {
                        Visit(item, visitInterfaces);
                    }

                    foreach (var item in namedTypeSymbol.GetAllBaseTypes())
                    {
                        Visit(item, visitInterfaces);
                    }
                }

                if (namedTypeSymbol.IsGenericType)
                {
                    foreach (var item in namedTypeSymbol.TypeArguments)
                    {
                        Visit(item, visitInterfaces);
                    }
                }

                break;
            }
        }
    }

    public IEnumerable<ITypeSymbol> GetEnums()
    {
        return _types.Where(x => x.TypeKind == TypeKind.Enum);
    }

    public IEnumerable<ITypeSymbol> GetArchivableTypes()
    {
        return _types.Where(x => x.HasAttribute<ArchivableAttribute>());
    }

    public IEnumerator<ITypeSymbol> GetEnumerator()
    {
        return _types.OfType<ITypeSymbol>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
