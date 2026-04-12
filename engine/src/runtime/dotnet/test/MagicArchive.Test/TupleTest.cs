// // @file TupleTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Test;

public class TupleTest
{
    private static void ConvertEqual<T>(T value)
    {
        Assert.That(ArchiveSerializer.Deserialize<T>(ArchiveSerializer.Serialize(value)), Is.EqualTo(value));
    }

    [Test]
    public void TupleT()
    {
        ConvertEqual(Tuple.Create(1));
        ConvertEqual(Tuple.Create(1, 2));
        ConvertEqual(Tuple.Create(1, 2, 3));
        ConvertEqual(Tuple.Create(1, 2, 3, 4));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
        ConvertEqual(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }

    [Test]
    public void ValueTupleT()
    {
        ConvertEqual(ValueTuple.Create(1));
        ConvertEqual(ValueTuple.Create(1, 2));
        ConvertEqual(ValueTuple.Create(1, 2, 3));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7));
        ConvertEqual(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
    }
}
