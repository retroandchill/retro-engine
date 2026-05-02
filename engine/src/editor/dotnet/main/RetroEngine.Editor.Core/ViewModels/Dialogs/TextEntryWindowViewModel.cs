// // @file TextEntryWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Dialogs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Dialogs;

public delegate bool TextEntryValidationFunc(string text, out Text errorText);

[ViewModelFor<TextEntryWindow>]
public sealed partial class TextEntryWindowViewModel : ObservableObject, IModalDialogViewModel, ICloseable
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Dialogs.TextEntryWindowViewModel";

    private static readonly Text DefaultErrorText = Text.AsLocalizable(TextNamespace, "DefaultError", "Invalid input");

    [ObservableProperty]
    public partial Text WindowTitle { get; set; } = Text.AsLocalizable(TextNamespace, "WindowTitle", "Text Entry");

    [ObservableProperty]
    public partial Text Prompt { get; set; } = Text.AsLocalizable(TextNamespace, "DefaultPrompt", "Enter text");

    public string TextEntry
    {
        get;
        set
        {
            SetProperty(ref field, value);
            UpdateCanCreateValue();
        }
    } = "";

    [ObservableProperty]
    public partial Text ConfirmText { get; set; } = Text.AsLocalizable(TextNamespace, "Confirm", "Confirm");

    [ObservableProperty]
    public partial Text CancelText { get; set; } = Text.AsLocalizable(TextNamespace, "Cancel", "Cancel");

    [ObservableProperty]
    public partial Text? ErrorText { get; private set; }

    public TextEntryValidationFunc? ValidationFunc
    {
        get;
        set
        {
            field = value;
            UpdateCanCreateValue();
        }
    }

    [ObservableProperty]
    public partial bool CanCreate { get; private set; } = false;

    [ObservableProperty]
    public partial bool? DialogResult { get; private set; }

    public event EventHandler? RequestClose;

    [RelayCommand]
    private void Confirm()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCanCreateValue()
    {
        if (ValidationFunc is not null)
        {
            CanCreate = ValidationFunc(TextEntry, out var errorText);
            if (!CanCreate)
            {
                ErrorText = errorText.IsEmptyOrWhiteSpace ? DefaultErrorText : errorText;
            }
            else
            {
                ErrorText = null;
            }
        }
        else
        {
            CanCreate = !string.IsNullOrWhiteSpace(TextEntry);
            ErrorText = !CanCreate ? DefaultErrorText : (Text?)null;
        }
    }
}
