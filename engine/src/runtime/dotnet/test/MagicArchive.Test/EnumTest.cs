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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Convert(BEnum.B), Is.EqualTo(BEnum.B));
            Assert.That(Convert(NormalEnum.A), Is.EqualTo(NormalEnum.A));
            Assert.That(Convert(NotNotEnum.C), Is.EqualTo(NotNotEnum.C));
        }
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
