// // @file GenricType.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using MagicArchive;

namespace RetroEngine.Scripting.Model;

/// <summary>
/// An unbound generic type.
/// </summary>
[Archivable]
public sealed partial class GenericType(string name)
    : TypeSymbol(name),
        IEquatable<TypeSymbol>,
        IEquatable<TypeSpecifier>,
        IEquatable<GenericType>,
        IEqualityOperators<GenericType, TypeSymbol, bool>,
        IEqualityOperators<GenericType, TypeSpecifier, bool>,
        IEqualityOperators<GenericType, GenericType, bool>
{
    /// <summary>
    /// Constraints for this generic type.
    /// </summary>
    public ImmutableArray<TypeSpecifier> Constraints { get; init; } = [];

    /// <summary>
    /// Any special constraints this generic type has.
    /// </summary>
    public GenericParameterAttributes ParameterAttributes { get; init; } = GenericParameterAttributes.None;

    /// <inheritdoc />
    public override string FullCodeNameUnbound => "";

    /// <summary>
    /// Creates a GenericType from a type. Type must be a generic argument.
    /// </summary>
    /// <typeparam name="T">Type to generate GenericType for.</typeparam>
    /// <returns>GenericType for the passed type.</returns>
    public static GenericType FromType<T>()
        where T : allows ref struct
    {
        return FromType(typeof(T));
    }

    /// <summary>
    /// Creates a GenericType from a type. Type must be a generic argument.
    /// </summary>
    /// <param name="type">Type to generate GenericType for.</param>
    /// <returns>GenericType for the passed type.</returns>
    public static GenericType FromType(Type type)
    {
        if (!type.IsGenericParameter)
            throw new ArgumentException("Type is not a generic parameter", nameof(type));

        return new GenericType(type.Name)
        {
            Constraints = [.. type.GetGenericParameterConstraints().Select(TypeSpecifier.FromType)],
            ParameterAttributes = type.GenericParameterAttributes,
        };
    }

    /// <inheritdoc />
    public bool Equals(TypeSymbol? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        return other switch
        {
            TypeSpecifier specifier => Equals(specifier),
            GenericType genericType => Equals(genericType),
            _ => false,
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as TypeSymbol);
    }

    /// <inheritdoc />
    public bool Equals(TypeSpecifier? other)
    {
        // TODO: Check constraints
        return false;
    }

    /// <inheritdoc />
    public bool Equals(GenericType? other)
    {
        // TODO: Check constraints
        if (ReferenceEquals(this, other))
            return true;

        return other is not null && Name == other.Name;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <inheritdoc />
    public static bool operator ==(GenericType? left, TypeSymbol? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(GenericType? left, TypeSymbol? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public static bool operator ==(GenericType? left, TypeSpecifier? right)
    {
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(GenericType? left, TypeSpecifier? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public static bool operator ==(GenericType? left, GenericType? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(GenericType? left, GenericType? right)
    {
        return !(left == right);
    }
};
