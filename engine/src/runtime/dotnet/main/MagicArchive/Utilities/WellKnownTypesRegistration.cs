using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;
using MagicArchive.Formatters;

// ReSharper disable BuiltInTypeReferenceStyle

namespace MagicArchive.Utilities;

internal static class WellKnownTypeRegistration
{
    public static void RegisterWellKnownTypesFormatters()
    {
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<SByte>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Byte>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int16>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt16>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int32>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt32>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Int64>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<UInt64>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Char>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Char>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Char>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Single>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Single>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Single>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Double>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Double>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Double>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<DateTime>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<DateTime>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<DateTime>());
        ArchiveFormatterRegistry.Register(new SimpleBlittableFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Rune>());
        ArchiveFormatterRegistry.Register(new BooleanFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Boolean>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Boolean>());
        ArchiveFormatterRegistry.Register(new DecimalFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Decimal>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Decimal>());
        ArchiveFormatterRegistry.Register(new GuidFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Guid>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<Guid>());
        ArchiveFormatterRegistry.Register(new DateTimeOffsetFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<DateTimeOffset>());
        ArchiveFormatterRegistry.Register(new NullableFormatter<DateTimeOffset>());
        ArchiveFormatterRegistry.Register(new StringFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<String>());
        ArchiveFormatterRegistry.Register(new VersionFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Version>());
        ArchiveFormatterRegistry.Register(new UriFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Uri>());
        ArchiveFormatterRegistry.Register(new TimeZoneInfoFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<TimeZoneInfo>());
        ArchiveFormatterRegistry.Register(new BigIntegerFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<BigInteger>());
        ArchiveFormatterRegistry.Register(new BitArrayFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<BitArray>());
        ArchiveFormatterRegistry.Register(new StringBuilderFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<StringBuilder>());
        ArchiveFormatterRegistry.Register(new TypeFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<Type>());
        ArchiveFormatterRegistry.Register(new CultureInfoFormatter());
        ArchiveFormatterRegistry.Register(new ArrayFormatter<CultureInfo>());
    }
}
