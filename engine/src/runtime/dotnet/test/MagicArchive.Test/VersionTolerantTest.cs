using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class VersionTolerantTest
{
    private static void ConvertEqual<T>(T value)
    {
        Assert.That(
            ArchiveSerializer.Deserialize<T>(ArchiveSerializer.Serialize(value)),
            Is.EqualTo(value).UsingPropertiesComparer()
        );
    }

    [Test]
    public void Zero()
    {
        var zero = ArchiveSerializer.Deserialize<VersionTolerant0>(ArchiveSerializer.Serialize(new VersionTolerant0()));
        Assert.That(zero, Is.TypeOf<VersionTolerant0>());

        var wrapper = new VTWrapper<VersionTolerant0>() { Values = [1, 10, 100], Versioned = new VersionTolerant0() };

        var v2 = ArchiveSerializer.Deserialize<VTWrapper<VersionTolerant0>>(ArchiveSerializer.Serialize(wrapper));
        Assert.That(v2, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v2.Versioned, Is.TypeOf<VersionTolerant0>());
            Assert.That(v2.Values, Is.EqualTo([1, 10, 100]));
        }
    }

    [Test]
    public void Standard()
    {
        // ConvertEqual(new VersionTolerant0());
        using var scope = Assert.EnterMultipleScope();
        ConvertEqual(new VersionTolerant1());
        ConvertEqual(new VersionTolerant2());
        ConvertEqual(new VersionTolerant3());
        ConvertEqual(new VersionTolerant4());
        ConvertEqual(new VersionTolerant5());
    }

    private static VTWrapper<T> MakeWrapper<T>(T v)
    {
        return new VTWrapper<T> { Versioned = v, Values = [1, 2, 10] };
    }

    private static void CheckArray<T>(VTWrapper<T> value)
    {
        Assert.That(value.Values, Is.EquivalentTo([1, 2, 10]));
    }
#pragma warning disable CS8602
#pragma warning disable CS8604

    [Test]
    public void Version()
    {
        var v0 = new VersionTolerant0();
        var v1 = new VersionTolerant1() { MyProperty1 = 1000 };
        var v2 = new VersionTolerant2() { MyProperty1 = 3000, MyProperty2 = 9999 };
        var v3 = new VersionTolerant3()
        {
            MyProperty1 = 444,
            MyProperty2 = 2452,
            MyProperty3 = 32,
        };
        var v4 = new VersionTolerant4() { MyProperty1 = 99, MyProperty3 = 13 };
        var v5 = new VersionTolerant5() { MyProperty3 = 5000, MyProperty6 = [1, 10, 100] };

        ArchiveSerializer.Serialize(MakeWrapper(v0));
        var bin1 = ArchiveSerializer.Serialize(MakeWrapper(v1));
        ArchiveSerializer.Serialize(MakeWrapper(v2));
        var bin3 = ArchiveSerializer.Serialize(MakeWrapper(v3));
        ArchiveSerializer.Serialize(MakeWrapper(v4));
        ArchiveSerializer.Serialize(MakeWrapper(v5));

        var a = ArchiveSerializer.Deserialize<VTWrapper<VersionTolerant2>>(bin1);
        CheckArray(a);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a.Versioned.MyProperty1, Is.EqualTo(1000));
            Assert.That(a.Versioned.MyProperty2, Is.Zero);
        }

        var b = ArchiveSerializer.Deserialize<VTWrapper<VersionTolerant2>>(bin3);
        CheckArray(b);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.Versioned.MyProperty1, Is.EqualTo(444));
            Assert.That(b.Versioned.MyProperty2, Is.EqualTo(2452));
        }

        var c = ArchiveSerializer.Deserialize<VTWrapper<VersionTolerant4>>(bin3);
        CheckArray(c);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(c.Versioned.MyProperty1, Is.EqualTo(444));
            Assert.That(c.Versioned.MyProperty3, Is.EqualTo(32));
        }

        var d = ArchiveSerializer.Deserialize<VTWrapper<VersionTolerant5>>(bin3);
        CheckArray(d);
        Assert.That(d.Versioned.MyProperty3, Is.EqualTo(32));
    }

    [Test]
    public void More()
    {
        var v3 = new VersionTolerant3
        {
            MyProperty1 = 1000,
            MyProperty2 = 2000,
            MyProperty3 = 3000,
        };
        var v4 = new VersionTolerant4 { MyProperty1 = 4000, MyProperty3 = 5000 };

        var bin3 = ArchiveSerializer.Serialize(v3);
        var bin4 = ArchiveSerializer.Serialize(v4);

        var rV4 = ArchiveSerializer.Deserialize<VersionTolerant4>(bin3);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rV4.MyProperty1, Is.EqualTo(1000));
            Assert.That(rV4.MyProperty3, Is.EqualTo(3000));
        }

        var rV3 = ArchiveSerializer.Deserialize<VersionTolerant3>(bin4);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rV3.MyProperty1, Is.EqualTo(4000));
            Assert.That(rV3.MyProperty2, Is.Zero);
            Assert.That(rV3.MyProperty3, Is.EqualTo(5000));
        }
    }

    [Test]
    public void More2()
    {
        var v1 = new Version1 { Id = 99, Name = "foo" };
        var v2 = new Version2
        {
            Id = 9999,
            FirstName = "a",
            LastName = "b",
        };

        ArchiveSerializer.Serialize(v1);
        var bin2 = ArchiveSerializer.Serialize(v2);

        var r = ArchiveSerializer.Deserialize<Version1>(bin2);
        Assert.That(r.Id, Is.EqualTo(9999));
    }

    [Test]
    public void Version2()
    {
        var v1 = new MoreVersionTolerant1() { MyProperty1 = new Version(10, 20, 4, 6) };
        var v2 = new MoreVersionTolerant2() { MyProperty1 = new Version(4, 23, 3, 99), MyProperty2 = 9999 };
        var v3 = new MoreVersionTolerant3()
        {
            MyProperty1 = new Version(6, 32, 425, 53),
            MyProperty2 = 2452,
            MyProperty3 = 32,
        };
        var v4 = new MoreVersionTolerant4() { MyProperty1 = new Version(11, 12, 13, 14), MyProperty3 = 13 };
        var v5 = new MoreVersionTolerant5() { MyProperty3 = 5000, MyProperty6 = new Version(1, 10, 100) };

        var bin1 = ArchiveSerializer.Serialize(MakeWrapper(v1));
        ArchiveSerializer.Serialize(MakeWrapper(v2));
        var bin3 = ArchiveSerializer.Serialize(MakeWrapper(v3));
        ArchiveSerializer.Serialize(MakeWrapper(v4));
        ArchiveSerializer.Serialize(MakeWrapper(v5));

        var a = ArchiveSerializer.Deserialize<VTWrapper<MoreVersionTolerant2>>(bin1);
        CheckArray(a);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a.Versioned.MyProperty1, Is.EqualTo(new Version(10, 20, 4, 6)));
            Assert.That(a.Versioned.MyProperty2, Is.Zero);
        }

        var b = ArchiveSerializer.Deserialize<VTWrapper<MoreVersionTolerant2>>(bin3);
        CheckArray(b);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.Versioned.MyProperty1, Is.EqualTo(new Version(6, 32, 425, 53)));
            Assert.That(b.Versioned.MyProperty2, Is.EqualTo(2452));
        }

        var c = ArchiveSerializer.Deserialize<VTWrapper<MoreVersionTolerant4>>(bin3);
        CheckArray(c);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(c.Versioned.MyProperty1, Is.EqualTo(new Version(6, 32, 425, 53)));
            Assert.That(c.Versioned.MyProperty3, Is.EqualTo(32));
        }

        var d = ArchiveSerializer.Deserialize<VTWrapper<MoreVersionTolerant5>>(bin3);
        CheckArray(d);
        Assert.That(d.Versioned.MyProperty3, Is.EqualTo(32));
    }

    [Test]
    public void More21()
    {
        var v3 = new MoreVersionTolerant3
        {
            MyProperty1 = new Version(4, 23, 3, 99),
            MyProperty2 = 2000,
            MyProperty3 = 3000,
        };
        var v4 = new MoreVersionTolerant4 { MyProperty1 = new Version(5, 1, 2, 6), MyProperty3 = 5000 };

        var bin3 = ArchiveSerializer.Serialize(v3);
        var bin4 = ArchiveSerializer.Serialize(v4);

        var rV4 = ArchiveSerializer.Deserialize<MoreVersionTolerant4>(bin3);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rV4.MyProperty1, Is.EqualTo(new Version(4, 23, 3, 99)));
            Assert.That(rV4.MyProperty3, Is.EqualTo(3000));
        }

        var rV3 = ArchiveSerializer.Deserialize<MoreVersionTolerant3>(bin4);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rV3.MyProperty1, Is.EqualTo(new Version(5, 1, 2, 6)));
            Assert.That(rV3.MyProperty2, Is.Zero);
            Assert.That(rV3.MyProperty3, Is.EqualTo(5000));
        }
    }

    [Test]
    public void More22()
    {
        var v1 = new MoreVersion1 { Id = new Version(4, 23, 3), Name = "foo" };
        var v2 = new MoreVersion2
        {
            Id = new Version(5, 1, 2, 6),
            FirstName = "a",
            LastName = "b",
        };

        ArchiveSerializer.Serialize(v1);
        var bin2 = ArchiveSerializer.Serialize(v2);

        var r = ArchiveSerializer.Deserialize<MoreVersion1>(bin2);
        Assert.That(r.Id, Is.EqualTo(new Version(5, 1, 2, 6)));
    }
}
