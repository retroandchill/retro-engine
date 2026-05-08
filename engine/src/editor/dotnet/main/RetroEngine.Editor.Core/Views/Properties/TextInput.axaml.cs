// // @file TextPropertyInput.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PropertyGenerator.Avalonia;
using RetroEngine.Portable.Localization;
using RetroEngine.Utilities.Async;
using Serilog;

namespace RetroEngine.Editor.Core.Views.Properties;

public partial class TextInput : UserControl
{
    [GeneratedStyledProperty]
    public partial Text Text { get; set; }

    public event Action<Text>? TextChanged;

    private readonly Debouncer _inputDebouncer = new();

    [GeneratedDirectProperty]
    public partial bool Localized { get; set; } = true;

    public TextInput()
    {
        InitializeComponent();
    }

    partial void OnTextPropertyChanged(Text newValue)
    {
        TextBox.Text = newValue.ToString();
        Localized = !newValue.IsCultureInvariant;
        TextChanged?.Invoke(newValue);
    }

    partial void OnLocalizedPropertyChanged(bool newValue)
    {
        LocalizedOnIcon.IsVisible = newValue;
        LocalizedOffIcon.IsVisible = !newValue;
    }

    private void InputTextChanged(object? sender, TextChangedEventArgs e)
    {
        _inputDebouncer.Debounce(UpdateText, TimeSpan.FromMilliseconds(100));
    }

    private void UpdateText()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var value = TextBox.Text ?? "";
            if (Text.IsCultureInvariant)
            {
                Text = Text.AsCultureInvariant(value);
            }
            else
            {
                var (@namespace, key) = Text.TextData.History.TextId;
                Text = new Text(value, @namespace, key, Text.Flags);
            }
        });
    }

    private void LocalizedCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        Localized = checkBox.IsChecked is true;
    }
}
