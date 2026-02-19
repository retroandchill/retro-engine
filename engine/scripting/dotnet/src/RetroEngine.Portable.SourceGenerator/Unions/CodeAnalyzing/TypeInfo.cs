// // @file ClrTypeInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

public sealed partial class TypeInfo : IEquatable<TypeInfo>
{
    private readonly bool _isReferenceType;
    private readonly string _fullyQualifiedName;

    public string Name { get; }

    public string? Namespace { get; }

    public TypeInfo? ContainingType { get; }

    public TypeKind Kind { get; }

    public TypeInfo(string? ns, TypeInfo? containingType, string name, string fullyQualifiedName, TypeKind kind)
    {
        Name = name;
        Namespace = ns;
        ContainingType = containingType;
        Kind = kind;
        _fullyQualifiedName = fullyQualifiedName;
        _isReferenceType = kind.Match(_ => true, (_, _) => false, () => true);
    }

    public string GetFullyQualifiedName(bool isRefNullable) =>
        _isReferenceType && isRefNullable ? $"{_fullyQualifiedName}?" : _fullyQualifiedName;

    public bool Equals(TypeInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _fullyQualifiedName == other._fullyQualifiedName;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is TypeInfo other && Equals(other));

    public override int GetHashCode() => _fullyQualifiedName.GetHashCode();

    public override string ToString() => GetFullyQualifiedName(false);

    public static TypeInfo SpecificType(string? ns, TypeInfo? containingType, string name, TypeKind kind)
    {
        var namespacePart = !string.IsNullOrEmpty(ns) ? $"{ns}." : string.Empty;
        var containingTypesPart =
            containingType != null ? $"{string.Join(".", FlattenNesting(containingType))}." : string.Empty;
        return new TypeInfo(ns, containingType, name, $"global::{namespacePart}{containingTypesPart}{name}", kind);
    }

    public static TypeInfo SpecialName(string name, TypeKind kind) => new(null, null, name, name, kind);

    public static bool operator ==(TypeInfo? left, TypeInfo? right) => Equals(left, right);

    public static bool operator !=(TypeInfo? left, TypeInfo? right) => !Equals(left, right);

    private static IEnumerable<string> FlattenNesting(TypeInfo? typeInfo)
    {
        if (typeInfo is null)
        {
            yield break;
        }

        foreach (var name in FlattenNesting(typeInfo.ContainingType))
        {
            yield return name;
        }

        yield return typeInfo.Name;
    }

    public enum ReferenceTypeKind
    {
        Class,
        Interface,
        Record,
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct BlittableKindData
    {
        [field: FieldOffset(0)]
        public ReferenceTypeKind Kind { get; init; }

        [field: FieldOffset(0)]
        public (bool IsUnmanaged, bool IsRecord) Value { get; init; }
    }

    public readonly struct TypeKind
    {
        private byte Index { get; init; }
        private BlittableKindData Data { get; init; }

        public static TypeKind ReferenceType(ReferenceTypeKind kind)
        {
            return new TypeKind
            {
                Index = 1,
                Data = new BlittableKindData { Kind = kind },
            };
        }

        public static TypeKind ValueType(bool isUnmanaged, bool isRecord)
        {
            return new TypeKind
            {
                Index = 2,
                Data = new BlittableKindData { Value = (isUnmanaged, isRecord) },
            };
        }

        public static TypeKind Unknown()
        {
            return new TypeKind { Index = 0 };
        }

        public T Match<T>(
            Func<ReferenceTypeKind, T> referenceTypeCase,
            Func<bool, bool, T> valueTypeCase,
            Func<T> unknownCase
        )
        {
            return Index switch
            {
                0 => unknownCase(),
                1 => referenceTypeCase(Data.Kind),
                2 => valueTypeCase(Data.Value.IsUnmanaged, Data.Value.IsRecord),
                _ => throw new ArgumentOutOfRangeException(nameof(Index), Index, null),
            };
        }

        public bool TryGetValueTypeData(out bool isUnmanaged, out bool isRecord)
        {
            if (Index != 2)
            {
                isUnmanaged = false;
                isRecord = false;
                return false;
            }

            isUnmanaged = Data.Value.IsUnmanaged;
            isRecord = Data.Value.IsRecord;
            return true;
        }

        public string ToCodeString() =>
            Match(
                kind =>
                    kind switch
                    {
                        ReferenceTypeKind.Class => "class",
                        ReferenceTypeKind.Interface => "interface",
                        ReferenceTypeKind.Record => "record",
                        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
                    },
                (_, isRecord) => isRecord ? "record struct" : "struct",
                () => throw new InvalidOperationException("Unknown type")
            );
    }
}
