// // @file EditableTextBlock2.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;

namespace RetroEngine.Editor.Core.Views.Common;

public partial class EditableTextBlock : UserControl
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<EditableTextBlock, string?>(
        nameof(Text),
        defaultBindingMode: BindingMode.TwoWay
    );

    private static readonly StyledProperty<string?> EditedTextProperty = AvaloniaProperty.Register<
        EditableTextBlock,
        string?
    >(nameof(EditedText), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsEditingProperty = AvaloniaProperty.Register<EditableTextBlock, bool>(
        nameof(IsEditing),
        defaultBindingMode: BindingMode.TwoWay
    );

    public EditableTextBlock()
    {
        InitializeComponent();
    }

    internal string? EditedText
    {
        get => GetValue(EditedTextProperty);
        set => SetValue(EditedTextProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    private void BeginEdit()
    {
        EditedText = Text;
        Dispatcher.UIThread.Post(() =>
        {
            Editor.Focus();
            Editor.SelectAll();
        });
    }

    private void CommitEdit()
    {
        IsEditing = false;
        Text = EditedText;
        EditedText = null;
    }

    private void CancelEdit()
    {
        IsEditing = false;
        EditedText = null;
    }

    private void EditorOnKeyDown(object? sender, KeyEventArgs e)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (e.Key)
        {
            case Key.Enter:
                CommitEdit();
                e.Handled = true;
                break;
            case Key.Escape:
                CancelEdit();
                e.Handled = true;
                break;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != IsEditingProperty)
            return;

        var oldValue = change.GetOldValue<bool>();
        var newValue = change.GetNewValue<bool>();
        if (!oldValue && newValue)
        {
            BeginEdit();
        }
    }
}
