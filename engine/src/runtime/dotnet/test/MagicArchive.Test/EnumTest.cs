using FluentAssertions;

namespace MagicArchive.Test;

public class EnumTest
{
    private static T Convert<T>(T value)
    {
        return ArchiveSerializer.Deserialize<T>(ArchiveSerializer.Serialize(value))!;
    }

    [Test]
    public void EnumsAreBlittable()
    {
        Convert(BEnum.B).Should().Be(BEnum.B);
        Convert(NormalEnum.A).Should().Be(NormalEnum.A);
        Convert(NotNotEnum.C).Should().Be(NotNotEnum.C);
    }

    private enum BEnum : byte
    {
        A,
        B,
        C,
    }

    private enum NormalEnum
    {
        A,
        B,
        C,
    }

    private enum NotNotEnum : long
    {
        A,
        B,
        C,
    }
}
