// // @file ArchiveSerializerOptions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive;

public enum StringEncoding : byte
{
    Utf8,
    Utf16,
}

public enum ByteOrder : byte
{
    LittleEndian,
    BigEndian,
}

public record ArchiveSerializerOptions
{
    public static ArchiveSerializerOptions Default { get; } = new();

    public static ArchiveSerializerOptions Utf8 { get; } = Default with { StringEncoding = StringEncoding.Utf8 };
    public static ArchiveSerializerOptions Utf16 { get; } = Default with { StringEncoding = StringEncoding.Utf16 };

    public static ArchiveSerializerOptions LittleEndian { get; } = Default with { ByteOrder = ByteOrder.LittleEndian };
    public static ArchiveSerializerOptions BigEndian { get; } = Default with { ByteOrder = ByteOrder.BigEndian };

    public StringEncoding StringEncoding { get; init; } = StringEncoding.Utf8;
    public ByteOrder ByteOrder { get; init; } = ByteOrder.LittleEndian;
    public bool IsPersistent { get; init; } = false;
    public IServiceProvider? ServiceProvider { get; init; }
}
