// // @file MyDocumentModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Model;

public sealed class SampleDocumentModel : INotifyPropertyChanged
{
    public Text Title
    {
        get;
        set => SetProperty(ref field, value);
    } = "";

    public Text Content
    {
        get;
        set => SetProperty(ref field, value);
    } = "";

    public Text EditableContent
    {
        get;
        set => SetProperty(ref field, value);
    } = "";

    public string Status
    {
        get;
        set => SetProperty(ref field, value);
    } = "New";

    public bool CanClose
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}
