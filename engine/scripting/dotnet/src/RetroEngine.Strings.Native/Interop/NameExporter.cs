// @file $NameExporter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Strings.Interop;

[BindExport("retro")]
internal static unsafe partial class NameExporter
{
    public static partial Name Lookup([CppType(IsConst = true)] char* name, int length, FindName findType);

    public static partial NativeBool IsValid(Name name);

    public static partial NativeBool Equals(Name lhs, [CppType(IsConst = true)] char* rhs, int length);

    public static partial int ToString(Name name, char* buffer, int bufferSize);
}
