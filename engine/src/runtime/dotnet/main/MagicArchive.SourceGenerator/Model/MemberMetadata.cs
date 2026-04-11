// // @file MemberMetadata.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Types;

namespace MagicArchive.SourceGenerator.Model;

public enum MemberKind
{
    Archivable,
    Nullable,
    Bool,
    Char,
    Rune,
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Single,
    Double,
    Guid,
    DateTimeOffset,
    String,
    Array,
    List,
    ArchivableArray,
    ArchivableList,
    ArchivableCollection,
    ArchivableNoGenerate,
    ArchivableUnion,
    Enum,

    AllowSerialize,
    Union,

    Object,
    RefLike,
    NonSerializable,
    Blank,
    CustomFormatter,
}

public class MemberMetadata
{
    public ISymbol Symbol { get; }
    public string Name { get; }
    public ITypeSymbol MemberType { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }
    public bool IsAssignable { get; }
    public bool IsConstructorParameter { get; }
    public string? ConstructorParameterName { get; }
    public int Order { get; }
    public bool HasExplicitOrder { get; }
    public MemberKind Kind { get; }
    public bool SuppressDefaultInitialization { get; }

    private MemberMetadata(int order)
    {
        Symbol = null!;
        Name = null!;
        MemberType = null!;
        Order = order;
        Kind = MemberKind.Blank;
    }

    public MemberMetadata(ISymbol symbol, IMethodSymbol? constructor, int sequentialOrder)
    {
        Symbol = symbol;
        Name = symbol.Name;
        Order = sequentialOrder;
        SuppressDefaultInitialization = symbol.HasAttribute<SuppressDefaultInitializationAttribute>();
        if (symbol.TryGetArchiveOrder(out var order))
        {
            Order = order;
            HasExplicitOrder = true;
        }
        else
        {
            HasExplicitOrder = false;
        }

        if (constructor is not null)
        {
            IsConstructorParameter = constructor.TryGetConstructorParameter(symbol, out var parameter);
            ConstructorParameterName = parameter?.Name;
        }
        else
        {
            IsConstructorParameter = false;
        }

        switch (symbol)
        {
            case IFieldSymbol field:
                IsProperty = false;
                IsField = true;
                IsSettable = !field.IsReadOnly;
                IsAssignable = IsSettable && !field.IsRequired;
                MemberType = field.Type;
                break;
            case IPropertySymbol property:
                IsProperty = true;
                IsField = false;
                IsSettable = !property.IsReadOnly;
                IsAssignable = IsSettable && property is { IsRequired: false, SetMethod.IsInitOnly: false };
                MemberType = property.Type;
                break;
            default:
                throw new ArgumentException("Symbol must be a field or property", nameof(symbol));
        }

        // TODO: Eventually we will want to allow for custom formatters, but not right now

        Kind = ParseMemberKind(symbol, MemberType);
    }

    public static MemberMetadata CreateEmpty(int order)
    {
        return new MemberMetadata(order);
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        var location = Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        return location;
    }

    private static MemberKind ParseMemberKind(ISymbol? memberSymbol, ITypeSymbol memberType)
    {
        switch (memberType.SpecialType)
        {
            case SpecialType.System_Object
            or SpecialType.System_Array
            or SpecialType.System_Delegate
            or SpecialType.System_MulticastDelegate:
                return MemberKind.NonSerializable; // object, Array, delegate is not allowed
            case SpecialType.System_Boolean:
                return MemberKind.Bool;
            case SpecialType.System_Char:
                return MemberKind.Char;
            case SpecialType.System_Byte:
                return MemberKind.Byte;
            case SpecialType.System_SByte:
                return MemberKind.SByte;
            case SpecialType.System_Int16:
                return MemberKind.Int16;
            case SpecialType.System_UInt16:
                return MemberKind.UInt16;
            case SpecialType.System_Int32:
                return MemberKind.Int32;
            case SpecialType.System_UInt32:
                return MemberKind.UInt32;
            case SpecialType.System_Int64:
                return MemberKind.Int64;
            case SpecialType.System_UInt64:
                return MemberKind.UInt64;
            case SpecialType.System_Single:
                return MemberKind.Single;
            case SpecialType.System_Double:
                return MemberKind.Double;
            case SpecialType.System_String:
                return MemberKind.String;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (memberType.TypeKind)
        {
            case TypeKind.Delegate:
                return MemberKind.NonSerializable;
            case TypeKind.Enum:
                return MemberKind.Enum;
        }

        if (memberType.AllInterfaces.Any(x => x.IsArchivableInterface))
        {
            return MemberKind.Archivable;
        }

        if (memberType.TryGetArchivableType(out var generateType, out _))
        {
            switch (generateType)
            {
                case GenerateType.Object:
                case GenerateType.VersionTolerant:
                case GenerateType.CircularReference:
                case GenerateType.Custom:
                    return MemberKind.Object;
                case GenerateType.Collection:
                    return MemberKind.ArchivableCollection;
                case GenerateType.NoGenerate:
                default:
                    return MemberKind.ArchivableNoGenerate;
            }
        }

        if (memberType.WillImplementArchivableUnion())
            return MemberKind.Union;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (memberType.TypeKind)
        {
            case TypeKind.Array:
            {
                if (memberType is not IArrayTypeSymbol array)
                    return MemberKind.NonSerializable;

                if (!array.IsSZArray)
                    return array.Rank <= 4 ? MemberKind.Object : MemberKind.NonSerializable;

                var elemType = array.ElementType;
                if (
                    elemType.TryGetArchivableType(out var elemGenerateType, out _)
                    && elemGenerateType
                        is GenerateType.Object
                            or GenerateType.VersionTolerant
                            or GenerateType.CircularReference
                            or GenerateType.Custom
                )
                {
                    return MemberKind.ArchivableArray;
                }

                return MemberKind.Array;
            }
            case TypeKind.TypeParameter:
                return MemberKind.Object;
        }

        if (memberType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsRefLikeType)
                return MemberKind.RefLike;

            if (namedTypeSymbol.IsNullableValueType)
                return MemberKind.Nullable;

            if (namedTypeSymbol.IsListType)
            {
                if (
                    namedTypeSymbol.TypeArguments[0].TryGetArchivableType(out var elemGenerateType, out _)
                    && elemGenerateType
                        is GenerateType.Object
                            or GenerateType.VersionTolerant
                            or GenerateType.CircularReference
                            or GenerateType.Custom
                )
                {
                    return MemberKind.ArchivableList;
                }

                return MemberKind.List;
            }

            if (namedTypeSymbol.IsSameType<Guid>())
                return MemberKind.Guid;
            if (namedTypeSymbol.IsSameType<DateTimeOffset>())
                return MemberKind.DateTimeOffset;
            if (namedTypeSymbol.ToDisplayString() == "System.Test.Rune")
                return MemberKind.Rune;
        }

        if (memberSymbol is not null && memberSymbol.HasAttribute<ArchiveAllowSerializeAttribute>())
        {
            return MemberKind.AllowSerialize;
        }

        return MemberKind.NonSerializable;
    }
}
