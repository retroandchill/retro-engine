using MagicArchive.Formatters;
using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class UnionTest
{
    [Test]
    public void Foo()
    {
        {
            var one = new AForOne { BaseValue = 10, MyProperty = 99 };
            var two = new AForTwo { BaseValue = 99, MyProperty = 10000 };

            var bin1 = ArchiveSerializer.Serialize((IForExternalUnion)one);
            var bin2 = ArchiveSerializer.Serialize((IForExternalUnion)two);

            var one2 = ArchiveSerializer.Deserialize<IForExternalUnion>(bin1);
            var two2 = ArchiveSerializer.Deserialize<IForExternalUnion>(bin2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(one2, Is.TypeOf<AForOne>());
                Assert.That(two2, Is.TypeOf<AForTwo>());
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(one2, Is.EqualTo(one).UsingPropertiesComparer());
                Assert.That(two2, Is.EqualTo(two).UsingPropertiesComparer());
            }
        }
        {
            var one = new BForOne<DateTime> { NoValue = DateTime.Now, MyProperty = 99 };
            var two = new BForTwo<string> { NoValue = "aaaa", MyProperty = 10000 };

            var bin1 = ArchiveSerializer.Serialize((IGenericsUnion<DateTime>)one);
            var bin2 = ArchiveSerializer.Serialize((IGenericsUnion<string>)two);

            var one2 = ArchiveSerializer.Deserialize<IGenericsUnion<DateTime>>(bin1);
            var two2 = ArchiveSerializer.Deserialize<IGenericsUnion<string>>(bin2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(one2, Is.TypeOf<BForOne<DateTime>>());
                Assert.That(two2, Is.TypeOf<BForTwo<string>>());
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(one2, Is.EqualTo(one).UsingPropertiesComparer());
                Assert.That(two2, Is.EqualTo(two).UsingPropertiesComparer());
            }
        }
    }

    [Test]
    public void Dynamic()
    {
        var f = new DynamicUnionFormatter<IDynamicBase>((0, typeof(Gen1)), (1, typeof(Gen2)));

        ArchiveFormatterRegistry.Register(f);

        var one = new Gen1() { MyProperty = 999 };
        var two = new Gen2() { MyProperty = "aabbbC" };

        var bin1 = ArchiveSerializer.Serialize<IDynamicBase>(one);
        var bin2 = ArchiveSerializer.Serialize<IDynamicBase>(two);

        var d1 = ArchiveSerializer.Deserialize<IDynamicBase>(bin1);
        var d2 = ArchiveSerializer.Deserialize<IDynamicBase>(bin2);

#pragma warning disable NUnit2045
        Assert.That(d1, Is.TypeOf<Gen1>());
        Assert.That(((Gen1)d1).MyProperty, Is.EqualTo(999));
        Assert.That(d2, Is.TypeOf<Gen2>());
        Assert.That(((Gen2)d2).MyProperty, Is.EqualTo("aabbbC"));
#pragma warning restore NUnit2045
    }
}

[Archivable(GenerateType.NoGenerate)]
public partial class IDynamicBase { }

[Archivable]
public partial class Gen1 : IDynamicBase
{
    public int MyProperty { get; set; }
}

[Archivable]
public partial class Gen2 : IDynamicBase
{
    public string? MyProperty { get; set; }
}
