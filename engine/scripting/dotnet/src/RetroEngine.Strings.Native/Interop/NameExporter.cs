// @file $NameExporter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Strings.Interop;

internal static unsafe partial class NameExporter
{
    private const string LibraryName = "retro_core";

    [LibraryImport(LibraryName, EntryPoint = "retro_name_lookup")]
    public static partial Name Lookup(char* name, int nameLength, FindName findType);

    [LibraryImport(LibraryName, EntryPoint = "retro_name_is_valid")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool IsValid(Name name);

    [LibraryImport(LibraryName, EntryPoint = "retro_name_compare")]
    public static partial int Compare(Name lhs, char* name, int nameLength);

    [LibraryImport(LibraryName, EntryPoint = "retro_name_compare_lexical")]
    public static partial int CompareLexical(NameEntryId lhs, NameEntryId rhs, NameCase nameCase);

    [LibraryImport(LibraryName, EntryPoint = "retro_name_to_string")]
    public static partial int ToString(Name name, char* buffer, int bufferLength);
}
