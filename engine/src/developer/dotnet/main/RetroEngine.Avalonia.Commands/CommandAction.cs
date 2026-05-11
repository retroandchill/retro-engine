// // @file CommandAction.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Avalonia.Commands;

public enum CheckBoxState : byte
{
    Unchecked,
    Checked,
    Indeterminate,
}

public enum CommandActionRepeatMode
{
    Disabled,
    Enabled,
}

public sealed class CommandAction(
    Action<object?>? execute = null,
    Func<object?, bool>? canExecutePredicate = null,
    Func<object?, CheckBoxState>? getActionCheckState = null,
    Func<object?, bool>? getActionVisibility = null,
    CommandActionRepeatMode repeatMode = CommandActionRepeatMode.Disabled
)
{
    public static readonly CommandAction Empty = new();

    public readonly Func<object?, bool>? GetActionVisibility = getActionVisibility;
    public CommandActionRepeatMode RepeatMode { get; } = repeatMode;

    public bool CanExecute(object? parameter)
    {
        return canExecutePredicate?.Invoke(parameter) ?? true;
    }

    public bool Execute(object? parameter)
    {
        if (execute is null)
            return false;
        execute(parameter);
        return true;
    }

    public CheckBoxState GetActionCheckState(object? parameter)
    {
        return getActionCheckState?.Invoke(parameter) ?? CheckBoxState.Unchecked;
    }

    public bool IsVisible(object? parameter)
    {
        return GetActionVisibility?.Invoke(parameter) ?? true;
    }

    public bool CanRepeat => RepeatMode == CommandActionRepeatMode.Enabled;

    public static Func<object?, CheckBoxState>? ConvertActionCheckStateFunc(Func<object?, bool>? canExecutePredicate)
    {
        if (canExecutePredicate is null)
            return null;

        return obj => canExecutePredicate(obj) ? CheckBoxState.Checked : CheckBoxState.Unchecked;
    }
}
