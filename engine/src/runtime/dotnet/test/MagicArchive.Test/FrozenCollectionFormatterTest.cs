using System.Collections.Frozen;

namespace MagicArchive.Test;

public class FrozenCollectionFormatterTest
{
    [Test]
    public void FrozenSet()
    {
        var set = new HashSet<int> { 1, 2, 3, 4, 5 };

        var value = set.ToFrozenSet();
        var bin = ArchiveSerializer.Serialize(value);
        var deserializedValue = ArchiveSerializer.Deserialize<FrozenSet<int>>(bin);
        Assert.That(deserializedValue, Is.EquivalentTo(value));
    }

    [Test]
    public void FrozenDictionary()
    {
        var dict = new Dictionary<int, int>()
        {
            { 1, 2 },
            { 3, 4 },
            { 4, 5 },
            { 6, 7 },
            { 8, 9 },
        };
        var value = dict.ToFrozenDictionary();
        var bin = ArchiveSerializer.Serialize(value);
        Assert.That(ArchiveSerializer.Deserialize<FrozenDictionary<int, int>>(bin), Is.EquivalentTo(value));
    }
}
