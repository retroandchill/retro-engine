// @file Name.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using MagicArchive;
using RetroEngine.Interop;
using RetroEngine.Portable.Serialization.Json;
using RetroEngine.Portable.Serialization.Yaml;
using VYaml.Serialization;

namespace RetroEngine.Portable.Strings;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct NameEntryId(uint value)
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
        return NativeCompareLexical(
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_compare_lexical")]
    private static partial int NativeCompareLexical(NameEntryId lhs, NameEntryId rhs, NameCase nameCase);
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
/// It also provides serialization support for MagicArchive and JSON via custom formatters.
/// </remarks>
/// <threadsafety>
/// This type is thread-safe due to its immutable nature.
/// </threadsafety>
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(NameJsonConverter))]
[Archivable(GenerateType.Custom)]
public readonly partial struct Name
    : IEquatable<Name>,
        IEquatable<string>,
        IEquatable<ReadOnlySpan<char>>,
        IComparable<Name>,
        IEqualityOperators<Name, Name, bool>,
        IEqualityOperators<Name, string?, bool>
{
    private const int MaxLength = 1024;

    private const int MaxDigits = 10;

    public const int MaxRenderedLength = MaxLength + MaxDigits + 1;

    private const int NoNumberInternal = 0;

    public const string NoneString = "None";

    /// <summary>
    /// Represents a "none" or null-like state for <see cref="Name"/> instances.
    /// </summary>
    public const int NoNumber = NoNumberInternal - 1;

    private static int NameInternalToExternal(int x) => x - 1;

    private static int ExternalToNameInternal(int x) => x + 1;

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

    private Name(scoped ReadOnlySpan<byte> name, FindName findType = FindName.Add)
    {
        NativeLookup(name, name.Length, findType, out this, out var error);
        error.ThrowIfError();
    }

    public static Name FromUtf8Bytes(scoped ReadOnlySpan<byte> name, FindName findType = FindName.Add)
    {
        return new Name(name, findType);
    }

    /// <summary>
    /// Construct a new <see cref="Name"/> from a <see cref="ReadOnlySpan{char}"/>
    /// </summary>
    /// <param name="name">The characters to construct from.</param>
    /// <param name="findType">
    /// Used to determine if we should add a new name or simply try to retrieve an existing one.
    /// </param>
    public Name(scoped ReadOnlySpan<char> name, FindName findType = FindName.Add)
    {
        NativeLookup(name, name.Length, findType, out this, out var error);
        error.ThrowIfError();
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
    public bool IsValid => NativeIsValid(this);

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
        return NativeCompare(lhs, rhs, rhs.Length) == 0;
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

    public int CompareLexical(Name other, StringComparison comparison)
    {
        var lexicalCompare = ComparisonIndex.CompareLexical(other.ComparisonIndex, comparison);
        return lexicalCompare != 0 ? lexicalCompare : _number.CompareTo(other._number);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var maxLength = Encoding.UTF8.GetMaxCharCount(MaxRenderedLength);
        Span<char> buffer = stackalloc char[maxLength];
        var newLength = NativeToString(this, buffer, buffer.Length);
        return buffer[..newLength].ToString();
    }

    internal int WriteUtf8Bytes(scoped Span<byte> buffer)
    {
        return NativeToString(this, buffer, buffer.Length);
    }

    public int WriteUtf16Bytes(scoped Span<char> buffer)
    {
        return NativeToString(this, buffer, buffer.Length);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(ComparisonIndex, _number);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_lookup_utf8")]
    private static partial void NativeLookup(
        ReadOnlySpan<byte> name,
        int nameLength,
        FindName findType,
        out Name result,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_lookup_utf16")]
    private static partial void NativeLookup(
        ReadOnlySpan<char> name,
        int nameLength,
        FindName findType,
        out Name result,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_is_valid")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsValid(Name name);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_compare_utf16")]
    private static partial int NativeCompare(Name lhs, scoped ReadOnlySpan<char> name, int nameLength);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_to_string_utf8")]
    private static partial int NativeToString(Name name, scoped ReadOnlySpan<byte> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_name_to_string_utf16")]
    private static partial int NativeToString(Name name, scoped ReadOnlySpan<char> buffer, int bufferLength);

    static partial void StaticInit()
    {
        GeneratedResolver.Register(new NameYamlConverter());
    }

    static void IArchivable<Name>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Name value
    )
    {
        Span<byte> buffer = stackalloc byte[MaxRenderedLength];
        var writtenLength = value.WriteUtf8Bytes(buffer);
        ref var dest = ref writer.GetSpanReference(writtenLength + sizeof(int));
        if (!writer.IsByteSwapping)
        {
            Unsafe.WriteUnaligned(ref dest, writtenLength);
        }
        else
        {
            Unsafe.WriteUnaligned(ref dest, BinaryPrimitives.ReverseEndianness(writtenLength));
        }

        Unsafe.CopyBlockUnaligned(
            ref Unsafe.Add(ref dest, sizeof(int)),
            ref MemoryMarshal.GetReference(buffer),
            (uint)writtenLength
        );
        writer.Advance(writtenLength + sizeof(int));
    }

    static void IArchivable<Name>.Deserialize(ref ArchiveReader reader, scoped ref Name value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            ArchiveSerializationException.ThrowDeserializeObjectIsNull(nameof(Name));
        }

        ref var spanRef = ref reader.GetSpanReference(length);
        var span = MemoryMarshal.CreateReadOnlySpan(ref spanRef, length);
        value = new Name(span);
        reader.Advance(length);
    }
}
