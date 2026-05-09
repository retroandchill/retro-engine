// // @file NameListView.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.Portable.Strings;

internal class NameListView(Dictionary<Name, List<Name>> backing) : IReadOnlyDictionary<Name, IReadOnlyList<Name>>
{
    public IReadOnlyList<Name> this[Name key] => backing[key];

    public IEnumerable<Name> Keys => backing.Keys;
    public IEnumerable<IReadOnlyList<Name>> Values => backing.Values;

    public int Count => backing.Count;

    public IEnumerator<KeyValuePair<Name, IReadOnlyList<Name>>> GetEnumerator()
    {
        foreach (var (key, value) in backing)
        {
            yield return new KeyValuePair<Name, IReadOnlyList<Name>>(key, value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(Name key)
    {
        return backing.ContainsKey(key);
    }

    public bool TryGetValue(Name key, [MaybeNullWhen(false)] out IReadOnlyList<Name> value)
    {
        if (backing.TryGetValue(key, out var list))
        {
            value = list;
            return true;
        }

        value = null;
        return false;
    }
}
