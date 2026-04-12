// // @file TypeMetadata.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MagicArchive.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Model;

public enum ClassType
{
    Class,
    Struct,
    Record,
    RecordStruct,
    Interface,
}

[UsedImplicitly]
public readonly record struct AdditionalTypeRegistration(string TypeName, string FormatterName);

public class TypeMetadata
{
    private DiagnosticDescriptor? _ctorInvalid;

    private readonly ReferenceSymbols _reference;
    public GenerateType GenerateType { get; }
    public SerializeLayout SerializeLayout { get; }
    public INamedTypeSymbol Symbol { get; }

    [UsedImplicitly]
    public string Namespace { get; }

    [UsedImplicitly]
    public ClassType ClassType { get; }

    [UsedImplicitly]
    public string Name { get; }

    [UsedImplicitly]
    public string SimpleName { get; }

    [UsedImplicitly]
    public string NullableName { get; }

    [UsedImplicitly]
    public ImmutableArray<AdditionalTypeRegistration> AdditionalTypeRegistrations { get; }
    public ImmutableArray<MemberMetadata> Members { get; }

    [UsedImplicitly]
    public ImmutableArray<MemberMetadata> PreConstructMembers { get; }

    [UsedImplicitly]
    public ImmutableArray<MemberMetadata> PostConstructMembers { get; }

    [UsedImplicitly]
    public int MemberCount => Members.Length;

    [UsedImplicitly]
    public bool IsValueType { get; }
    public bool IsInterfaceOrAbstract { get; }
    public bool IsUnion { get; }
    public bool IsRecord { get; }

    [UsedImplicitly]
    public bool IsTolerant => GenerateType is GenerateType.VersionTolerant or GenerateType.CircularReference;

    [UsedImplicitly]
    public bool IsCircularReference => GenerateType is GenerateType.CircularReference;

    [UsedImplicitly]
    public bool RequiresConstruct => PreConstructMembers.Length > 0 || Constructor is { Parameters.Length: > 0 };

    [UsedImplicitly]
    public bool HasDefault { get; }

    [UsedImplicitly]
    public bool IsCustom => GenerateType is GenerateType.Custom;

    public bool UsesEmptyConstructor => Constructor is null || Constructor.Parameters.IsEmpty;

    public IMethodSymbol? Constructor { get; }

    public TypeMetadata(INamedTypeSymbol symbol, ReferenceSymbols referenceSymbols)
    {
        Symbol = symbol;
        _reference = referenceSymbols;

        symbol.TryGetArchivableType(out var generateType, out var serializeLayout);
        GenerateType = generateType;
        SerializeLayout = serializeLayout;

        Namespace = symbol.ContainingNamespace.ToDisplayString();
        ClassType = GetClassType();
        Name = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        SimpleName = symbol.Name;
        NullableName = symbol.IsValueType ? Name : $"{Name}?";

        Constructor = ChooseConstructor(symbol);

        Members =
        [
            .. symbol
                .GetAllMembers()
                .Where(x =>
                    x
                        is (IFieldSymbol or IPropertySymbol)
                            and { IsStatic: false, IsImplicitlyDeclared: false, CanBeReferencedByName: true }
                )
                .Reverse()
                .DistinctBy(x => x.Name)
                .Reverse()
                .Where(x =>
                {
                    var include = x.HasAttribute<ArchiveIncludeAttribute>();
                    var ignore = x.HasAttribute<ArchiveIgnoreAttribute>();
                    if (ignore)
                        return false;
                    if (include)
                        return true;
                    return x.DeclaredAccessibility is Accessibility.Public;
                })
                .Where(x =>
                {
                    if (x is not IPropertySymbol p)
                        return true;
                    if (p.GetMethod is null && p.SetMethod is not null)
                        return false;

                    return !p.IsIndexer;
                })
                .Select((x, i) => new MemberMetadata(x, Constructor, _reference, i))
                .OrderBy(x => x.Order),
        ];

        var preConstructEnd = Members.Length - 1;
        while (preConstructEnd >= 0)
        {
            if (!Members[preConstructEnd].IsAssignable || Members[preConstructEnd].IsConstructorParameter)
                break;

            preConstructEnd--;
        }

        PreConstructMembers = Members[..(preConstructEnd + 1)];
        PostConstructMembers = Members[(preConstructEnd + 1)..];

        AdditionalTypeRegistrations = GetAdditionalTypeRegistrations();

        IsValueType = symbol.IsValueType;
        IsInterfaceOrAbstract = symbol.TypeKind is TypeKind.Interface || symbol.IsAbstract;
        IsUnion = symbol.HasAttribute<ArchivableUnionAttribute>();
        IsRecord = symbol.IsRecord;
        HasDefault = Constructor is null || Constructor.Parameters.IsEmpty;

        if (GenerateType is GenerateType.VersionTolerant or GenerateType.CircularReference)
        {
            if (Members.Length != 0)
            {
                var maxOrder = Members.Max(x => x.Order);
                var tempMembers = new MemberMetadata[maxOrder + 1];
                for (var i = 0; i <= maxOrder; i++)
                {
                    tempMembers[i] = Members.FirstOrDefault(x => x.Order == i) ?? MemberMetadata.CreateEmpty(i);
                    tempMembers[i].Index = i;
                }
                Members = ImmutableCollectionsMarshal.AsImmutableArray(tempMembers);
            }
        }
    }

    private ClassType GetClassType()
    {
        if (Symbol.TypeKind == TypeKind.Interface)
            return ClassType.Interface;

        if (Symbol.IsValueType)
        {
            return Symbol.IsRecord ? ClassType.RecordStruct : ClassType.Struct;
        }

        return Symbol.IsRecord ? ClassType.Record : ClassType.Class;
    }

    private IMethodSymbol? ChooseConstructor(INamedTypeSymbol symbol)
    {
        var ctors = symbol.InstanceConstructors.Where(x => !x.IsImplicitlyDeclared).ToArray();

        switch (ctors.Length)
        {
            case 0:
                return null;
            case 1:
                return ctors[0];
            default:
            {
                var ctorsWithAttribute = ctors.Where(x => x.HasAttribute<ArchivableConstructorAttribute>()).ToArray();

                switch (ctorsWithAttribute.Length)
                {
                    case 0:
                        _ctorInvalid = DiagnosticDescriptors.MultipleCtorWithoutAttribute;
                        return null;
                    case 1:
                        return ctorsWithAttribute[0];
                    default:
                        _ctorInvalid = DiagnosticDescriptors.MultipleCtorAttribute;
                        return null;
                }
            }
        }
    }

    public bool Validate(TypeDeclarationSyntax syntax, SourceProductionContext context)
    {
        var noError = true;

        switch (GenerateType)
        {
            case GenerateType.NoGenerate or GenerateType.Custom:
                return true;
            case GenerateType.Collection:
                // TODO: We'll do collections later
                return false;
            case GenerateType.CircularReference when !UsesEmptyConstructor:
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CircularReferenceOnlyAllowsParameterlessConstructor,
                        syntax.Identifier.GetLocation(),
                        Symbol.Name
                    )
                );
                return false;
        }

        if (Symbol.IsRefLikeType)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticDescriptors.TypeIsRefStruct, syntax.Identifier.GetLocation(), Symbol.Name)
            );
            return false;
        }

        if (IsInterfaceOrAbstract && !IsUnion)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticDescriptors.AbstractMustUnion, syntax.Identifier.GetLocation(), Symbol.Name)
            );
            noError = false;
        }

        if (_ctorInvalid != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(_ctorInvalid, syntax.Identifier.GetLocation(), Symbol.Name));
            noError = false;
        }

        if (Constructor is not null)
        {
            foreach (var parameter in Constructor.Parameters)
            {
                if (Members.ContainsConstructorParameter(parameter))
                    continue;
                var location = Constructor.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ConstructorHasNoMatchedParameter,
                        location,
                        Symbol.Name,
                        parameter.Name
                    )
                );
                noError = false;
            }
        }

        foreach (var item in Members)
        {
            if (item.IsField && ((IFieldSymbol)item.Symbol).IsReadOnly && !item.IsConstructorParameter)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.ReadOnlyFieldMustBeConstructorMember,
                        item.GetLocation(syntax),
                        Symbol.Name,
                        item.Name
                    )
                );
                noError = false;
            }
            else if (item is { SuppressDefaultInitialization: true, IsAssignable: false })
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.SuppressDefaultInitializationMustBeSettable,
                        item.GetLocation(syntax),
                        Symbol.Name,
                        item.Name
                    )
                );
                noError = false;
            }
        }

        if (Symbol.BaseType != null)
        {
            // Member override member can't annotate[Ignore][Include]
            foreach (var item in Symbol.GetAllMembers(withoutOverride: false))
            {
                if (item.IsOverride)
                {
                    var include = item.HasAttribute<ArchiveIncludeAttribute>();
                    var ignore = item.HasAttribute<ArchiveIgnoreAttribute>();
                    if (include || ignore)
                    {
                        var location = item.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();

                        var attr = include ? "ArchiveInclude" : "ArchiveIgnore";
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.OverrideMemberCantAddAnnotation,
                                location,
                                Symbol.Name,
                                item.Name,
                                attr
                            )
                        );
                        noError = false;
                    }
                }
            }

            // inherit type can not serialize parent private member
            foreach (var item in Symbol.GetParentMembers())
            {
                var include = item.HasAttribute<ArchiveIncludeAttribute>();
                if (!include || item.DeclaredAccessibility != Accessibility.Private)
                    continue;
                var location = item.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InheritTypeCanNotIncludeParentPrivateMember,
                        location,
                        Symbol.Name,
                        item.Name
                    )
                );
                noError = false;
            }
        }

        // ALl Members
        if (Members.Length >= 250) // MemoryPackCode.Reserved1
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.MembersCountOver250,
                    syntax.Identifier.GetLocation(),
                    Symbol.Name,
                    Members.Length
                )
            );
            noError = false;
        }

        // exists can't serialize member
        foreach (var item in Members)
        {
            if (item.Kind == MemberKind.NonSerializable)
            {
                if (
                    item.MemberType.SpecialType
                        is SpecialType.System_Object
                            or SpecialType.System_Array
                            or SpecialType.System_Delegate
                            or SpecialType.System_MulticastDelegate
                    || item.MemberType.TypeKind == TypeKind.Delegate
                )
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MemberCantSerializeType,
                            item.GetLocation(syntax),
                            Symbol.Name,
                            item.Name,
                            item.MemberType.FullyQualifiedToString()
                        )
                    );
                    noError = false;
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.MemberIsNotArchivable,
                            item.GetLocation(syntax),
                            Symbol.Name,
                            item.Name,
                            item.MemberType.FullyQualifiedToString()
                        )
                    );
                    noError = false;
                }
            }
            else if (item.Kind == MemberKind.RefLike)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.MemberIsRefStruct,
                        item.GetLocation(syntax),
                        Symbol.Name,
                        item.Name,
                        item.MemberType.FullyQualifiedToString()
                    )
                );
                noError = false;
            }
        }

        // order
        if (SerializeLayout == SerializeLayout.Explicit)
        {
            // All members must annotate MemoryPackOrder
            foreach (var item in Members)
            {
                if (!item.HasExplicitOrder && !item.IsBlank)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.AllMembersMustAnnotateOrder,
                            item.GetLocation(syntax),
                            Symbol.Name,
                            item.Name
                        )
                    );
                    noError = false;
                }
            }

            // don't allow duplicate order
            var orderSet = new Dictionary<int, MemberMetadata>(Members.Length);
            foreach (var item in Members)
            {
                if (orderSet.TryGetValue(item.Order, out var duplicateMember))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.DuplicateOrderDoesNotAllow,
                            item.GetLocation(syntax),
                            Symbol.Name,
                            item.Name,
                            duplicateMember.Name
                        )
                    );
                    noError = false;
                }
                else
                {
                    orderSet.Add(item.Order, item);
                }
            }

            // Annotated MemoryPackOrder must be continuous number from zero if GenerateType.Object.
            if (noError && GenerateType == GenerateType.Object)
            {
                var expectedOrder = 0;
                foreach (var item in Members)
                {
                    if (item.Order != expectedOrder)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.AllMembersMustBeContinuousNumber,
                                item.GetLocation(syntax),
                                Symbol.Name,
                                item.Name
                            )
                        );
                        noError = false;
                        break;
                    }

                    expectedOrder++;
                }
            }
        }

        return noError;
    }

    public string EmitConstructorParameters()
    {
        return string.Join(
            ", ",
            PreConstructMembers
                .Where(x => x.IsConstructorParameter)
                .Select(x => $"{x.ConstructorParameterName}: __{x.Name}__")
        );
    }

    private ImmutableArray<AdditionalTypeRegistration> GetAdditionalTypeRegistrations()
    {
        if (IsCustom)
            return [];

        var collector = new TypeCollector();
        collector.Visit(this, false);

        return
        [
            .. collector
                .Select(x => (Type: x, Formatter: _reference.KnownTypes.GetNonDefaultFormatterName(x)))
                .Where(x => x.Formatter is not null)
                .Select(x => new AdditionalTypeRegistration(x.Type.FullyQualifiedToString(), x.Formatter!)),
        ];
    }
}
