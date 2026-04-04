// // @file StringTableRegistry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Localization.StringTables;

public sealed class StringTableRegistry
{
    private StringTableRegistry() { }

    public StringTableRegistry Instance { get; } = new();

    public void RegisterStringTable(Name tableId, StringTable table)
    {
        throw new NotImplementedException();
    }

    public void UnregisterStringTable(Name tableId)
    {
        throw new NotImplementedException();
    }

    public StringTable? FindStringTable(Name tableId)
    {
        throw new NotImplementedException();
    }

    public void EnumerateStringTables(Action<Name, StringTable> action)
    {
        throw new NotImplementedException();
    }
}
