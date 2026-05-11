// // @file CommandList.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace RetroEngine.Avalonia.Commands;

public class CommandList
{
    private readonly Dictionary<CommandInfo, CommandAction> _commands = new();

    public void MapAction(
        CommandInfo command,
        Action<object?> action,
        Func<object?, bool>? canExecute = null,
        Func<object?, bool>? isActionChecked = null,
        Func<object?, bool>? isActionVisible = null,
        CommandActionRepeatMode repeatMode = CommandActionRepeatMode.Disabled
    )
    {
        MapAction(
            command,
            new CommandAction(
                action,
                canExecute,
                CommandAction.ConvertActionCheckStateFunc(isActionChecked),
                isActionVisible,
                repeatMode
            )
        );
    }

    [OverloadResolutionPriority(int.MaxValue)]
    public void MapAction(
        CommandInfo command,
        Action<object?> action,
        Func<object?, bool>? canExecute = null,
        Func<object?, CheckBoxState>? getActionCheckState = null,
        Func<object?, bool>? isActionVisible = null,
        CommandActionRepeatMode repeatMode = CommandActionRepeatMode.Disabled
    )
    {
        MapAction(command, new CommandAction(action, canExecute, getActionCheckState, isActionVisible, repeatMode));
    }

    public void MapAction(CommandInfo command, CommandAction action)
    {
        _commands.Add(command, action);
    }

    public void UnmapAction(CommandInfo command)
    {
        _commands.Remove(command);
    }

    public bool IsActionMapped(CommandInfo command)
    {
        return _commands.ContainsKey(command);
    }

    public virtual bool ExecuteAction(CommandInfo command, object? parameter)
    {
        var action = _commands.GetValueOrDefault(command);
        return action is not null && action.Execute(parameter);
    }

    public bool CanExecuteAction(CommandInfo command, object? parameter)
    {
        var action = _commands.GetValueOrDefault(command);
        return action is null || action.CanExecute(parameter);
    }

    public bool TryExecuteAction(CommandInfo command, object? parameter)
    {
        return CanExecuteAction(command, parameter) && ExecuteAction(command, parameter);
    }

    public bool IsVisible(CommandInfo command, object? parameter)
    {
        var action = _commands.GetValueOrDefault(command);
        return action is null || action.IsVisible(parameter);
    }

    public CheckBoxState GetActionCheckState(CommandInfo command, object? parameter)
    {
        var action = _commands.GetValueOrDefault(command);
        return action?.GetActionCheckState(parameter) ?? CheckBoxState.Unchecked;
    }

    public CommandAction? GetAction(CommandInfo command)
    {
        return _commands.GetValueOrDefault(command);
    }
}
