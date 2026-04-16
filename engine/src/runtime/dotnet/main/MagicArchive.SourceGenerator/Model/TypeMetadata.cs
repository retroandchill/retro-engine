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
public record AdditionalTypeRegistration(string TypeName, string FormatterName);

public record UnionTag(ushort Tag, INamedTypeSymbol Type)
{
    public required string FullyQualifiedName { get; init; }
    public required string WriteMethod { get; init; }
    public required string ReadMethod { get; init; }
}

public readonly record struct BlittableRegistration(bool IsBlittable, bool IsComplex, string TypeName);

public class TypeMetadata
{
    private DiagnosticDescriptor? _ctorInvalid;

    private readonly ReferenceSymbols _reference;
    public INamedTypeSymbol Symbol { get; set; }
    public GenerateType GenerateType { get; }
    public SerializeLayout SerializeLayout { get; }
    public string TypeName { get; }
    public ImmutableArray<MemberMetadata> Members { get; }
    public bool IsValueType { get; }
    public bool IsBlittable { get; }
    public bool IsUnion { get; }
    public bool IsRecord { get; }
    public bool IsInterfaceOrAbstract { get; }
    public IMethodSymbol? Constructor { get; }
    public ImmutableArray<MethodMetadata> OnSerializing { get; }
    public ImmutableArray<MethodMetadata> OnSerialized { get; }
    public ImmutableArray<MethodMetadata> OnDeserializing { get; }
    public ImmutableArray<MethodMetadata> OnDeserialized { get; }
    public ImmutableArray<UnionTag> UnionTags { get; }

    public bool UsesEmptyConstructor => Constructor is null || Constructor.Parameters.IsEmpty;

    public TypeMetadata(INamedTypeSymbol symbol, ReferenceSymbols referenceSymbols)
    {
        Symbol = symbol;
        _reference = referenceSymbols;

        symbol.TryGetArchivableType(out var generateType, out var serializeLayout);
        GenerateType = generateType;
        SerializeLayout = serializeLayout;

        TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

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

        IsValueType = symbol.IsValueType;
        IsBlittable = symbol.IsBlittable(_reference, out _);
        IsInterfaceOrAbstract = symbol.TypeKind is TypeKind.Interface || symbol.IsAbstract;
        IsUnion = symbol.HasAttribute<ArchivableUnionAttribute>();
        IsRecord = symbol.IsRecord;
        OnSerializing = CollectMethods<ArchivableOnSerializingAttribute>(IsValueType, false);
        OnSerialized = CollectMethods<ArchivableOnSerializedAttribute>(IsValueType, false);
        OnDeserializing = CollectMethods<ArchivableOnDeserializingAttribute>(IsValueType, true);
        OnDeserialized = CollectMethods<ArchivableOnDeserializedAttribute>(IsValueType, true);

        if (IsUnion)
        {
            UnionTags =
            [
                .. symbol
                    .GetArchivableUnionInfos()
                    .Select(x =>
                    {
                        var namedSymbol = (INamedTypeSymbol)x.Type;
                        var displayName = ToUnionTagTypeFullyQualifiedToString(namedSymbol);
                        string writeMethodName;
                        string readMethodName;
                        if (
                            x.Type.TryGetArchivableType(out var genType, out _)
                            && genType
                                is GenerateType.Object
                                    or GenerateType.VersionTolerant
                                    or GenerateType.CircularReference
                        )
                        {
                            writeMethodName = "WriteArchivable";
                            readMethodName = "ReadArchivable";
                        }
                        else
                        {
                            writeMethodName = "Write";
                            readMethodName = "Read";
                        }
                        return new UnionTag(x.Tag, namedSymbol)
                        {
                            FullyQualifiedName = displayName,
                            WriteMethod = writeMethodName,
                            ReadMethod = readMethodName,
                        };
                    }),
            ];
        }
        else
        {
            UnionTags = [];
        }

        if (symbol.TypeKind != TypeKind.Class)
            return;
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

    private ImmutableArray<MethodMetadata> CollectMethods<TAttribute>(bool isValueType, bool isReader)
        where TAttribute : Attribute
    {
        return
        [
            .. Symbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute<TAttribute>())
                .Select(x => new MethodMetadata(x, isValueType, isReader)),
        ];
    }

    private string ToUnionTagTypeFullyQualifiedToString(INamedTypeSymbol type)
    {
        if (type.IsGenericType && Symbol.IsGenericType)
        {
            // when generic type, it is unconstructed.( typeof(T<>) ) so construct symbol's T
            var typeName = string.Join(", ", this.Symbol.TypeArguments.Select(x => x.FullyQualifiedToString()));
            return type.FullyQualifiedToString().Replace("<>", "<" + typeName + ">");
        }
        else
        {
            return type.FullyQualifiedToString();
        }
    }

    private static IEnumerable<BlittableRegistration> GetBlittableSubTypes(
        ITypeSymbol? type,
        ReferenceSymbols referenceSymbols
    )
    {
        if (
            type is null
            || type.SpecialType
                is SpecialType.System_Enum
                    or SpecialType.System_Char
                    or SpecialType.System_SByte
                    or SpecialType.System_Byte
                    or SpecialType.System_Int16
                    or SpecialType.System_UInt16
                    or SpecialType.System_Int32
                    or SpecialType.System_UInt32
                    or SpecialType.System_Int64
                    or SpecialType.System_UInt64
                    or SpecialType.System_Single
                    or SpecialType.System_Double
                    or SpecialType.System_DateTime
            || referenceSymbols.KnownTypes.Contains(type)
        )
            yield break;

        if (type.IsBlittable(referenceSymbols, out var isComplex))
        {
            yield return new BlittableRegistration(true, isComplex, type.FullyQualifiedToString());
        }

        switch (type)
        {
            case IArrayTypeSymbol arrayType:
            {
                foreach (var item in GetBlittableSubTypes(arrayType.ElementType, referenceSymbols))
                {
                    yield return item;
                }

                break;
            }
            case INamedTypeSymbol { IsGenericType: true } namedType:
            {
                foreach (var item in namedType.TypeArguments.SelectMany(x => GetBlittableSubTypes(x, referenceSymbols)))
                {
                    yield return item;
                }

                break;
            }
        }

        if (type.IsUnmanagedType)
        {
            yield return new BlittableRegistration(false, false, type.FullyQualifiedToString());
        }
    }

    public bool Validate(TypeDeclarationSyntax syntax, SourceProductionContext context, bool unionFormatter)
    {
        var noError = true;
        if (unionFormatter)
        {
            return UnionValidations(syntax, context);
        }

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

        if (_ctorInvalid is not null)
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

        foreach (var item in OnSerializing.Concat(OnSerialized).Concat(OnDeserializing).Concat(OnDeserialized))
        {
            if (item.Symbol.Parameters.Length != 0)
            {
                if (item.Symbol.Parameters.Length != 0)
                {
                    if (
                        item.Symbol.Parameters[0].RefKind == RefKind.Ref
                        && item.Symbol.Parameters[1].RefKind is RefKind.Ref or RefKind.In
                    )
                    {
                        continue;
                    }
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.OnMethodHasParameter,
                        item.GetLocation(syntax),
                        Symbol.Name,
                        item.Name
                    )
                );
                noError = false;
            }
        }

        if (Symbol.BaseType is not null)
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
            foreach (var item in Members.Where(item => !item.HasExplicitOrder))
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

    private bool UnionValidations(TypeDeclarationSyntax syntax, SourceProductionContext context)
    {
        if (!IsUnion)
            return true;

        var noError = true;
        if (Symbol.IsSealed)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.SealedTypeCantBeUnion,
                    syntax.Identifier.GetLocation(),
                    Symbol.Name
                )
            );
            noError = false;
        }

        if (!Symbol.IsAbstract)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.ConcreteTypeCantBeUnion,
                    syntax.Identifier.GetLocation(),
                    Symbol.Name
                )
            );
            noError = false;
        }

        if (GenerateType != GenerateType.Union)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.GenerateTypeCannotSpeciyToUnionBaseType,
                    syntax.Identifier.GetLocation(),
                    Symbol.Name,
                    GenerateType
                )
            );
            noError = false;
        }

        if (UnionTags.Select(x => x.Tag).HasDuplicate())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticDescriptors.UnionTagDuplicate, syntax.Identifier.GetLocation(), Symbol.Name)
            );
            noError = false;
        }

        foreach (var item in UnionTags)
        {
            // type does not derived target symbol
            if (Symbol.TypeKind == TypeKind.Interface)
            {
                // interface, check interfaces.
                var check = item.Type.IsGenericType
                    ? item.Type.OriginalDefinition.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(Symbol))
                    : item.Type.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, Symbol));

                if (!check)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotImplementBaseType,
                            syntax.Identifier.GetLocation(),
                            Symbol.Name,
                            item.Type.Name
                        )
                    );
                    noError = false;
                }
            }
            else
            {
                // abstract type, check base.
                var check = item.Type.IsGenericType
                    ? item.Type.OriginalDefinition.GetAllBaseTypes().Any(x => x.EqualsUnconstructedGenericType(Symbol))
                    : item.Type.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, Symbol));

                if (!check)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotDerivedBaseType,
                            syntax.Identifier.GetLocation(),
                            Symbol.Name,
                            item.Type.Name
                        )
                    );
                    noError = false;
                }
            }

            if (item.Type.IsValueType)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UnionMemberNotAllowStruct,
                        syntax.Identifier.GetLocation(),
                        Symbol.Name,
                        item.Type.Name
                    )
                );
                noError = false;
            }

            if (item.Type.HasAttribute<ArchivableAttribute>())
                continue;
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnionMemberMustBeArchivable,
                    syntax.Identifier.GetLocation(),
                    Symbol.Name,
                    item.Type.Name
                )
            );
            noError = false;
        }
        return noError;
    }

    public string Emit(TemplateSource source, SourceProductionContext context)
    {
        if (IsUnion)
        {
            return EmitUnionTemplate(source, context);
        }

        if (GenerateType == GenerateType.Collection)
        {
            return EmitGenericCollectionTemplate(source, context);
        }

        ImmutableArray<MemberMetadata> members;
        if (GenerateType is GenerateType.VersionTolerant or GenerateType.CircularReference && Members.Length != 0)
        {
            var maxOrder = Members.Max(x => x.Order);
            var tempMembers = new MemberMetadata[maxOrder + 1];
            for (var i = 0; i <= maxOrder; i++)
            {
                tempMembers[i] = Members.FirstOrDefault(x => x.Order == i) ?? MemberMetadata.CreateEmpty(i);
            }
            members = ImmutableCollectionsMarshal.AsImmutableArray(tempMembers);
        }
        else
        {
            members = Members;
        }

        var classOrStructOrRecord = (IsRecord, IsValueType) switch
        {
            (true, true) => "record struct",
            (true, false) => "record",
            (false, true) => "struct",
            (false, false) => "class",
        };

        var containingTypeDeclarations = new List<string>();
        var containingType = Symbol.ContainingType;
        while (containingType is not null)
        {
            containingTypeDeclarations.Add(
                (containingType.IsRecord, containingType.IsValueType) switch
                {
                    (true, true) => $"partial record struct {containingType.Name}",
                    (true, false) => $"partial record {containingType.Name}",
                    (false, true) => $"partial struct {containingType.Name}",
                    (false, false) => $"partial class {containingType.Name}",
                }
            );
            containingType = containingType.ContainingType;
        }
        containingTypeDeclarations.Reverse();

        var nullable = !IsValueType ? "?" : "";

        var fixedSize = Members.All(x =>
            x.Kind is MemberKind.Bool or MemberKind.Blittable or MemberKind.Enum or MemberKind.Blank
        );
        var callbackCount = new[] { OnSerialized, OnSerializing, OnDeserialized, OnDeserializing }
            .Select(x => x.Length)
            .Sum();
        string[]? sizeofTypes;
        if (fixedSize && GenerateType == GenerateType.Object && IsValueType && callbackCount == 0)
        {
            sizeofTypes = Members
                .Select(x => x.Kind == MemberKind.Bool ? "byte" : x.MemberType.FullyQualifiedToString())
                .ToArray();
        }
        else
        {
            sizeofTypes = null;
        }

        var isCustom = GenerateType == GenerateType.Custom;
        ImmutableArray<BlittableRegistration> blittableRegistrations;
        if (isCustom)
        {
            blittableRegistrations = [.. GetBlittableSubTypes(Symbol, _reference).Distinct()];
        }
        else
        {
            blittableRegistrations =
            [
                .. Members
                    .SelectMany(x => GetBlittableSubTypes(x.MemberType, _reference))
                    .Concat(GetBlittableSubTypes(Symbol, _reference))
                    .Distinct(),
            ];
        }

        var archivableType = new
        {
            ContainingTypeDeclarations = containingTypeDeclarations,
            ClassType = classOrStructOrRecord,
            TypeName,
            SimpleName = Symbol.Name,
            Nullable = nullable,
            IsFixedSize = fixedSize && sizeofTypes is not null,
            SizeOfTypes = sizeofTypes,
            IsValueType,
            IsCustom = isCustom,
            IsBlittable,
            IsTolerant = GenerateType is GenerateType.VersionTolerant or GenerateType.CircularReference,
            IsCircularReference = GenerateType == GenerateType.CircularReference,
            MemberCount = members.Length,
            Constructor,
            OnSerializing,
            OnSerialized,
            OnDeserializing,
            OnDeserialized,
            Members = members,
            UsesEmptyConstructor,
            AdditionalTypeRegistrations = GetAdditionalTypeRegistrations(),
            BlittableRegistrations = blittableRegistrations,
        };

        return source.ArchivableTemplate(archivableType);
    }

    private string EmitUnionTemplate(TemplateSource source, SourceProductionContext context)
    {
        var classType = this switch
        {
            { IsRecord: true } => "record",
            { Symbol.TypeKind: TypeKind.Interface } => "interface",
            _ => "class",
        };

        var parameters = new
        {
            ClassType = classType,
            TypeName,
            SimpleName = Symbol.Name,
            UnionTags,
            OnSerializing,
            OnSerialized,
            OnDeserializing,
            OnDeserialized,
        };

        return source.UnionTemplate(parameters);
    }

    public string EmitUnionFormatterTemplate(TemplateSource source, INamedTypeSymbol formatterSymbol)
    {
        var isGenericDefinition = Symbol is { IsGenericType: true, IsUnboundGenericType: true };
        string baseUnionTarget;
        string baseDefinitionName;
        if (isGenericDefinition)
        {
            baseUnionTarget = Symbol.ConstructUnboundGenericType().FullyQualifiedToString();
            baseDefinitionName = formatterSymbol.ConstructUnboundGenericType().FullyQualifiedToString();
        }
        else
        {
            baseUnionTarget = "";
            baseDefinitionName = "";
        }

        var templateArgs = new
        {
            TypeName,
            UnionTarget = ToUnionTagTypeFullyQualifiedToString(Symbol),
            InitializerName = $"{TypeName.Replace("global::", "").Replace('<', '_').Replace('>', '_')}Initializer",
            UnionTags,
            OnSerializing,
            OnSerialized,
            OnDeserializing,
            OnDeserialized,
            IsGenericDefinition = isGenericDefinition,
            BaseUnionTarget = baseUnionTarget,
            BaseDefinitionName = baseDefinitionName,
        };

        return source.UnionFormatterTemplate(templateArgs);
    }

    private string EmitGenericCollectionTemplate(TemplateSource source, SourceProductionContext context)
    {
        throw new NotImplementedException();
    }

    private ImmutableArray<AdditionalTypeRegistration> GetAdditionalTypeRegistrations()
    {
        if (GenerateType == GenerateType.Custom)
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

    private IReadOnlyList<string> GetConstructorParameters()
    {
        if (Constructor is not { Parameters.Length: > 0 })
        {
            return [];
        }

        var nameDict = Members
            .Where(x => x.IsConstructorParameter)
            .ToDictionary(x => x.ConstructorParameterName, x => x.Name, StringComparer.OrdinalIgnoreCase);
        return Constructor
            .Parameters.Select(x => nameDict.TryGetValue(x.Name, out var memberName) ? memberName : null)
            .OfType<string>()
            .ToArray();
    }
}
