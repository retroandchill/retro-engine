// @file Name.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using DefaultNamespace;
using JetBrains.Annotations;
using MessagePack;
using RetroEngine.Strings.Serialization.Json;
#if RETRO_NAME_BACKEND_NATIVE
using RetroEngine.Strings.Interop;
#endif

namespace RetroEngine.Strings;

[StructLayout(LayoutKind.Sequential)]
public readonly struct NameEntryId(uint value)
    : IEquatable<NameEntryId>,
        IComparable<NameEntryId>,
        IComparisonOperators<NameEntryId, NameEntryId, bool>
{
    public uint Value { get; } = value;

    public bool IsNone => Value == 0;

    public static NameEntryId None => new(0);

    public override bool Equals(object? obj)
    {
        return obj is NameEntryId other && Equals(other);
    }

    public bool Equals(NameEntryId other)
    {
        return Value == other.Value;
    }

    public int CompareLexical(NameEntryId other, StringComparison comparison)
    {
#if RETRO_NAME_BACKEND_NATIVE
        return NameExporter.CompareLexical(
            this,
            other,
            comparison switch
            {
                StringComparison.CurrentCulture => NameCase.CaseSensitive,
                StringComparison.CurrentCultureIgnoreCase => NameCase.IgnoreCase,
                StringComparison.InvariantCulture => NameCase.CaseSensitive,
                StringComparison.InvariantCultureIgnoreCase => NameCase.IgnoreCase,
                StringComparison.Ordinal => NameCase.CaseSensitive,
                StringComparison.OrdinalIgnoreCase => NameCase.IgnoreCase,
                _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null),
            }
        );
#else
        return NameTable.Instance.Compare(this, other, comparison);
#endif
    }

    public int CompareTo(NameEntryId other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator ==(NameEntryId left, NameEntryId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NameEntryId left, NameEntryId right)
    {
        return !(left == right);
    }

    public static bool operator >(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NameEntryId left, NameEntryId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public override int GetHashCode()
    {
        return (int)Value;
    }
}

public enum NameCase : byte
{
    CaseSensitive,
    IgnoreCase,
}

/// <summary>
/// Enumeration used to determine whether to add a new name or try to retrieve an existing one.
/// </summary>
public enum FindName : byte
{
    /// <summary>
    /// Find an existing name, but do not try to add a new one.
    /// </summary>
    Find,

    /// <summary>
    /// Will add a new name if it does not already exist.
    /// </summary>
    Add,
}

/// <summary>
/// Represents an immutable and compact string-like type with efficient comparison,
/// serialization, and interop capabilities.
/// </summary>
/// <remarks>
/// The <c>Name</c> struct offers efficient equality and comparison operations,
/// as well as interop between <see cref="string"/> and <see cref="ReadOnlySpan{char}"/>.
/// It also provides serialization support for MessagePack and JSON via custom formatters.
/// </remarks>
/// <threadsafety>
/// This type is thread-safe due to its immutable nature.
/// </threadsafety>
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(NameJsonConverter))]
[MessagePackFormatter(typeof(NameMessagePackFormatter))]
public readonly struct Name
    : IEquatable<Name>,
        IEquatable<string>,
        IEquatable<ReadOnlySpan<char>>,
        IComparable<Name>,
        IEqualityOperators<Name, Name, bool>,
        IEqualityOperators<Name, string?, bool>
{
    [PublicAPI]
    public const int MaxLength = 1024;

    private const int NoNumberInternal = 0;

    /// <summary>
    /// Represents a "none" or null-like state for <see cref="Name"/> instances.
    /// </summary>
    public const int NoNumber = NoNumberInternal - 1;

    private static int NameInternalToExternal(int x) => x - 1;

    private static int ExternalToNameInternal(int x) => x + 1;

#if !RETRO_NAME_BACKEND_NATIVE
    internal const string NoneString = "None";
#endif

    /// <summary>
    /// Gets the comparison index for this <see cref="Name"/>. This is the primary value used for equality comparisons.
    /// </summary>
    [UsedImplicitly]
    public NameEntryId ComparisonIndex { get; }

    private readonly int _number;

    /// <summary>
    /// Gets the number of the name. A number that is greater than 0 indicates that the name is a numbered name, which
    /// means callees to <see cref="ToString"/> will return the one less than the number as a suffix.
    /// </summary>
    public int Number
    {
        get => NameInternalToExternal(_number);
        init => _number = ExternalToNameInternal(value);
    }

    /// <summary>
    /// Gets the display string index for this <see cref="Name"/>. This is the way in which Name is able to get back
    /// the correct case-sensitive string representation of the name.
    /// </summary>
    [UsedImplicitly]
#if RETRO_WITH_CASE_PRESERVING_NAME
    public NameEntryId DisplayIndex { get; }
#else
    public NameEntryId DisplayIndex => ComparisonIndex;
#endif

    /// <summary>
    /// Construct a new <see cref="Name"/> from a <see cref="ReadOnlySpan{char}"/>
    /// </summary>
    /// <param name="name">The characters to construct from.</param>
    /// <param name="findType">
    /// Used to determine if we should add a new name or simply try to retrieve an existing one.
    /// </param>
    public Name(ReadOnlySpan<char> name, FindName findType = FindName.Add)
    {
#if RETRO_NAME_BACKEND_NATIVE
        unsafe
        {
            fixed (char* namePtr = name)
            {
                this = NameExporter.Lookup(namePtr, name.Length, findType);
            }
        }
#else
        (var indices, _number) = LookupName(name, findType);
        ComparisonIndex = indices.ComparisonIndex;
#if RETRO_WITH_CASE_PRESERVING_NAME
        DisplayIndex = indices.DisplayIndex;
#endif
#endif
    }

    /// <summary>
    /// Construct a new <see cref="Name"/> from a <see cref="string"/>
    /// </summary>
    /// <param name="name">The characters to construct from.</param>
    /// <param name="findType">
    /// Used to determine if we should add a new name or simply try to retrieve an existing one.
    /// </param>
    public Name(string name, FindName findType = FindName.Add)
        : this(name.AsSpan(), findType) { }

    /// <summary>
    /// Gets a predefined, immutable <see cref="Name"/> instance representing a "none" or null-like state.
    /// </summary>
    /// <remarks>
    /// This property serves as a default or neutral value for instances of <see cref="Name"/>.
    /// It is particularly useful in scenarios where a valid, meaningful value is not applicable or has not been assigned.
    /// </remarks>
    /// <returns>
    /// A <see cref="Name"/> instance configured to represent the "none" state.
    /// </returns>
    /// <threadsafety>
    /// This property is thread-safe due to the immutable nature of the <see cref="Name"/> struct.
    /// </threadsafety>
    public static Name None => new();

    /// <summary>
    /// Gets a value indicating whether the current <see cref="Name"/> instance represents a valid state.
    /// </summary>
    /// <remarks>
    /// This property checks the underlying value of the <see cref="Name"/> instance to determine if it represents
    /// a meaningful or valid state. An instance may be invalid if it is in a "none" state or has not been initialized
    /// properly, depending on the platform-specific implementation.
    /// </remarks>
    /// <returns>
    /// A boolean value; <c>true</c> if the <see cref="Name"/> instance is valid, otherwise <c>false</c>.
    /// </returns>
    /// <threadsafety>
    /// This property is thread-safe due to the immutable nature of the <see cref="Name"/> struct.
    /// </threadsafety>
#if RETRO_NAME_BACKEND_NATIVE
    public bool IsValid => NameExporter.IsValid(this);
#else
    public bool IsValid => NameTable.Instance.IsWithinBounds(ComparisonIndex);
#endif

    /// <summary>
    /// Indicates whether the current <see cref="Name"/> instance represents a "none" or null-like state.
    /// </summary>
    /// <remarks>
    /// This property allows checking if the <see cref="Name"/> instance is equivalent to the predefined "none" state,
    /// providing a reliable mechanism to determine neutrality or absence of a meaningful value.
    /// The evaluation is based on internal comparisons with the static <see cref="Name.None"/> property.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if the current instance represents the "none" state; otherwise, <c>false</c>.
    /// </returns>
    /// <threadsafety>
    /// This property is thread-safe due to the immutable nature of the <see cref="Name"/> struct.
    /// </threadsafety>
    public bool IsNone => this == None;

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The first <see cref="Name"/> to compare.</param>
    /// <param name="rhs">The second <see cref="Name"/> to compare.</param>
    /// <returns>
    /// <c>true</c> if the two <see cref="Name"/> values are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Name lhs, Name rhs)
    {
        return lhs.ComparisonIndex == rhs.ComparisonIndex && lhs._number == rhs._number;
    }

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are not equal.
    /// </summary>
    /// <param name="lhs">The first <see cref="Name"/> instance to compare.</param>
    /// <param name="rhs">The second <see cref="Name"/> instance to compare.</param>
    /// <returns><c>true</c> if the two <see cref="Name"/> instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Name lhs, Name rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The left-hand side <see cref="Name"/> instance.</param>
    /// <param name="rhs">The right-hand side <see cref="Name"/> instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Name lhs, string? rhs)
    {
        return rhs is not null && lhs == rhs.AsSpan();
    }

    /// <summary>
    /// Compares whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The first <see cref="Name"/> instance to compare.</param>
    /// <param name="rhs">The second <see cref="Name"/> instance to compare.</param>
    /// <returns>Returns <c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Name lhs, string? rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Compares a <see cref="Name"/> instance with a <see cref="ReadOnlySpan{char}"/> for equality.
    /// </summary>
    /// <param name="lhs">The <see cref="Name"/> on the left-hand side of the equality operator.</param>
    /// <param name="rhs">The <see cref="ReadOnlySpan{char}"/> on the right-hand side of the equality operator.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="lhs"/> and <paramref name="rhs"/> are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Name lhs, ReadOnlySpan<char> rhs)
    {
#if RETRO_NAME_BACKEND_NATIVE
        unsafe
        {
            fixed (char* rhsPtr = rhs)
            {
                return NameExporter.Compare(lhs, rhsPtr, rhs.Length) == 0;
            }
        }
#else
        var (number, newLength) = ParseNumber(rhs);
        return NameTable.Instance.Compare(lhs.ComparisonIndex, rhs[..newLength], StringComparison.OrdinalIgnoreCase)
                == 0
            && number == lhs._number;
#endif
    }

    /// <summary>
    /// Determines inequality between a <see cref="Name"/> and a <see cref="ReadOnlySpan{char}"/>.
    /// </summary>
    /// <param name="lhs">The <see cref="Name"/> instance to compare.</param>
    /// <param name="rhs">The <see cref="ReadOnlySpan{char}"/> to compare against.</param>
    /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Name lhs, ReadOnlySpan<char> rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Defines the implicit conversion from a <see cref="string"/> to a <see cref="Name"/>.
    /// </summary>
    /// <param name="name">The string to convert to a <see cref="Name"/>.</param>
    /// <returns>A new <see cref="Name"/> instance containing the given string.</returns>
    public static implicit operator Name(string name) => new(name);

    /// <summary>
    /// Converts a <see cref="Name"/> instance to a <see cref="string"/> implicitly.
    /// </summary>
    /// <param name="name">The <see cref="Name"/> instance to convert.</param>
    /// <returns>A <see cref="string"/> representation of the <see cref="Name"/> instance.</returns>
    public static implicit operator string(Name name) => name.ToString();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Name other && Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(Name other)
    {
        return this == other;
    }

    /// <inheritdoc />
    public bool Equals(string? other)
    {
        return this == other;
    }

    /// <inheritdoc />
    public bool Equals(ReadOnlySpan<char> other)
    {
        return this == other;
    }

    /// <inheritdoc />
    public int CompareTo(Name other)
    {
        return ComparisonIndex.CompareTo(other.ComparisonIndex);
    }

    /// <inheritdoc />
    public override string ToString()
    {
#if RETRO_NAME_BACKEND_NATIVE
        Span<char> buffer = stackalloc char[MaxLength];
        int newLength;
        unsafe
        {
            fixed (char* ch = buffer)
            {
                newLength = NameExporter.ToString(this, ch, buffer.Length);
            }
        }

        return buffer[..newLength].ToString();
#else
        var baseString = NameTable.Instance.Get(DisplayIndex);
        return _number != NoNumberInternal ? $"{baseString}_{NameInternalToExternal(_number)}" : baseString;
#endif
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(ComparisonIndex, _number);
    }

#if !RETRO_NAME_BACKEND_NATIVE
    private static (NameIndices Indices, int Number) LookupName(ReadOnlySpan<char> value, FindName findType)
    {
        if (value.Length == 0)
            return (NameIndices.None, NoNumberInternal);

        var (internalNumber, newLength) = ParseNumber(value);
        var newSlice = value[..newLength];
        var indices = NameTable.Instance.GetOrAddEntry(newSlice, findType);
        return !indices.IsNone ? (indices, internalNumber) : (NameIndices.None, NoNumberInternal);
    }

    private static (int Number, int Length) ParseNumber(ReadOnlySpan<char> name)
    {
        var digits = 0;
        for (var i = name.Length - 1; i >= 0; i--)
        {
            var character = name[i];
            if (character is < '0' or > '9')
                break;

            digits++;
        }

        var firstDigit = name.Length - digits;
        if (firstDigit == 0)
            return (NoNumberInternal, name.Length);

        const int maxDigits = 10;
        if (
            digits <= 0
            || digits >= name.Length
            || name[firstDigit - 1] != '_'
            || digits > maxDigits
            || digits != 1 && name[firstDigit] == '0'
        )
            return (NoNumberInternal, name.Length);

        return int.TryParse(name.Slice(firstDigit, digits), out var number)
            ? (ExternalToNameInternal(number), name.Length - (digits + 1))
            : (NoNumberInternal, name.Length);
    }
#endif
}
