// // @file TextPropertyInput.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Threading;
using PropertyGenerator.Avalonia;
using RetroEngine.Editor.Core.ViewModels.Properties;
using RetroEngine.Portable.Localization;
using RetroEngine.Utilities.Async;
using Serilog;

namespace RetroEngine.Editor.Core.Views.Properties;

public partial class TextInput : UserControl
{
    [GeneratedStyledProperty]
    public partial Text Text { get; set; }
    private readonly TextInputViewModel _viewModel = new();

    private readonly Debouncer _debouncer = new();

    public event Action<Text>? TextChanged;

    public TextInput()
    {
        _viewModel.ImportFromText(Text);
        _viewModel.PropertyChanged += (_, _) =>
        {
            _debouncer.Debounce(SetText, TimeSpan.FromMilliseconds(250));
        };
        DataContext = _viewModel;

        InitializeComponent();
    }

    private void SetText()
    {
        Dispatcher.UIThread.InvokeAsync(() => Text = _viewModel.ExportToText());
    }

    partial void OnTextPropertyChanged(Text newValue)
    {
        _viewModel.ImportFromText(newValue);
        TextChanged?.Invoke(newValue);
    }
}
