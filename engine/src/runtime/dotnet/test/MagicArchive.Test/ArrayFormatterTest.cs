using System.Buffers;
using MagicArchive.Test.Utils;

namespace MagicArchive.Test;

public class ArrayFormatterTest
{
    private static T Convert<T>(T value)
    {
        return ArchiveSerializer.Deserialize<T>(ArchiveSerializer.Serialize(value))!;
    }

    private static readonly int[] Array = [1, 10, 100];

    [Test]
    public void ArrayTes()
    {
        {
            var xs = new[] { 1, 10, 100 };
            Assert.That(Convert(xs), Is.EquivalentTo(xs));
        }
        {
            var xs = new[] { "foo", "bar", "baz" };
            Assert.That(Convert(xs), Is.EquivalentTo(xs));
        }
        {
            var xs = new ArraySegment<int>([1, 10, 100]);
            Assert.That(Convert(xs), Is.EquivalentTo(xs));
        }
        {
            var xs = Array.AsMemory();
            Assert.That(Convert(xs).ToArray(), Is.EquivalentTo(xs.ToArray()));
        }
        {
            var xs = new ReadOnlyMemory<int>([1, 10, 100]);
            Assert.That(Convert(xs).ToArray(), Is.EquivalentTo(xs.ToArray()));
        }

        {
            var xs = ReadOnlySequenceBuilder.Create(
                [1, 2, 3],
                [4, 5, 6, 7, 8],
                // ReSharper disable once UseUtf8StringLiteral
                [9, 10]
            );
            var bin = ArchiveSerializer.Serialize(xs);
            var xs2 = ArchiveSerializer.Deserialize<ReadOnlySequence<byte>>(bin);
            Assert.That(Convert(xs2).ToArray(), Is.EquivalentTo(xs.ToArray()));
        }
    }

    [Test]
    public void CharArrayTest()
    {
        var input = new[] { 'a', 'b', 'c' };
        var bytes = ArchiveSerializer.Serialize(input);
        var output = ArchiveSerializer.Deserialize<char[]>(bytes)!;
        Assert.True(input.SequenceEqual(output));
    }

    [Test]
    [TestCase(100, 100, 10, 5)]
    [TestCase(10, 20, 15, 5)]
    [TestCase(3, 5, 10, 15)]
    public void MultiDimensional(int dataI, int dataJ, int dataK, int dataL)
    {
        var two = new ValueTuple<int, int>[dataI, dataJ];
        var three = new ValueTuple<int, int, int>[dataI, dataJ, dataK];
        var four = new ValueTuple<int, int, int, int>[dataI, dataJ, dataK, dataL];

        for (int i = 0; i < dataI; i++)
        {
            for (int j = 0; j < dataJ; j++)
            {
                two[i, j] = (i, j);
                for (int k = 0; k < dataK; k++)
                {
                    three[i, j, k] = (i, j, k);
                    for (int l = 0; l < dataL; l++)
                    {
                        four[i, j, k, l] = (i, j, k, l);
                    }
                }
            }
        }

        var cTwo = Convert(two);
        var cThree = Convert(three);
        var cFour = Convert(four);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cTwo, Has.Length.EqualTo(two.Length));
            Assert.That(cThree, Has.Length.EqualTo(three.Length));
            Assert.That(cFour, Has.Length.EqualTo(four.Length));
        }

        for (var i = 0; i < dataI; i++)
        {
            for (var j = 0; j < dataJ; j++)
            {
                Assert.That(cTwo[i, j], Is.EqualTo(two[i, j]));
                for (var k = 0; k < dataK; k++)
                {
                    Assert.That(cThree[i, j, k], Is.EqualTo(three[i, j, k]));
                    for (var l = 0; l < dataL; l++)
                    {
                        Assert.That(cFour[i, j, k, l], Is.EqualTo(four[i, j, k, l]));
                    }
                }
            }
        }
    }

    [Test]
    [TestCase(100, 100, 10, 5)]
    [TestCase(10, 20, 15, 5)]
    [TestCase(3, 5, 10, 15)]
    public void MultiDimensional2(int dataI, int dataJ, int dataK, int dataL)
    {
        var two = new ValueTuple<ObjectValue, ObjectValue>[dataI, dataJ];
        var three = new ValueTuple<ObjectValue, ObjectValue, ObjectValue>[dataI, dataJ, dataK];
        var four = new ValueTuple<ObjectValue, ObjectValue, ObjectValue, ObjectValue>[dataI, dataJ, dataK, dataL];

        for (var i = 0; i < dataI; i++)
        {
            for (var j = 0; j < dataJ; j++)
            {
                two[i, j] = (i, j);
                for (var k = 0; k < dataK; k++)
                {
                    three[i, j, k] = (i, j, k);
                    for (var l = 0; l < dataL; l++)
                    {
                        four[i, j, k, l] = (i, j, k, l);
                    }
                }
            }
        }

        var cTwo = Convert(two);
        var cThree = Convert(three);
        var cFour = Convert(four);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cTwo, Has.Length.EqualTo(two.Length));
            Assert.That(cThree, Has.Length.EqualTo(three.Length));
            Assert.That(cFour, Has.Length.EqualTo(four.Length));
        }

        for (var i = 0; i < dataI; i++)
        {
            for (var j = 0; j < dataJ; j++)
            {
                Assert.That(cTwo[i, j], Is.EqualTo(two[i, j]));
                for (var k = 0; k < dataK; k++)
                {
                    Assert.That(cThree[i, j, k], Is.EqualTo(three[i, j, k]));
                    for (var l = 0; l < dataL; l++)
                    {
                        Assert.That(cFour[i, j, k, l], Is.EqualTo(four[i, j, k, l]));
                    }
                }
            }
        }
    }

    [Test]
    public void MultiDimensionalOverwrite()
    {
        var two = new int[3, 3];
        two[0, 0] = 0;
        two[0, 1] = 1;
        two[0, 2] = 2;
        two[1, 0] = 3;
        two[1, 1] = 4;
        two[1, 2] = 5;
        two[2, 0] = 6;
        two[2, 1] = 7;
        two[2, 2] = 8;

        var bin = ArchiveSerializer.Serialize(two);
        var refArray = two;

        System.Array.Clear(two);
        ArchiveSerializer.Deserialize(bin, ref two);
        Assert.That(two, Is.SameAs(refArray));
        using var scope = Assert.EnterMultipleScope();
        Assert.That(two[0, 0], Is.Zero);
        Assert.That(two[0, 1], Is.EqualTo(1));
        Assert.That(two[0, 2], Is.EqualTo(2));
        Assert.That(two[1, 0], Is.EqualTo(3));
        Assert.That(two[1, 1], Is.EqualTo(4));
        Assert.That(two[1, 2], Is.EqualTo(5));
        Assert.That(two[2, 0], Is.EqualTo(6));
        Assert.That(two[2, 1], Is.EqualTo(7));
        Assert.That(two[2, 2], Is.EqualTo(8));
    }
}

[Archivable]
public partial class ObjectValue(int value) : IEquatable<ObjectValue>
{
    public int Value { get; } = value;

    public static implicit operator ObjectValue(int value)
    {
        return new ObjectValue(value);
    }

    public static implicit operator int(ObjectValue value)
    {
        return value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ObjectValue);
    }

    public bool Equals(ObjectValue? other)
    {
        if (other is null)
            return false;
        return Value == other;
    }
}
