using System.Collections;
using System.Globalization;
using System.Text;

namespace MagicArchive.Test;

public class WellKnownFormattersTest
{
    private static T Convert<T>(T value)
    {
        return ArchiveSerializer.Deserialize<T>(ArchiveSerializer.Serialize(value))!;
    }

    private static void ConvertEqual<T>(T value)
    {
        Assert.That(Convert(value), Is.EqualTo(value));
    }

    [Test]
    public void Genenrics()
    {
        ConvertEqual(new KeyValuePair<int, string>(100, "hoge"));
        Assert.That(Convert(new Lazy<int>(100)).Value, Is.EqualTo(100));
    }

    [Test]
    public void Nullable()
    {
        var converted = Convert(new Sonota?(new Sonota { MyProperty = "9" })!);
        Assert.That(converted, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(converted.Value.MyProperty, Is.EqualTo("9"));
            Assert.That(Convert(default(Sonota?)), Is.Null);
        }
    }

    [Test]
    public void Others()
    {
        ConvertEqual(new Version(1, 3, 4, 5));
        ConvertEqual(new Uri("http://hoehoge.com/huu?q=takotako"));
        ConvertEqual(TimeZoneInfo.Utc);
        var sb = new StringBuilder(new string('a', 99999));
        Assert.That(Convert(sb).ToString(), Is.EqualTo(sb.ToString()));
        ConvertEqual(typeof(WellKnownFormattersTest));
        var bitArray = new BitArray(Enumerable.Range(1, 1000).Select(x => x % 3 == 0).ToArray());
        Assert.That(Convert(bitArray).OfType<bool>().ToArray(), Is.EqualTo(bitArray.OfType<bool>().ToArray()));
        ConvertEqual(CultureInfo.InvariantCulture);
        ConvertEqual(CultureInfo.GetCultureInfo("ja"));
        ConvertEqual(CultureInfo.GetCultureInfo("ja-JP"));
        ConvertEqual(CultureInfo.GetCultureInfo("en"));
    }
}

[Archivable]
public partial struct Sonota
{
    public string MyProperty { get; set; }
}
