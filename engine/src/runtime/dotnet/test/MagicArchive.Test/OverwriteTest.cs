// // @file OverwriteTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class OverwriteTest
{
    [Test]
    public void CanOverwriteAnExistingClassType()
    {
        var write = new Overwrite()
        {
            MyProperty1 = 10,
            MyProperty2 = 20,
            MyProperty3 = "foo",
            MyProperty4 = "bar",
        };

        var bin = ArchiveSerializer.Serialize(write);
        write.MyProperty1 = 99;
        write.MyProperty2 = 9999;
        write.MyProperty3 = "hoahoahoa";
        write.MyProperty4 = "kukukukuku";

        var original = write;
        ArchiveSerializer.Deserialize(bin, ref write);
        Assert.That(write, Is.Not.Null);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(write.MyProperty1, Is.EqualTo(10));
        Assert.That(write.MyProperty2, Is.EqualTo(20));
        Assert.That(write.MyProperty3, Is.EqualTo("foo"));
        Assert.That(write.MyProperty4, Is.EqualTo("bar"));
        Assert.That(original, Is.SameAs(write));
    }

    [Test]
    public void CanOverwriteAnExistingStructType()
    {
        var write = new Overwrite2()
        {
            MyProperty1 = 10,
            MyProperty2 = 20,
            MyProperty3 = "foo",
            MyProperty4 = "bar",
        };

        var bin = ArchiveSerializer.Serialize(write);

        write.MyProperty1 = 99;
        write.MyProperty2 = 9999;
        write.MyProperty3 = "hoahoahoa";
        write.MyProperty4 = "kukukukuku";

        ArchiveSerializer.Deserialize(bin, ref write);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(write.MyProperty1, Is.EqualTo(10));
        Assert.That(write.MyProperty2, Is.EqualTo(20));
        Assert.That(write.MyProperty3, Is.EqualTo("foo"));
        Assert.That(write.MyProperty4, Is.EqualTo("bar"));
    }

    [Test]
    public void TypesWithAnExplicitConstructorShouldAlwaysCreateANewInstance()
    {
        var write = new Overwrite3(10, 20) { MyProperty3 = "foo", MyProperty4 = "bar" };

        var bin = ArchiveSerializer.Serialize(write);

        write.MyProperty1 = 99;
        write.MyProperty2 = 9999;
        write.MyProperty3 = "hoahoahoa";
        write.MyProperty4 = "kukukukuku";

        var original = write;
        ArchiveSerializer.Deserialize(bin, ref write);

        Assert.That(write, Is.Not.Null);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(write.MyProperty1, Is.EqualTo(10));
        Assert.That(write.MyProperty2, Is.EqualTo(20));
        Assert.That(write.MyProperty3, Is.EqualTo("foo"));
        Assert.That(write.MyProperty4, Is.EqualTo("bar"));
        Assert.That(original, Is.Not.SameAs(write));
    }

    [Test]
    public void ComplexOverwritingScenario()
    {
        var write = new Overwrite4()
        {
            MyProperty1 = 4444,
            MyProperty2 = new Overwrite()
            {
                MyProperty1 = 10,
                MyProperty2 = 20,
                MyProperty3 = "foo",
                MyProperty4 = "bar",
            },
            MyProperty3 = [1, 5, 9],
        };

        var bin = ArchiveSerializer.Serialize(write);

        write.MyProperty1 = 5555;
        write.MyProperty2.MyProperty1 = 99;
        write.MyProperty2.MyProperty2 = 9999;
        write.MyProperty2.MyProperty3 = "hoahoahoa";
        write.MyProperty2.MyProperty4 = "kukukukuku";
        write.MyProperty3.Add(99999);

        var original = write;
        ArchiveSerializer.Deserialize(bin, ref write);

        Assert.That(write, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(write.MyProperty2, Is.Not.Null);
            Assert.That(write.MyProperty3, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(write.MyProperty1, Is.EqualTo(4444));
            Assert.That(write.MyProperty2.MyProperty1, Is.EqualTo(10));
            Assert.That(write.MyProperty2.MyProperty2, Is.EqualTo(20));
            Assert.That(write.MyProperty2.MyProperty3, Is.EqualTo("foo"));
            Assert.That(write.MyProperty2.MyProperty4, Is.EqualTo("bar"));
            Assert.That(write.MyProperty3, Is.EquivalentTo([1, 5, 9]));

            Assert.That(original, Is.SameAs(write));
            Assert.That(original.MyProperty2, Is.SameAs(write.MyProperty2));
            Assert.That(original.MyProperty3, Is.SameAs(write.MyProperty3));
        }
    }
}
