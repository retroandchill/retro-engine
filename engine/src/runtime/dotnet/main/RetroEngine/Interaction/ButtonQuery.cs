// @file ButtonQuery.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace RetroEngine.Interaction;

public readonly ref struct ButtonQuery
{
    public ReadOnlySpan<LogicalKey> LogicalKeys { get; init; }

    public ReadOnlySpan<PhysicalKey> PhysicalKeys { get; init; }

    public ReadOnlySpan<MouseButton> MouseButtons { get; init; }
}
