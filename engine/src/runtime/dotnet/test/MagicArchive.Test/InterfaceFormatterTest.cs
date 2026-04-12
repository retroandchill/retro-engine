using System.Collections.ObjectModel;

namespace MagicArchive.Test;

public class InterfaceFormatterTest
{
    // ReSharper disable once UnusedParameter.Local
    private static void CollectionEqual<T, TSerializeAs>(T value, TSerializeAs? dummy)
        where T : TSerializeAs
        where TSerializeAs : IEnumerable<int>
    {
        var bin = ArchiveSerializer.Serialize<TSerializeAs>(value);
        var value2 = ArchiveSerializer.Deserialize<TSerializeAs>(bin);
        Assert.That(value2, Is.EquivalentTo(value));
    }

    [Test]
    public void EnumerableTest()
    {
        using var scope = Assert.EnterMultipleScope();

        // Array
        {
            var collection = new[] { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(IEnumerable<int>));
        }

        // List
        {
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(IEnumerable<int>));
        }

        // Has Count
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(IEnumerable<int>));
        }

        // No Count
        {
            var collection = Iterate(1, 5);
            CollectionEqual(collection, default(IEnumerable<int>));
        }
    }

    [Test]
    public void CollectionTest()
    {
        using var scope = Assert.EnterMultipleScope();

        // Array
        {
            var collection = new[] { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(ICollection<int>));
        }

        // List
        {
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(ICollection<int>));
        }

        // Has Count
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection, default(ICollection<int>));
        }
    }

    [Test]
    public void Collections()
    {
        using var scope = Assert.EnterMultipleScope();
        var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
        CollectionEqual(collection, default(IReadOnlyCollection<int>));
        CollectionEqual(collection, default(IList<int>));
        CollectionEqual(collection, default(IReadOnlyList<int>));
    }

    [Test]
    public void Dictionaries()
    {
        using var scope = Assert.EnterMultipleScope();
        var collection = new Dictionary<int, int>
        {
            { 1, 2 },
            { 3, 4 },
            { 5, 6 },
        };

        {
            var bin = ArchiveSerializer.Serialize<IDictionary<int, int>>(collection);
            Assert.That(ArchiveSerializer.Deserialize<IDictionary<int, int>>(bin), Is.EquivalentTo(collection));
        }
        {
            var bin = ArchiveSerializer.Serialize<IReadOnlyDictionary<int, int>>(collection);
            Assert.That(ArchiveSerializer.Deserialize<IReadOnlyDictionary<int, int>>(bin), Is.EquivalentTo(collection));
        }
    }

    [Test]
    public void Lookup()
    {
        var seq = new[] { (1, 2), (1, 100), (3, 42), (45, 30), (3, 10) };

        var lookup = seq.ToLookup(x => x.Item1, x => x.Item2);
        {
            var bin = ArchiveSerializer.Serialize(lookup);
            Assert.That(ArchiveSerializer.Deserialize<ILookup<int, int>>(bin), Is.EquivalentTo(lookup));
        }

        var grouping = lookup.First(x => x.Key == 3);
        {
            var bin = ArchiveSerializer.Serialize(grouping);
            var g2 = ArchiveSerializer.Deserialize<IGrouping<int, int>>(bin);
            Assert.That(g2, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(g2.Key, Is.EqualTo(grouping.Key));
                Assert.That(g2.AsEnumerable(), Is.EquivalentTo(grouping.AsEnumerable()));
            }
        }

        var emptyLookup = Array.Empty<int>().ToLookup(x => x, x => x);
        {
            var bin = ArchiveSerializer.Serialize(emptyLookup);
            var deserialized = ArchiveSerializer.Deserialize<ILookup<int, int>>(bin);
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized[0], Is.Empty);
        }
    }

    [Test]
    public void Sets()
    {
        using var scope = Assert.EnterMultipleScope();
        var collection = new HashSet<int> { 1, 10, 100, 1000, 10000, 20, 200 };

        {
            var bin = ArchiveSerializer.Serialize<ISet<int>>(collection);
            Assert.That(ArchiveSerializer.Deserialize<ISet<int>>(bin), Is.EquivalentTo(collection));
        }
        {
            var bin = ArchiveSerializer.Serialize<IReadOnlySet<int>>(collection);
            Assert.That(ArchiveSerializer.Deserialize<IReadOnlySet<int>>(bin), Is.EquivalentTo(collection));
        }
    }

    private static IEnumerable<int> Iterate(int from, int to)
    {
        for (var i = from; i <= to; i++)
        {
            yield return i;
        }
    }
}
