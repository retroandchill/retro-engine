// // @file CustomFormatterAttributes.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Compression;
using MagicArchive.Compression;
using MagicArchive.Formatters;

namespace MagicArchive;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class ArchiveCustomFormatterAttribute<TFormatter, T> : Attribute
    where TFormatter : IArchiveFormatter<T>
{
    public abstract TFormatter GetFormatter();
}

public sealed class Utf8StringFormatterAttribute : ArchiveCustomFormatterAttribute<Utf8StringFormatter, string>
{
    public override Utf8StringFormatter GetFormatter() => Utf8StringFormatter.Default;
}

public sealed class Utf16StringFormatterAttribute : ArchiveCustomFormatterAttribute<Utf16StringFormatter, string>
{
    public override Utf16StringFormatter GetFormatter() => Utf16StringFormatter.Default;
}

public sealed class OrdinalIgnoreCaseStringDictionaryFormatter<TValue>
    : ArchiveCustomFormatterAttribute<DictionaryFormatter<string, TValue?>, Dictionary<string, TValue?>>
{
    private static readonly DictionaryFormatter<string, TValue?> _formatter = new(StringComparer.OrdinalIgnoreCase);

    public override DictionaryFormatter<string, TValue?> GetFormatter() => _formatter;
}

public sealed class InternStringFormatterAttribute : ArchiveCustomFormatterAttribute<InternStringFormatter, string>
{
    public override InternStringFormatter GetFormatter() => InternStringFormatter.Default;
}

public sealed class BitPackFormatterAttribute : ArchiveCustomFormatterAttribute<BitPackFormatter, bool[]>
{
    public override BitPackFormatter GetFormatter() => BitPackFormatter.Default;
}

public sealed class BrotliFormatterAttribute(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault,
    int decompressionSizeLimit = BrotliFormatter.DefaultDecompressionSizeLimit
) : ArchiveCustomFormatterAttribute<BrotliFormatter, byte[]>
{
    public override BrotliFormatter GetFormatter()
    {
        return new BrotliFormatter(compressionLevel, window, decompressionSizeLimit);
    }
}

public sealed class BrotliFormatterAttribute<T>(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault
) : ArchiveCustomFormatterAttribute<BrotliFormatter<T>, T>
{
    public override BrotliFormatter<T> GetFormatter()
    {
        return new BrotliFormatter<T>(compressionLevel, window);
    }
}

public sealed class BrotliStringFormatterAttribute(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault,
    int decompressionSizeLimit = BrotliFormatter.DefaultDecompressionSizeLimit
) : ArchiveCustomFormatterAttribute<BrotliStringFormatter, string>
{
    public override BrotliStringFormatter GetFormatter()
    {
        return new BrotliStringFormatter(compressionLevel, window, decompressionSizeLimit);
    }
}

public sealed class MemoryPoolFormatterAttribute<T>
    : ArchiveCustomFormatterAttribute<MemoryPoolFormatter<T>, Memory<T?>>
{
    private static readonly MemoryPoolFormatter<T> _formatter = new();

    public override MemoryPoolFormatter<T> GetFormatter() => _formatter;
}

public sealed class ReadOnlyMemoryPoolFormatterAttribute<T>
    : ArchiveCustomFormatterAttribute<ReadOnlyMemoryPoolFormatter<T>, ReadOnlyMemory<T?>>
{
    private static readonly ReadOnlyMemoryPoolFormatter<T> _formatter = new();

    public override ReadOnlyMemoryPoolFormatter<T> GetFormatter() => _formatter;
}
