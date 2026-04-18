// // @file ReferenceSymbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using MagicArchive.SourceGenerator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Utils;

public sealed class ReferenceSymbols
{
    public const string FormatterNamespace = "global::MagicArchive.Formatters";

    public Compilation Compilation { get; }

    public SemanticModel SemanticModel { get; }

    public INamedTypeSymbol ArchiveCustomFormatterAttribute { get; }
    public INamedTypeSymbol IArchivable { get; }

    public WellKnownTypes KnownTypes { get; }

    public ReferenceSymbols(Compilation compilation, SemanticModel semanticModel)
    {
        Compilation = compilation;
        SemanticModel = semanticModel;

        ArchiveCustomFormatterAttribute = GetTypeByMetadataName("MagicArchive.ArchiveCustomFormatterAttribute`2")
            .ConstructUnboundGenericType();
        IArchivable = GetTypeByMetadataName("MagicArchive.IArchivable`1");
        KnownTypes = new WellKnownTypes(this);
    }

    private INamedTypeSymbol GetTypeByMetadataName(string metadataName)
    {
        return Compilation.GetTypeByMetadataName(metadataName)
            ?? throw new InvalidOperationException($"Type {metadataName} is not found in compilation.");
    }

    private INamedTypeSymbol GetType(Type type)
    {
        return GetTypeByMetadataName(type.FullName ?? type.Name);
    }

    private INamedTypeSymbol GetType<T>()
    {
        return GetType(typeof(T));
    }

    public sealed class WellKnownTypes
    {
        private readonly ReferenceSymbols _parent;

        public INamedTypeSymbol IEnumerable { get; }
        public INamedTypeSymbol ICollection { get; }
        public INamedTypeSymbol ISet { get; }
        public INamedTypeSymbol IDictionary { get; }
        public INamedTypeSymbol List { get; }

        public INamedTypeSymbol Half { get; }
        public INamedTypeSymbol Int128 { get; }
        public INamedTypeSymbol UInt128 { get; }
        public INamedTypeSymbol Decimal { get; }
        public INamedTypeSymbol Guid { get; }
        public INamedTypeSymbol Rune { get; }
        public INamedTypeSymbol Version { get; }
        public INamedTypeSymbol Uri { get; }

        public INamedTypeSymbol BigInteger { get; }
        public INamedTypeSymbol TimeZoneInfo { get; }
        public INamedTypeSymbol BitArray { get; }
        public INamedTypeSymbol StringBuilder { get; }
        public INamedTypeSymbol Type { get; }
        public INamedTypeSymbol CultureInfo { get; }
        public INamedTypeSymbol Lazy { get; }
        public INamedTypeSymbol KeyValuePair { get; }
        public INamedTypeSymbol Nullable { get; }

        public INamedTypeSymbol DateTime { get; }
        public INamedTypeSymbol DateTimeOffset { get; }
        public INamedTypeSymbol StructLayout { get; }

        private const string Memory = "global::System.Memory<>";
        private const string ReadOnlyMemory = "global::System.ReadOnlyMemory<>";
        private const string ReadOnlySequence = "global::System.Buffers.ReadOnlySequence<>";
        private const string PriorityQueue = "global::System.Collections.Generic.PriorityQueue<,>";

        private readonly HashSet<ITypeSymbol> _knownTypes;
        private readonly Dictionary<ITypeSymbol, BlittableTypeInfo> _knownBlittableTypes;

        private static readonly Dictionary<string, string> KnownGenericTypes = new()
        {
            // ArrayFormatters
            { "System.ArraySegment<>", "global::MagicArchive.Formatters.ArraySegmentFormatter<TREPLACE>" },
            { "System.Memory<>", "global::MagicArchive.Formatters.MemoryFormatter<TREPLACE>" },
            { "System.ReadOnlyMemory<>", "global::MagicArchive.Formatters.ReadOnlyMemoryFormatter<TREPLACE>" },
            {
                "System.Buffers.ReadOnlySequence<>",
                "global::MagicArchive.Formatters.ReadOnlySequenceFormatter<TREPLACE>"
            },
            // CollectionFormatters
            { "System.Collections.Generic.List<>", "global::MagicArchive.Formatters.ListFormatter<TREPLACE>" },
            { "System.Collections.Generic.Stack<>", "global::MagicArchive.Formatters.StackFormatter<TREPLACE>" },
            { "System.Collections.Generic.Queue<>", "global::MagicArchive.Formatters.QueueFormatter<TREPLACE>" },
            {
                "System.Collections.Generic.LinkedList<>",
                "global::MagicArchive.Formatters.LinkedListFormatter<TREPLACE>"
            },
            { "System.Collections.Generic.HashSet<>", "global::MagicArchive.Formatters.HashSetFormatter<TREPLACE>" },
            {
                "System.Collections.Generic.SortedSet<>",
                "global::MagicArchive.Formatters.SortedSetFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.PriorityQueue<,>",
                "global::MagicArchive.Formatters.PriorityQueueFormatter<TREPLACE>"
            },
            {
                "System.Collections.ObjectModel.ObservableCollection<>",
                "global::MagicArchive.Formatters.ObservableCollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.ObjectModel.Collection<>",
                "global::MagicArchive.Formatters.CollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.Concurrent.ConcurrentQueue<>",
                "global::MagicArchive.Formatters.ConcurrentQueueFormatter<TREPLACE>"
            },
            {
                "System.Collections.Concurrent.ConcurrentStack<>",
                "global::MagicArchive.Formatters.ConcurrentStackFormatter<TREPLACE>"
            },
            {
                "System.Collections.Concurrent.ConcurrentBag<>",
                "global::MagicArchive.Formatters.ConcurrentBagFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.Dictionary<,>",
                "global::MagicArchive.Formatters.DictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.SortedDictionary<,>",
                "global::MagicArchive.Formatters.SortedDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.SortedList<,>",
                "global::MagicArchive.Formatters.SortedListFormatter<TREPLACE>"
            },
            {
                "System.Collections.Concurrent.ConcurrentDictionary<,>",
                "global::MagicArchive.Formatters.ConcurrentDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.ObjectModel.ReadOnlyCollection<>",
                "global::MagicArchive.Formatters.ReadOnlyCollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.ObjectModel.ReadOnlyObservableCollection<>",
                "global::MagicArchive.Formatters.ReadOnlyObservableCollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.Concurrent.BlockingCollection<>",
                "global::MagicArchive.Formatters.BlockingCollectionFormatter<TREPLACE>"
            },
            // ImmutableCollectionFormatters
            {
                "System.Collections.Immutable.ImmutableArray<>",
                "global::MagicArchive.Formatters.ImmutableArrayFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableList<>",
                "global::MagicArchive.Formatters.ImmutableListFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableQueue<>",
                "global::MagicArchive.Formatters.ImmutableQueueFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableStack<>",
                "global::MagicArchive.Formatters.ImmutableStackFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableDictionary<,>",
                "global::MagicArchive.Formatters.ImmutableDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableSortedDictionary<,>",
                "global::MagicArchive.Formatters.ImmutableSortedDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableSortedSet<>",
                "global::MagicArchive.Formatters.ImmutableSortedSetFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.ImmutableHashSet<>",
                "global::MagicArchive.Formatters.ImmutableHashSetFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.IImmutableList<>",
                "global::MagicArchive.Formatters.InterfaceImmutableListFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.IImmutableQueue<>",
                "global::MagicArchive.Formatters.InterfaceImmutableQueueFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.IImmutableStack<>",
                "global::MagicArchive.Formatters.InterfaceImmutableStackFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.IImmutableDictionary<,>",
                "global::MagicArchive.Formatters.InterfaceImmutableDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Immutable.IImmutableSet<>",
                "global::MagicArchive.Formatters.InterfaceImmutableSetFormatter<TREPLACE>"
            },
            // InterfaceCollectionFormatters
            {
                "System.Collections.Generic.IEnumerable<>",
                "global::MagicArchive.Formatters.InterfaceEnumerableFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.ICollection<>",
                "global::MagicArchive.Formatters.InterfaceCollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.IReadOnlyCollection<>",
                "global::MagicArchive.Formatters.InterfaceReadOnlyCollectionFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.IList<>",
                "global::MagicArchive.Formatters.InterfaceListFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.IReadOnlyList<>",
                "global::MagicArchive.Formatters.InterfaceReadOnlyListFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.IDictionary<,>",
                "global::MagicArchive.Formatters.InterfaceDictionaryFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.IReadOnlyDictionary<,>",
                "global::MagicArchive.Formatters.InterfaceReadOnlyDictionaryFormatter<TREPLACE>"
            },
            { "System.Linq.ILookup<,>", "global::MagicArchive.Formatters.InterfaceLookupFormatter<TREPLACE>" },
            { "System.Linq.IGrouping<,>", "global::MagicArchive.Formatters.InterfaceGroupingFormatter<TREPLACE>" },
            { "System.Collections.Generic.ISet<>", "global::MagicArchive.Formatters.InterfaceSetFormatter<TREPLACE>" },
            {
                "System.Collections.Generic.IReadOnlySet<>",
                "global::MagicArchive.Formatters.InterfaceReadOnlySetFormatter<TREPLACE>"
            },
            {
                "System.Collections.Generic.KeyValuePair<,>",
                "global::MagicArchive.Formatters.KeyValuePairFormatter<TREPLACE>"
            },
            { "System.Lazy<>", "global::MagicArchive.Formatters.LazyFormatter<TREPLACE>" },
            // TupleFormatters
            { "System.Tuple<>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.Tuple<,,,,,,,>", "global::MagicArchive.Formatters.TupleFormatter<TREPLACE>" },
            { "System.ValueTuple<>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
            { "System.ValueTuple<,,,,,,,>", "global::MagicArchive.Formatters.ValueTupleFormatter<TREPLACE>" },
        };

        public WellKnownTypes(ReferenceSymbols referenceSymbols)
        {
            _parent = referenceSymbols;
            IEnumerable = GetType(typeof(IEnumerable<>)).ConstructUnboundGenericType();
            ICollection = GetType(typeof(ICollection<>)).ConstructUnboundGenericType();
            ISet = GetType(typeof(ISet<>)).ConstructUnboundGenericType();
            IDictionary = GetType(typeof(IDictionary<,>)).ConstructUnboundGenericType();
            List = GetType(typeof(List<>)).ConstructUnboundGenericType();
            Decimal = GetType<decimal>();
            Half = GetTypeByMetadataName("System.Half");
            Int128 = GetTypeByMetadataName("System.Int128");
            UInt128 = GetTypeByMetadataName("System.UInt128");
            Guid = GetType<Guid>();
            Rune = GetTypeByMetadataName("System.Text.Rune");
            Version = GetType<Version>();
            Uri = GetType<Uri>();
            BigInteger = GetType<BigInteger>();
            TimeZoneInfo = GetType<TimeZoneInfo>();
            BitArray = GetType<BitArray>();
            StringBuilder = GetType<StringBuilder>();
            Type = GetType<Type>();
            CultureInfo = GetType<CultureInfo>();
            Lazy = GetType(typeof(Lazy<>)).ConstructUnboundGenericType();
            KeyValuePair = GetType(typeof(KeyValuePair<,>)).ConstructUnboundGenericType();
            Nullable = GetType(typeof(Nullable<>)).ConstructUnboundGenericType();

            DateTime = GetType<DateTime>();
            DateTimeOffset = GetType<DateTimeOffset>();
            StructLayout = GetType<StructLayoutAttribute>();

            _knownTypes = new HashSet<ITypeSymbol>(
                [
                    IEnumerable,
                    ICollection,
                    ISet,
                    IDictionary,
                    Version,
                    Uri,
                    BigInteger,
                    TimeZoneInfo,
                    BitArray,
                    StringBuilder,
                    Type,
                    CultureInfo,
                    Lazy,
                    KeyValuePair,
                    Nullable,
                    Decimal,
                    Guid,
                    DateTime,
                    DateTimeOffset,
                ],
                SymbolEqualityComparer.Default
            );

            _knownBlittableTypes = new Dictionary<ITypeSymbol, BlittableTypeInfo>(SymbolEqualityComparer.Default)
            {
                [Rune] = BlittableTypeInfo.Simple(sizeof(uint)),
                [Half] = BlittableTypeInfo.Simple(sizeof(ushort)),
                [Int128] = BlittableTypeInfo.Simple(2 * sizeof(long)),
                [UInt128] = BlittableTypeInfo.Simple(2 * sizeof(ulong)),
                [Decimal] = BlittableTypeInfo.NotBlittable,
                [Guid] = BlittableTypeInfo.NotBlittable,
                [DateTimeOffset] = BlittableTypeInfo.NotBlittable,
            };
        }

        private INamedTypeSymbol GetTypeByMetadataName(string metadataName)
        {
            return _parent.GetTypeByMetadataName(metadataName);
        }

        private INamedTypeSymbol GetType(Type type)
        {
            return _parent.GetType(type);
        }

        private INamedTypeSymbol GetType<T>()
        {
            return _parent.GetType<T>();
        }

        public bool Contains(ITypeSymbol symbol)
        {
            var constructedSymbol = symbol;
            if (symbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
            {
                symbol = namedTypeSymbol.ConstructUnboundGenericType();
            }

            if (_knownTypes.Contains(symbol))
                return true;

            var fullyQualifiedName = symbol.FullyQualifiedToString();
            if (fullyQualifiedName is Memory or ReadOnlyMemory or ReadOnlySequence or PriorityQueue)
                return true;

            if (
                fullyQualifiedName.StartsWith("global::System.Tuple<")
                || fullyQualifiedName.StartsWith("global::System.ValueTuple<")
            )
            {
                return true;
            }

            return constructedSymbol.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(IEnumerable));
        }

        public string? GetNonDefaultFormatterName(ITypeSymbol? type)
        {
            if (type is null)
                return null;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (type.TypeKind)
            {
                case TypeKind.Enum:
                    return $"{FormatterNamespace}.SimpleBlittableFormatter<{type.FullyQualifiedToString()}>";
                case TypeKind.Array:
                    if (type is not IArrayTypeSymbol array)
                        return null;

                    if (array.IsSZArray)
                    {
                        return $"{FormatterNamespace}.ArrayFormatter<{array.ElementType.FullyQualifiedToString()}>";
                    }

                    return array.Rank switch
                    {
                        2 =>
                            $"{FormatterNamespace}.TwoDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>",
                        3 =>
                            $"{FormatterNamespace}.ThreeDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>",
                        4 =>
                            $"{FormatterNamespace}.FourDimensionalArrayFormatter<{array.ElementType.FullyQualifiedToString()}>",
                        _ => null,
                    };
            }

            if (type is not INamedTypeSymbol { IsGenericType: true } namedType)
                return null;

            var genericType = namedType.ConstructUnboundGenericType();
            var genericTypeString = genericType.ToDisplayString();

            // Nullable value type
            if (genericTypeString == "T?")
            {
                var firstTypeArgument = namedType.TypeArguments[0];
                return $"{FormatterNamespace}.NullableFormatter<{firstTypeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>";
            }

            if (!KnownGenericTypes.TryGetValue(genericTypeString, out var formatter))
                return null;

            // Known types
            var typeArgs = string.Join(
                ", ",
                namedType.TypeArguments.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            );
            return formatter.Replace("TREPLACE", typeArgs);
        }

        private const int UnknownSize = -1;

        public BlittableTypeInfo GetBlittableTypeInfo(ITypeSymbol symbol)
        {
            if (!symbol.IsUnmanagedType)
            {
                return BlittableTypeInfo.NotBlittable;
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (symbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Decimal:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                    return BlittableTypeInfo.NotBlittable;
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                    return BlittableTypeInfo.Simple(sizeof(byte));
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Char:
                    return BlittableTypeInfo.Simple(sizeof(ushort));
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Single:
                    return BlittableTypeInfo.Simple(sizeof(uint));
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Double:
                case SpecialType.System_DateTime:
                    return BlittableTypeInfo.Simple(sizeof(long));
            }

            if (symbol is not INamedTypeSymbol namedType)
                return BlittableTypeInfo.NotBlittable;

            if (namedType is { TypeKind: TypeKind.Enum, EnumUnderlyingType: { } underlyingType })
            {
                // ReSharper disable once TailRecursiveCall
                return GetBlittableTypeInfo(underlyingType);
            }

            if (_knownBlittableTypes.TryGetValue(symbol, out var result))
                return result;

            var structLayoutAttribute = namedType
                .GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, StructLayout));
            int knownSize;
            if (structLayoutAttribute is not null)
            {
                var layoutKind = (LayoutKind)structLayoutAttribute.ConstructorArguments[0].Value!;
                if (layoutKind != LayoutKind.Sequential)
                    return BlittableTypeInfo.NotBlittable;

                var pack = structLayoutAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Pack").Value.Value;
                var size = structLayoutAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Size").Value.Value;

                if (pack is int packValue && packValue != 0)
                    return BlittableTypeInfo.NotBlittable;

                if (size is int sizeValue && sizeValue != 0)
                {
                    knownSize = sizeValue;
                }
                else
                {
                    knownSize = UnknownSize;
                }
            }
            else
            {
                knownSize = UnknownSize;
            }

            var archivable = symbol.TryGetArchivableType(out var generateType, out var layout);
            if (archivable)
            {
                if (generateType != GenerateType.Object || layout == SerializeLayout.Explicit)
                {
                    return BlittableTypeInfo.NotBlittable;
                }

                foreach (var member in symbol.GetMembers())
                {
                    switch (member)
                    {
                        case IMethodSymbol methodSymbol
                            when methodSymbol.HasAttribute<ArchivableConstructorAttribute>()
                                || methodSymbol.HasAttribute<ArchivableOnSerializingAttribute>()
                                || methodSymbol.HasAttribute<ArchivableOnSerializedAttribute>()
                                || methodSymbol.HasAttribute<ArchivableOnDeserializingAttribute>()
                                || methodSymbol.HasAttribute<ArchivableOnDeserializedAttribute>():
                        case IFieldSymbol { IsStatic: false, IsConst: false, IsImplicitlyDeclared: false } fieldSymbol
                            when fieldSymbol.DeclaredAccessibility != Accessibility.Public
                                && !fieldSymbol.HasAttribute<ArchiveIncludeAttribute>()
                                || fieldSymbol.DeclaredAccessibility == Accessibility.Public
                                    && fieldSymbol.HasAttribute<ArchiveIgnoreAttribute>():
                            return BlittableTypeInfo.NotBlittable;
                        case IPropertySymbol
                        {
                            IsStatic: false,
                            GetMethod: not null,
                            SetMethod: not null
                        } propertySymbol:
                        {
                            var propertySyntax =
                                propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
                                as PropertyDeclarationSyntax;
                            var hasBackingField = propertySyntax?.HasBackingField() ?? false;
                            if (
                                propertySymbol.DeclaredAccessibility != Accessibility.Public
                                    && !propertySymbol.HasAttribute<ArchiveIncludeAttribute>()
                                || propertySymbol.GetMethod.DeclaredAccessibility == Accessibility.Public
                                    && propertySymbol.HasAttribute<ArchiveIgnoreAttribute>()
                                || !hasBackingField
                            )
                                return BlittableTypeInfo.NotBlittable;
                            break;
                        }
                    }
                }
            }

            var fullyQualifiedName = symbol.FullyQualifiedToString();
            var fields = symbol.GetMembers().OfType<IFieldSymbol>().Where(x => !x.IsStatic && !x.IsConst).ToArray();
            if (
                fields.Length == 1
                && GetBlittableTypeInfo(fields[0].Type) is { Type: BlittableType.BlittableSimple } info
            )
            {
                _knownBlittableTypes[symbol] = info;
                return info;
            }

            if (!fullyQualifiedName.StartsWith("global::System.ValueTuple<") && !archivable)
            {
                return BlittableTypeInfo.NotBlittable;
            }

            var fieldWiseBlittablity = GetFieldWiseBlittablity(fields, knownSize);
            _knownBlittableTypes[symbol] = fieldWiseBlittablity;
            return fieldWiseBlittablity;
        }

        private BlittableTypeInfo GetFieldWiseBlittablity(IFieldSymbol[] fields, int knownSize)
        {
            var maxAlignment = 0;
            var runningSize = 0;
            var blittableType = BlittableType.BlittableSimple;
            var i = 0;
            foreach (var field in fields)
            {
                var fieldTypeInfo = GetBlittableTypeInfo(field.Type);
                if (fieldTypeInfo.Type == BlittableType.NotBlittable)
                    return BlittableTypeInfo.NotBlittable;

                if (i > 0 || fieldTypeInfo.IsComplex)
                {
                    blittableType = BlittableType.BlittableComplex;
                }

                if (fieldTypeInfo.Alignment > maxAlignment)
                {
                    maxAlignment = fieldTypeInfo.Alignment;
                }

                if (runningSize % fieldTypeInfo.Alignment != 0)
                {
                    return BlittableTypeInfo.NotBlittable;
                }

                runningSize += fieldTypeInfo.Size;
                i++;
            }

            if (
                maxAlignment == 0
                || runningSize % maxAlignment != 0
                || knownSize != UnknownSize && runningSize != knownSize
            )
            {
                return BlittableTypeInfo.NotBlittable;
            }

            var blittableInfo = new BlittableTypeInfo(blittableType, runningSize, maxAlignment);
            return blittableInfo;
        }
    }
}
