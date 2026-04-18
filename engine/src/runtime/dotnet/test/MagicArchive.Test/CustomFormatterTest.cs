// // @file CustomFormatterTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class CustomFormatterTest
{
    [Test]
    public void NormalFormatters()
    {
        var value = new CustomFormatterCheck
        {
            NoMarkField = "aaaa",
            Field1 = "aaaa",
            Prop1 = "bbbb",
            NoMarkProp = "bbbb",
            PropDict = new Dictionary<string, int> { { "ZooM", 999 }, { "DdddN", 10000 } },
            FieldDict = new Dictionary<string, string> { { "hOGe", "hugahuga" }, { "HagE", "nanonano" } },
        };

        var bin1 = ArchiveSerializer.Serialize(value, ArchiveSerializerOptions.Utf8);
        var bin2 = ArchiveSerializer.Serialize(value, ArchiveSerializerOptions.Utf16);

        var v1 = ArchiveSerializer.Deserialize<CustomFormatterCheck>(bin1);
        var v2 = ArchiveSerializer.Deserialize<CustomFormatterCheck>(bin2);

        Assert.That(v1, Is.Not.Null);
        Assert.That(v1.PropDict, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v1.PropDict["zoom"], Is.EqualTo(999));
            Assert.That(v1.PropDict["DDDDN"], Is.EqualTo(10000));
            Assert.That(v1.FieldDict, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(v1.FieldDict["HOGE"], Is.EqualTo("hugahuga"));
            Assert.That(v1.FieldDict["hage"], Is.EqualTo("nanonano"));
        }

        Assert.That(v2, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v1.Prop1, Is.EqualTo(value.Prop1));
            Assert.That(v1.Field1, Is.EqualTo(value.Field1));
            Assert.That(v2.Prop1, Is.EqualTo(value.Prop1));
            Assert.That(v2.Field1, Is.EqualTo(value.Field1));
        }
    }

    private static readonly int[] Pool1 = [1, 10, 100, 100, 10000];
    private static readonly string[] Pool3 = ["aaa", "bbb", "DDDDDDDDDDDDDD", "あいうえおか"];

    [Test]
    public void PoolFormatters()
    {
        var forPool = new MemoryPoolModel(
            Pool1,
            "あいうえおかきくけこさしすせそ"u8.ToArray(),
            Pool3,
            new[]
            {
                new StdData { MyProperty = 10 },
                new StdData { MyProperty = 99 },
            }
        );

        var bin = ArchiveSerializer.Serialize(forPool);
        using var v2 = ArchiveSerializer.Deserialize<MemoryPoolModel>(bin);
        ArraySegment<int> seg1;
        ArraySegment<byte> seg2;
        ArraySegment<string> seg3;
        ArraySegment<StdData> seg4;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v2, Is.Not.Null);

            Assert.That(MemoryMarshal.TryGetArray(v2.Pool1, out seg1), Is.True);
            Assert.That(MemoryMarshal.TryGetArray(v2.Pool2, out seg2), Is.True);
            Assert.That(MemoryMarshal.TryGetArray(v2.Pool3, out seg3), Is.True);
            Assert.That(MemoryMarshal.TryGetArray(v2.Pool4, out seg4), Is.True);

            Assert.That(seg1.Array, Is.Not.Null);
            Assert.That(seg2.Array, Is.Not.Null);
            Assert.That(seg3.Array, Is.Not.Null);
            Assert.That(seg4.Array, Is.Not.Null);
        }

        using var scope = Assert.EnterMultipleScope();
        Assert.That(seg1.Array, Has.Length.EqualTo(PoolSize(forPool.Pool1.Length)));
        Assert.That(seg2.Array, Has.Length.EqualTo(PoolSize(forPool.Pool2.Length)));
        Assert.That(seg3.Array, Has.Length.EqualTo(PoolSize(forPool.Pool3.Length)));
        Assert.That(seg4.Array, Has.Length.EqualTo(PoolSize(forPool.Pool4.Length)));

        Assert.That(v2.Pool1.ToArray(), Is.EquivalentTo(forPool.Pool1.ToArray()));
        Assert.That(v2.Pool2.ToArray(), Is.EquivalentTo(forPool.Pool2.ToArray()));
        Assert.That(v2.Pool3.ToArray(), Is.EquivalentTo(forPool.Pool3.ToArray()));

        Assert.That(v2.Pool4.Span[0].MyProperty, Is.EqualTo(forPool.Pool4.Span[0].MyProperty));
        Assert.That(v2.Pool4.Span[1].MyProperty, Is.EqualTo(forPool.Pool4.Span[1].MyProperty));
    }

    private static int PoolSize(int size)
    {
        size = BitOperations.Log2((uint)size - 1 | 15) - 3;
        return 16 << size;
    }
}
