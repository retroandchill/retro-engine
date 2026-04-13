// // @file ReferenceSymbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MagicArchive.SourceGenerator.Utils;

public sealed class ReferenceSymbols
{
    public const string FormatterNamespace = "global::MagicArchive.Formatters";

    public Compilation Compilation { get; }

    public SemanticModel SemanticModel { get; }

    public INamedTypeSymbol IArchivable { get; }

    public WellKnownTypes KnownTypes { get; }

    public ReferenceSymbols(Compilation compilation, SemanticModel semanticModel)
    {
        Compilation = compilation;
        SemanticModel = semanticModel;

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
    }
}
