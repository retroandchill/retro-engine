// // @file CustomCollectionTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Test;

[Archivable(GenerateType.Collection)]
public partial class ListInt : List<int>;

[Archivable(GenerateType.Collection)]
public partial class SetInt : HashSet<int>;

[Archivable(GenerateType.Collection)]
public partial class DictionaryIntInt : Dictionary<int, int>;

[Archivable(GenerateType.Collection)]
public partial class ListGenerics<T> : List<T>;

[Archivable(GenerateType.Collection)]
public partial class SetGenerics<T> : HashSet<T>;

[Archivable(GenerateType.Collection)]
public partial class DictionaryGenerics<TK, TV> : Dictionary<TK, TV>
    where TK : notnull;

public class CustomCollectionTest
{
    private static T Convert<T>(T value)
    {
        var bin = ArchiveSerializer.Serialize(value);
        return ArchiveSerializer.Deserialize<T>(bin)!;
    }

    [Test]
    public void NonGenerics()
    {
        var l = new ListInt { 1, 2, 3, 4, 5, 6, 7 };
        Assert.That(Convert(l), Is.EquivalentTo(l));

        var s = new SetInt { 1, 10, 20, 30 };
        Assert.That(Convert(s), Is.EquivalentTo(s));

        var d = new DictionaryIntInt
        {
            { 1, 10 },
            { 2, 30 },
            { 65, 2342 },
        };
        Assert.That(Convert(d), Is.EquivalentTo(d));
    }

    [Test]
    public void Generics()
    {
        var l = new ListGenerics<int> { 1, 2, 3, 4, 5, 6, 7 };
        Assert.That(Convert(l), Is.EquivalentTo(l));

        var s = new SetGenerics<int> { 1, 10, 20, 30 };
        Assert.That(Convert(s), Is.EquivalentTo(s));

        var d = new DictionaryGenerics<int, int>
        {
            { 1, 10 },
            { 2, 30 },
            { 65, 2342 },
        };
        Assert.That(Convert(d), Is.EquivalentTo(d));
    }
}
