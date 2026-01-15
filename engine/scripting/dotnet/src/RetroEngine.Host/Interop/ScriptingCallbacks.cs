// // @file ScriptingCallbacks.cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Host.Interop;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct ScriptingCallbacks
{
    public required delegate* unmanaged[Cdecl]<char*, int, char*, int, int> Start { get; init; }
    public required delegate* unmanaged[Cdecl]<float, int, int> Tick { get; init; }
    public required delegate* unmanaged[Cdecl]<void> Exit { get; init; }
}
