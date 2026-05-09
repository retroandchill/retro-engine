// // @file TextPropertyInput.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using PropertyGenerator.Avalonia;
using RetroEngine.Editor.Core.ViewModels.Properties;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Views.Properties;

public partial class TextInput : UserControl
{
    [GeneratedStyledProperty]
    public partial Text Text { get; set; }
    private readonly TextInputViewModel _viewModel = new();

    public event Action<Text>? TextChanged;

    public TextInput()
    {
        _viewModel.ImportFromText(Text);
        _viewModel.PropertyChanged += (_, _) =>
        {
            Text = _viewModel.ExportToText();
        };
        DataContext = _viewModel;

        InitializeComponent();
    }

    partial void OnTextPropertyChanged(Text newValue)
    {
        _viewModel.ImportFromText(newValue);
        TextChanged?.Invoke(newValue);
    }
}
