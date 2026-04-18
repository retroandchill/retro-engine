// // @file ArrayTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class ArrayTest
{
    [Test]
    public void StandardArraySerialization()
    {
        var checker = new ArrayCheck
        {
            Array1 = [1, 10, -1000, int.MaxValue, int.MinValue],
            Array2 = [300, null, -99999, null, 234242],
            Array3 = ["foo", "bar", "baz", "", "t"],
            Array4 = ["zzzz", null, "", "あいうえお"],
        };

        var bin = ArchiveSerializer.Serialize(checker);
        var v2 = ArchiveSerializer.Deserialize<ArrayCheck>(bin);
        Assert.That(v2, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v2.Array1, Is.EquivalentTo(checker.Array1));
            Assert.That(v2.Array2, Is.EquivalentTo(checker.Array2));
            Assert.That(v2.Array3, Is.EquivalentTo(checker.Array3));
            Assert.That(v2.Array4, Is.EquivalentTo(checker.Array4));
        }
    }

    [Test]
    public void TestOptimizedCheck()
    {
        var checker = new ArrayOptimizeCheck
        {
            Array1 = [new StandardTypeTwo { One = 9, Two = 2 }, new StandardTypeTwo { One = 999, Two = 444 }],
            List1 = [new StandardTypeTwo { One = 93, Two = 12 }, new StandardTypeTwo { One = 9499, Two = 45344 }],
        };

        var bin = ArchiveSerializer.Serialize(checker);
        var v2 = ArchiveSerializer.Deserialize<ArrayOptimizeCheck>(bin);
        using (Assert.EnterMultipleScope())
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.That(v2.Array1[0].One, Is.EqualTo(checker.Array1[0].One));
            Assert.That(v2.Array1[0].Two, Is.EqualTo(checker.Array1[0].Two));
            Assert.That(v2.Array1[1].One, Is.EqualTo(checker.Array1[1].One));
            Assert.That(v2.Array1[1].Two, Is.EqualTo(checker.Array1[1].Two));

            Assert.That(v2.List1[0].One, Is.EqualTo(checker.List1[0].One));
            Assert.That(v2.List1[0].Two, Is.EqualTo(checker.List1[0].Two));
            Assert.That(v2.List1[1].One, Is.EqualTo(checker.List1[1].One));
            Assert.That(v2.List1[1].Two, Is.EqualTo(checker.List1[1].Two));
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [Test]
    public void BoolArray()
    {
        var rand = new Random();
        for (var i = 0; i < 1000; i++)
        {
            var data = Enumerable.Range(0, i).Select(_ => rand.Next(0, 2) == 0).ToArray();
            var value = new BitPackSingleData { Data = data };

            var bin = ArchiveSerializer.Serialize(value);
            var value2 = ArchiveSerializer.Deserialize<BitPackSingleData>(bin);

            Assert.That(value2, Is.Not.Null);
            Assert.That(value2.Data, Is.EquivalentTo(data));
        }
        for (var i = 0; i < 1000; i++)
        {
            var data = Enumerable.Range(0, i).Select(_ => rand.Next(0, 2) == 0).ToArray();
            var value = new BitPackData { Data = data, AAA = i };

            var bin = ArchiveSerializer.Serialize(value);
            var value2 = ArchiveSerializer.Deserialize<BitPackData>(bin);

            Assert.That(value2, Is.Not.Null);

            using var scope = Assert.EnterMultipleScope();
            Assert.That(value2.Data, Is.EquivalentTo(data));
            Assert.That(value2.AAA, Is.EqualTo(i));
        }
    }
}
