// // @file TextRegistry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using RetroEngine.Portable.Concurrency;

namespace RetroEngine.Portable.Localization;

internal class TextKeyRegistry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<int, uint> _stringToId = new();
    private readonly Dictionary<uint, string> _idToString = new();
    private uint _nextId = 0;

    private TextKeyRegistry() { }

    public static TextKeyRegistry Instance { get; } = new();

    public uint FindOrAdd(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty)
            return 0;

        var hash = string.GetHashCode(str);

        using (_lock.EnterReadScope())
        {
            if (_stringToId.TryGetValue(hash, out var id))
            {
                return id;
            }
        }

        using (_lock.EnterWriteScope())
        {
            if (_stringToId.TryGetValue(hash, out var id))
            {
                return id;
            }

            var newId = ++_nextId;
            _stringToId.Add(hash, newId);
            _idToString.Add(newId, str.ToString());
            return newId;
        }
    }

    public string GetString(uint id)
    {
        using var scope = _lock.EnterReadScope();
        return _idToString.GetValueOrDefault(id, "");
    }
}
