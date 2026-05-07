// // @file TypeSpecifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.InteropServices;
using Cysharp.Text;
using MagicArchive;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Scripting.Model;

/// <summary>
/// Specifier describing "real" types (not purely unbound generic).
/// </summary>
[Archivable]
public sealed partial class TypeSpecifier(string name)
    : TypeSymbol(name),
        IEquatable<TypeSpecifier>,
        IEquatable<TypeSymbol>,
        IEquatable<GenericType>,
        IEqualityOperators<TypeSpecifier, TypeSpecifier, bool>,
        IEqualityOperators<TypeSpecifier, TypeSymbol, bool>,
        IEqualityOperators<TypeSpecifier, GenericType, bool>
{
    /// <summary>
    /// Whether this type is an enum.
    /// </summary>
    public bool IsEnum { get; init; }

    /// <summary>
    /// Whether this type is an interface.
    /// </summary>
    public bool IsInterface { get; init; }

    /// <summary>
    /// Generic arguments this type takes.
    /// </summary>
    public ImmutableArray<TypeSymbol> GenericArguments { get; init; } = [];

    /// <inheritdoc />
    public override string ShortName
    {
        get
        {
            var shortNameSpan = Name.AsSpan();
            var separatorIndex = shortNameSpan.LastIndexOf('.');
            var shortName = separatorIndex == -1 ? shortNameSpan : shortNameSpan[(separatorIndex + 1)..];

            if (GenericArguments.Length <= 0)
            {
                return separatorIndex == -1 ? Name : shortName.ToString();
            }

            using var builder = new Utf16ValueStringBuilder(false);
            builder.Append(shortName);
            builder.Append('<');
            builder.AppendJoin(", ", GenericArguments.Select(t => t.ShortName));
            builder.Append('>');

            return builder.ToString();
        }
    }

    /// <inheritdoc />
    public override string FullCodeName
    {
        get
        {
            var codeName = base.FullCodeName;

            if (GenericArguments.Length <= 0)
            {
                return codeName;
            }

            using var builder = new Utf16ValueStringBuilder(false);
            builder.Append(codeName);
            builder.Append('<');
            builder.AppendJoin(", ", GenericArguments.Select(t => t.FullCodeName));
            builder.Append('>');

            return builder.ToString();
        }
    }

    /// <inheritdoc />
    public override string FullCodeNameUnbound
    {
        get
        {
            var codeName = base.FullCodeNameUnbound;

            if (GenericArguments.Length <= 0)
            {
                return codeName;
            }

            using var builder = new Utf16ValueStringBuilder(false);
            builder.Append(codeName);
            builder.Append('<');
            builder.AppendJoin(", ", GenericArguments.Select(t => t.FullCodeNameUnbound));
            builder.Append('>');

            return builder.ToString();
        }
    }

    /// <summary>
    /// Whether this type is a primitive type (eg. int, bool, float, Enum, ...).
    /// </summary>
    [ArchiveIgnore]
    public bool IsPrimitive =>
        this == FromType<byte>()
        || this == FromType<sbyte>()
        || this == FromType<short>()
        || this == FromType<ushort>()
        || this == FromType<int>()
        || this == FromType<uint>()
        || this == FromType<long>()
        || this == FromType<ulong>()
        || this == FromType<float>()
        || this == FromType<double>()
        || this == FromType<decimal>()
        || this == FromType<bool>()
        || this == FromType<char>()
        || this == FromType<string>()
        || this == FromType<Name>()
        || this == FromType<Text>()
        || IsEnum;

    /// <summary>
    /// Creates a TypeSpecifier for a given type.
    /// </summary>
    /// <typeparam name="T">Type to create a TypeSpecifier for.</typeparam>
    /// <returns>TypeSpecifier for the given type.</returns>
    public static TypeSpecifier FromType<T>()
        where T : allows ref struct
    {
        return FromType(typeof(T));
    }

    /// <summary>
    /// Creates a TypeSpecifier for a given type.
    /// </summary>
    /// <param name="type">Type to create a TypeSpecifier for.</param>
    /// <returns>TypeSpecifier for the given type.</returns>
    public static TypeSpecifier FromType(Type type)
    {
        if (type.IsGenericParameter)
        {
            throw new ArgumentException("Cannot create type specifier from generic parameter", nameof(type));
        }

        using var builder = new Utf16ValueStringBuilder(false);
        if (!string.IsNullOrEmpty(type.Namespace))
        {
            builder.Append(type.Namespace);
            builder.Append('.');
        }

        var typeNameSpan = type.Name.AsSpan();
        var genericSpecifierIndex = typeNameSpan.IndexOf('`');
        var typeName = genericSpecifierIndex == -1 ? typeNameSpan : typeNameSpan[..genericSpecifierIndex];
        builder.Append(typeName);

        return new TypeSpecifier(builder.ToString())
        {
            IsEnum = type.IsSubclassOf(typeof(Enum)),
            IsInterface = type.IsInterface,
            GenericArguments =
            [
                .. type.GetGenericArguments()
                    .Select(TypeSymbol (t) => t.IsGenericParameter ? GenericType.FromType(t) : FromType(t)),
            ],
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        return obj switch
        {
            TypeSpecifier specifier => Equals(specifier),
            GenericType genericType => Equals(genericType),
            _ => false,
        };
    }

    /// <inheritdoc />
    public bool Equals(TypeSpecifier? specifier)
    {
        if (specifier is null)
            return false;

        if (Name == specifier.Name && GenericArgumentsEqual(specifier))
        {
            return IsEnum != specifier.IsEnum
                ? throw new ArgumentException("obj has same type name but IsEnum is different")
                : true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool Equals(GenericType? other)
    {
        // TODO: Check constraints
        return false;
    }

    /// <inheritdoc />
    public bool Equals(TypeSymbol? other)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether the generic arguments for this type and a given type match.
    /// </summary>
    /// <param name="t">Specifier for the type to check.</param>
    /// <returns>Whether the generic arguments for the types match.</returns>
    public bool GenericArgumentsEqual(TypeSpecifier t)
    {
        return GenericArguments.SequenceEqual(t.GenericArguments);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Name);
        foreach (var arg in GenericArguments)
        {
            hash.Add(arg);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        using var builder = new Utf16ValueStringBuilder(false);

        var nameSpan = Name.AsSpan();
        nameSpan.Replace(builder.GetSpan(nameSpan.Length), '+', '.');
        if (GenericArguments.Length == 0)
            return builder.ToString();

        builder.Append('<');
        builder.AppendJoin(", ", GenericArguments.AsSpan());
        builder.Append('>');

        return builder.ToString();
    }

    /// <summary>
    /// Constructs this type by replacing all its generic arguments with the given types.
    /// </summary>
    /// <param name="typeSpecifiers">
    /// Specifiers for the types to replace the generic
    /// type arguments with
    /// </param>
    /// <returns>Constructed type with generic type arguments replaced by the given ones.</returns>
    public TypeSpecifier Construct(IReadOnlyDictionary<GenericType, TypeSymbol> typeSpecifiers)
    {
        if (GenericArguments.Length != typeSpecifiers.Count)
        {
            throw new ArgumentException("Need to replace all generic arguments when constructing type.");
        }

        if (typeSpecifiers.Count == 0)
            return this;

        var newGenericArgs = new TypeSymbol[GenericArguments.Length];
        for (var i = 0; i < GenericArguments.Length; i++)
        {
            var currentArg = GenericArguments[i];
            if (currentArg is GenericType genericType && typeSpecifiers.TryGetValue(genericType, out var newArg))
            {
                newGenericArgs[i] = newArg;
            }
            else
            {
                newGenericArgs[i] = currentArg;
            }
        }

        return new TypeSpecifier(Name)
        {
            IsEnum = IsEnum,
            IsInterface = IsInterface,
            GenericArguments = ImmutableCollectionsMarshal.AsImmutableArray(newGenericArgs),
        };
    }

    /// <inheritdoc />
    public static bool operator ==(TypeSpecifier? left, TypeSpecifier? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(TypeSpecifier? left, TypeSpecifier? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public static bool operator ==(TypeSpecifier? left, TypeSymbol? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(TypeSpecifier? left, TypeSymbol? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public static bool operator ==(TypeSpecifier? left, GenericType? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(TypeSpecifier? left, GenericType? right)
    {
        return !(left == right);
    }
}
