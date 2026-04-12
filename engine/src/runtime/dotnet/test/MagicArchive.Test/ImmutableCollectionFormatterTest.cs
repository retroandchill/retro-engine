using System.Collections.Immutable;

namespace MagicArchive.Test;

public class ImmutableCollectionFormatterTest
{
    private static T? Convert<T>(T? value)
    {
        var bin = ArchiveSerializer.Serialize(value);
        return ArchiveSerializer.Deserialize<T>(bin);
    }

    // ReSharper disable once UnusedParameter.Local
    private static TAs? ConvertAs<T, TAs>(T? value, TAs dummy)
        where T : TAs
    {
        var bin = ArchiveSerializer.Serialize<TAs>(value);
        return ArchiveSerializer.Deserialize<TAs>(bin);
    }

    [Test]
    public void Collection()
    {
        {
            var value = ImmutableArray.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Assert.That(Convert(value), Is.EquivalentTo(value));
        }
        {
            var value = ImmutableList.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            using var scope = Assert.EnterMultipleScope();
            Assert.That(Convert(value), Is.EquivalentTo(value));
            Assert.That(ConvertAs(value, default(IImmutableList<int>)), Is.EquivalentTo(value));
        }
        {
            var value = ImmutableQueue.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            using var scope = Assert.EnterMultipleScope();
            Assert.That(Convert(value), Is.EquivalentTo(value));
            Assert.That(ConvertAs(value, default(IImmutableQueue<int>)), Is.EquivalentTo(value));
        }
        {
            var value = ImmutableStack.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            using var scope = Assert.EnterMultipleScope();
            Assert.That(Convert(value), Is.EquivalentTo(value));
            Assert.That(ConvertAs(value, default(IImmutableStack<int>)), Is.EquivalentTo(value));
        }
        {
            var value = ImmutableHashSet.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            using var scope = Assert.EnterMultipleScope();
            Assert.That(Convert(value), Is.EquivalentTo(value));
            Assert.That(ConvertAs(value, default(IImmutableSet<int>)), Is.EquivalentTo(value));
        }
        {
            var value = ImmutableSortedSet.Create(1, 10, 100, 2, 4, 530, 647, 73, 8, 42);
            Assert.That(Convert(value), Is.EquivalentTo(value));
        }
    }

    [Test]
    public void Dictionary()
    {
        {
            var value = ImmutableDictionary.CreateRange(
                new KeyValuePair<int, int>[] { new(1, 10), new(2, 20), new(3, 30), new(5, 50) }
            );

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Convert(value), Is.EquivalentTo(value));
                Assert.That(ConvertAs(value, default(IImmutableDictionary<int, int>)), Is.EquivalentTo(value));
            }
        }
        {
            var value = ImmutableSortedDictionary.CreateRange(
                new KeyValuePair<int, int>[] { new(1, 10), new(2, 20), new(3, 30), new(5, 50) }
            );

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Convert(value), Is.EquivalentTo(value));
                Assert.That(ConvertAs(value, default(IImmutableDictionary<int, int>)), Is.EquivalentTo(value));
            }
        }
    }
}
