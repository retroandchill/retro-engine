// // @file ListBenchmark.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using BenchmarkDotNet.Attributes;

namespace MagicArchive.Benchmark;

[MemoryDiagnoser]
public class ArrayBenchmark
{
    private readonly int[] _numbers = Enumerable.Range(1, 1000).ToArray();

    [Benchmark]
    public int SerializeToJson()
    {
        var serialized = JsonSerializer.Serialize(_numbers);
        return serialized.Length;
    }

    [Benchmark]
    public int SerializeToBinary()
    {
        var serialized = ArchiveSerializer.Serialize(_numbers);
        return serialized.Length;
    }
}
