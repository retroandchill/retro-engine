using System.Text;
using MagicArchive.Formatters;

// ReSharper disable BuiltInTypeReferenceStyle

namespace MagicArchive.Utilities;

internal static class WellKnownTypeRegistration
{
    public static void RegisterWellKnownTypesFormatters()
    {
        ArchiveFormatterRegistry.Register(new BlittableFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Char>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Char>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Char>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Single>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Single>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Single>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Double>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Double>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Double>());
        ArchiveFormatterRegistry.Register(new BlittableFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new BooleanFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Boolean>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Boolean>());
        ArchiveFormatterRegistry.Register(new GuidFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Guid>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Guid>());
        ArchiveFormatterRegistry.Register(new DateTimeOffsetFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<DateTimeOffset>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<DateTimeOffset>());
        ArchiveFormatterRegistry.Register(new StringFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<String>());
    }
}
