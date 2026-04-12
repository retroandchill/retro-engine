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
    public INamedTypeSymbol? CustomFormatter { get; }
    public string? CustomFormatterName { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }
    public bool IsAssignable { get; }
    public bool IsConstructorParameter { get; }
    public string? ConstructorParameterName { get; }
    public int Order { get; }
    public int OrderPlusOne => Order + 1;
    public bool HasExplicitOrder { get; }
    public MemberKind Kind { get; }
    public bool SuppressDefaultInitialization { get; }
    public string DefaultValue { get; }

    private MemberMetadata(int order)
    {
        Symbol = null!;
        Name = null!;
        MemberType = null!;
        DefaultValue = null!;
        Order = order;
        Kind = MemberKind.Blank;
    }

    public MemberMetadata(ISymbol symbol, IMethodSymbol? constructor, SemanticModel semanticModel, int sequentialOrder)
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
            if (parameter is not null)
            {
                ConstructorParameterName = parameter.Name;
                var defaultValue = parameter
                    .DeclaringSyntaxReferences.Select(x => x.GetSyntax())
                    .OfType<ParameterSyntax>()
                    .FirstOrDefault(x => x.Identifier.ValueText == parameter.Name)
                    ?.Default;
                if (defaultValue is not null)
                    DefaultValue = defaultValue.Value.ToString();
            }
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
                if (DefaultValue is null)
                {
                    var defaultValue = field
                        .DeclaringSyntaxReferences.Select(x => x.GetSyntax())
                        .OfType<FieldDeclarationSyntax>()
                        .SelectMany(x => x.Declaration.Variables)
                        .Where(x => x.Identifier.ValueText == field.Name && x.Initializer is not null)
                        .Select(x => x.Initializer!.Value.ToString())
                        .FirstOrDefault();
                    if (defaultValue is not null)
                        DefaultValue = defaultValue;
                }
                break;
            case IPropertySymbol property:
                IsProperty = true;
                IsField = false;
                IsSettable = !property.IsReadOnly;
                IsAssignable = IsSettable && property is { IsRequired: false, SetMethod.IsInitOnly: false };
                MemberType = property.Type;
                if (DefaultValue is null)
                {
                    var defaultValue = property
                        .DeclaringSyntaxReferences.Select(x => x.GetSyntax())
                        .OfType<PropertyDeclarationSyntax>()
                        .Where(x => IsValidInitializer(x.Initializer, semanticModel))
                        .Select(x => x.Initializer!.Value.ToString())
                        .FirstOrDefault();
                    if (defaultValue is not null)
                        DefaultValue = defaultValue;
                }
                break;
            default:
                throw new ArgumentException("Symbol must be a field or property", nameof(symbol));
        }

        DefaultValue ??= $"default({MemberType.FullyQualifiedToString()})";

        // TODO: Eventually we will want to allow for custom formatters, but not right now

        Kind = ParseMemberKind(symbol, MemberType);
    }

    private static bool IsValidInitializer(EqualsValueClauseSyntax? initializer, SemanticModel semanticModel)
    {
        if (initializer is null)
            return false;

        foreach (var id in initializer.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            var symbol = semanticModel.GetSymbolInfo(id).Symbol;
            if (symbol is IParameterSymbol param)
                return false;
        }

        return true;
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

    public string EmitSerialize(string writer)
    {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (Kind)
        {
            case MemberKind.Archivable:
            case MemberKind.ArchivableArray:
            case MemberKind.ArchivableList:
                return $"{writer}.WriteArchivable(value.@{Name});";
            case MemberKind.Enum:
                return $"{writer}.WriteEnum(value.@{Name});";
            case MemberKind.Blank:
                return "";
            case MemberKind.CustomFormatter:
                return $"{writer}.WriteWithFormatter(__{Name}Formatter, value.@{Name});";
            default:
                return $"{writer}.Write(value.@{Name});";
        }
    }

    public string EmitDeserialize(string reader)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (Kind)
        {
            case MemberKind.Archivable:
                return $"{reader}.ReadArchivable<{MemberType.FullyQualifiedToString()}>()";
            case MemberKind.Nullable:
            {
                var namedNullable = (INamedTypeSymbol)MemberType;
                return $"{reader}.ReadNullable<{namedNullable.TypeArguments[0].FullyQualifiedToString()}>()";
            }
            case MemberKind.Bool:
                return $"{reader}.ReadBool()";
            case MemberKind.Char:
                return $"{reader}.ReadChar()";
            case MemberKind.Rune:
                return $"{reader}.ReadRune()";
            case MemberKind.Byte:
                return $"{reader}.ReadByte()";
            case MemberKind.SByte:
                return $"{reader}.ReadSByte()";
            case MemberKind.Int16:
                return $"{reader}.ReadInt16()";
            case MemberKind.UInt16:
                return $"{reader}.ReadUInt16()";
            case MemberKind.Int32:
                return $"{reader}.ReadInt32()";
            case MemberKind.UInt32:
                return $"{reader}.ReadUInt32()";
            case MemberKind.Int64:
                return $"{reader}.ReadInt64()";
            case MemberKind.UInt64:
                return $"{reader}.ReadUInt64()";
            case MemberKind.Single:
                return $"{reader}.ReadSingle()";
            case MemberKind.Double:
                return $"{reader}.ReadDouble()";
            case MemberKind.Guid:
                return $"{reader}.ReadGuid()";
            case MemberKind.DateTimeOffset:
                return $"{reader}.ReadDateTimeOffset()";
            case MemberKind.String:
                return $"{reader}.ReadString()";
            case MemberKind.Array:
            {
                var arrayType = (IArrayTypeSymbol)MemberType;
                var elemType = arrayType.ElementType;
                return $"{reader}.ReadArray<{elemType.FullyQualifiedToString()}>()";
            }
            case MemberKind.ArchivableList:
            {
                var listType = (INamedTypeSymbol)MemberType;
                return $"{reader}.ReadArchivableList<{listType.TypeArguments[0].FullyQualifiedToString()}>()";
            }
            case MemberKind.Enum:
                return $"{reader}.ReadEnum<{MemberType.FullyQualifiedToString()}>()";
            case MemberKind.Blank:
                break;
            case MemberKind.CustomFormatter:
            {
                var mt = MemberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return $"reader.ReadValueWithFormatter<{CustomFormatterName}, {mt}>(__{Name}Formatter)";
            }
        }

        return $"{reader}.Read<{MemberType.FullyQualifiedToString()}>();";
    }
}
