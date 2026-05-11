// // @file CommandInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Input;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Avalonia.Commands;

public sealed class CommandInfo
{
    public Name CommandName { get; }

    public Text Label { get; }

    public Text Description { get; init; }

    public KeyGesture? Gesture { get; init; }
    public UserInterfaceActionType ActionType { get; init; } = UserInterfaceActionType.Button;

    public CommandInfo(Name commandName, Text label)
    {
        if (label.IsEmpty)
        {
            throw new ArgumentException("Label cannot be empty", nameof(label));
        }

        CommandName = commandName;
        Label = label;
    }
}
