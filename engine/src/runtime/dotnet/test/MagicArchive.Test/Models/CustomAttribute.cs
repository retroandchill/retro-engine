// // @file CustomAttribute.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicArchive.Test.Models;

[Archivable]
public partial class CustomFormatterCheck
{
    public string? NoMarkField;
    public string? NoMarkProp { get; set; }

    [Utf8StringFormatter]
    public string? Field1;

    [Utf16StringFormatter]
    public string? Prop1 { get; set; }

    [OrdinalIgnoreCaseStringDictionaryFormatter<int>]
    public Dictionary<string, int>? PropDict { get; set; }

    [OrdinalIgnoreCaseStringDictionaryFormatter<string>]
    public Dictionary<string, string>? FieldDict;
}

[Archivable]
public sealed partial class MemoryPoolModel(
    Memory<int> pool1,
    Memory<byte> pool2,
    ReadOnlyMemory<string> pool3,
    ReadOnlyMemory<StdData> pool4
) : IDisposable
{
    [MemoryPoolFormatter<int>]
    public Memory<int> Pool1 { get; private set; } = pool1;

    [MemoryPoolFormatter<byte>]
    public Memory<byte> Pool2 { get; private set; } = pool2;

    [ReadOnlyMemoryPoolFormatter<string>]
    public ReadOnlyMemory<string> Pool3 { get; private set; } = pool3;

    [ReadOnlyMemoryPoolFormatter<StdData>]
    public ReadOnlyMemory<StdData> Pool4 { get; private set; } = pool4;

    private bool _usePool;

    [ArchivableOnDeserialized]
    private void OnDeserialized()
    {
        _usePool = true;
    }

    private static void Return<T>(Memory<T> memory) => Return((ReadOnlyMemory<T>)memory);

    private static void Return<T>(ReadOnlyMemory<T> memory)
    {
        if (MemoryMarshal.TryGetArray(memory, out var segment) && segment.Array is { Length: > 0 })
        {
            ArrayPool<T>.Shared.Return(segment.Array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }

    public void Dispose()
    {
        if (!_usePool)
            return;

        Return(Pool1);
        Pool1 = default;
        Return(Pool2);
        Pool2 = default;
        Return(Pool3);
        Pool3 = default;
        Return(Pool4);
        Pool4 = default;
    }
}

[Archivable]
public partial class StdData
{
    public int MyProperty { get; set; }
}
