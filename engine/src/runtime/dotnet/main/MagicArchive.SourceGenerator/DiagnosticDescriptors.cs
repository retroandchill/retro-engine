// // @file DiagnosticDescriptors.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace MagicArchive.SourceGenerator;

public class DiagnosticDescriptors
{
    private const string Category = "GenerateMagicArchive";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "MAGARCH001",
        title: "Archivable object must be partial",
        messageFormat: "The Archivable object '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnknownError = new(
        id: "MAGARCH002",
        title: "Unexpected error occured during code generation",
        messageFormat: "An unexpected error occurred during code generation for type '{0}': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AbstractMustUnion = new(
        id: "MAGARCH003",
        title: "abstract/interface type of Archivable object must annotate with Union",
        messageFormat: "abstract/interface type of Archivable object '{0}' must annotate with Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MultipleCtorWithoutAttribute = new(
        id: "MAGARCH004",
        title: "Require [ArchivableConstructor] when exists multiple constructors",
        messageFormat: "The Archivable object '{0}' must annotate with [ArchivableConstructor] when exists multiple constructors",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MultipleCtorAttribute = new(
        id: "MAGARCH005",
        title: "[ArchivableConstructor] exists in multiple constructors",
        messageFormat: "Mupltiple [ArchivableConstructor] exists in '{0}' but allows only single ctor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ConstructorHasNoMatchedParameter = new(
        id: "MAGARCH006",
        title: "Archivable's constructor has no matched parameter",
        messageFormat: "The Archivable object '{0}' constructor's parameter '{1}' must match a serialized member name(case-insensitive)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor OnMethodHasParameter = new(
        id: "MAGARCH007",
        title: "ArchiveObject's On*** methods must has no parameter",
        messageFormat: "The Archivable object '{0}''s '{1}' method must has no parameter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor OnMethodInUnamannagedType = new(
        id: "MAGARCH008",
        title: "ArchiveObject's On*** methods can't annotate in unamnaged struct",
        messageFormat: "The Archivable object '{0}' is unmanaged struct that can't annotate On***Attribute however '{1}' method annotaed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor OverrideMemberCantAddAnnotation = new(
        id: "MAGARCH009",
        title: "Override member can't annotate Ignore/Include attribute",
        messageFormat: "The Archivable object '{0}' override member '{1}' can't annotate {2} attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor SealedTypeCantBeUnion = new(
        id: "MAGARCH010",
        title: "Sealed type can't be union",
        messageFormat: "The Archivable object '{0}' is sealed type so can't be Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ConcreteTypeCantBeUnion = new(
        id: "MAGARCH011",
        title: "Concrete type can't be union",
        messageFormat: "The Archivable object '{0}' can be Union, only allow abstract or interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnionTagDuplicate = new(
        id: "MAGARCH012",
        title: "Union tag is duplicate",
        messageFormat: "The Archivable object '{0}' union tag value is duplicate",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnionMemberTypeNotImplementBaseType = new(
        id: "MAGARCH013",
        title: "Union member not implement union interface",
        messageFormat: "The Archivable object '{0}' union member '{1}' not implement union interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnionMemberTypeNotDerivedBaseType = new(
        id: "MAGARCH014",
        title: "Union member not dervided union base type",
        messageFormat: "The Archivable object '{0}' union member '{1}' not derived union type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnionMemberNotAllowStruct = new(
        id: "MAGARCH015",
        title: "Union member can't be struct",
        messageFormat: "The Archivable object '{0}' union member '{1}' can't be member, not allows struct",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnionMemberMustBeArchivable = new(
        id: "MAGARCH016",
        title: "Union member must be Archivable",
        messageFormat: "The Archivable object '{0}' union member '{1}' must be Archivable",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MembersCountOver250 = new(
        id: "MAGARCH017",
        title: "Members count limit",
        messageFormat: "The Archivable object '{0}' member count is '{1}', however limit size is 249",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MemberCantSerializeType = new(
        id: "MAGARCH018",
        title: "Member can't serialize type",
        messageFormat: "The Archivable object '{0}' member '{1}' type is '{2}' that can't serialize",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MemberIsNotArchivable = new(
        id: "MAGARCH019",
        title: "Member is not Archivable object",
        messageFormat: "The Archivable object '{0}' member '{1}' type '{2}' is not Archivable. Annotate [Archivable] to '{2}' or if external type that can serialize, annotate `[ArchiveAllowSerialize]` to member",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor TypeIsRefStruct = new(
        id: "MAGARCH020",
        title: "Type is ref struct",
        messageFormat: "The Archivable object '{0}' is ref struct, it can not serialize",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MemberIsRefStruct = new(
        id: "MAGARCH021",
        title: "Member is ref struct",
        messageFormat: "The Archivable object '{0}' member '{1}' type '{2}' is ref struct, it can not serialize",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor CollectionGenerateIsAbstract = new(
        id: "MAGARCH022",
        title: "Collection type not allows interface/abstract",
        messageFormat: "The Archivable object '{0}' is GenerateType.Collection but interface/abstract, only allows concrete type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor CollectionGenerateNotImplementedInterface = new(
        id: "MAGARCH023",
        title: "Collection type must implement collection interface",
        messageFormat: "The Archivable object '{0}' is GenerateType.Collection but not implemented collection interface(ICollection<T>/ISet<T>/IDictionary<TKey,TValue>)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor CollectionGenerateNoParameterlessConstructor = new(
        id: "MAGARCH024",
        title: "Collection type must require parameterless constructor",
        messageFormat: "The Archivable object '{0}' is GenerateType.Collection but not exists parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AllMembersMustAnnotateOrder = new(
        id: "MAGARCH025",
        title: "All members must annotate ArchiveOrder when SerializeLayout.Explicit",
        messageFormat: "The Archivable object '{0}' member '{1}' is not annotated ArchiveOrder",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AllMembersMustBeContinuousNumber = new(
        id: "MAGARCH026",
        title: "All ArchiveOrder members must be continuous number from zero",
        messageFormat: "The Archivable object '{0}' member '{1}' is not continuous number from zero",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor CircularReferenceOnlyAllowsParameterlessConstructor = new(
        id: "MAGARCH033",
        title: "CircularReference Archive Object must require parameterless constructor",
        messageFormat: "The Archivable object '{0}' is GenerateType.CircularReference but not exists parameterless constructor.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InheritTypeCanNotIncludeParentPrivateMember = new(
        id: "MAGARCH036",
        title: "Inherit type can not include private member",
        messageFormat: "Type '{0}' can not include parent type's private member '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ReadOnlyFieldMustBeConstructorMember = new(
        id: "MAGARCH037",
        title: "Readonly field must be constructor member",
        messageFormat: "Type '{0}' readonly field '{1}' must be constructor member",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DuplicateOrderDoesNotAllow = new(
        id: "MAGARCH038",
        title: "All members order must be unique",
        messageFormat: "The Archivable object '{0}' member '{1}' is duplicated order between '{2}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor GenerateTypeCannotSpeciyToUnionBaseType = new(
        id: "MAGARCH039",
        title: "GenerateType cannot be specified for the Union base type itself",
        messageFormat: "The Archivable object '{0}' cannot specify '{1}'. Because it is Union base type.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor SuppressDefaultInitializationMustBeSettable = new(
        id: "MAGARCH040",
        title: "Readonly member cannot specify [SuppressDefaultInitialization]",
        messageFormat: "The Archivable object '{0}' member '{1}' has [SuppressDefaultInitialization], it cannot be readonly, init-only and required.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor NestedContainingTypesMustBePartial = new(
        id: "MAGARCH042",
        title: "Nested Archivable object's containing type(s) must be partial",
        messageFormat: "The Archivable object '{0}' containing type(s) must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
