// // @file TypeName.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

public readonly struct TypeName : IEquatable<TypeName>
{
    private readonly bool _isRefNullable;

    public CodeAnalyzing.TypeInfo TypeInfo { get; }

    public string Name => TypeInfo.Name;

    public string FullyQualifiedName => TypeInfo.GetFullyQualifiedName(_isRefNullable);

    public TypeName(CodeAnalyzing.TypeInfo typeInfo, bool isRefNullable)
    {
        TypeInfo = typeInfo;
        _isRefNullable = isRefNullable;
    }

    public bool Equals(TypeName other) => _isRefNullable == other._isRefNullable && TypeInfo.Equals(other.TypeInfo);

    public override bool Equals(object? obj) => obj is TypeName other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (_isRefNullable.GetHashCode() * 397) ^ TypeInfo.GetHashCode();
        }
    }

    public override string ToString() => FullyQualifiedName;

    public static bool operator ==(TypeName left, TypeName right) => left.Equals(right);

    public static bool operator !=(TypeName left, TypeName right) => !(left == right);
}
