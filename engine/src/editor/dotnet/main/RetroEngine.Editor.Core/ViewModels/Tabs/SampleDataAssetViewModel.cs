// // @file SampleDataAssetViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.RetroEngine.Controls;
using RetroEngine.Assets;
using RetroEngine.Assets.Decoders;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Mappers;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Async;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class SampleDataAssetViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool BoolProperty { get; set; }

    [ObservableProperty]
    public partial int IntProperty { get; set; }

    [ObservableProperty]
    public partial float FloatProperty { get; set; }

    [ObservableProperty]
    public partial Name NameProperty { get; set; }

    [ObservableProperty]
    public partial string StringProperty { get; set; } = "";

    [ObservableProperty]
    public partial Text TextProperty { get; set; }
}

[ViewModelFor<SampleDataAssetEditor>]
public sealed partial class SampleDataAssetEditorViewModel : Document, IAssetViewModel
{
    public required AssetPath Path { get; init; }
    public bool IsReadOnly => false;

    [ObservableProperty]
    public required partial SampleDataAsset Target { get; set; }

    object IAssetViewModel.Asset => Target;

    public SampleDataAssetViewModel EditableTarget { get; } = new();

    private readonly Debouncer _editDebouncer = new();

    public SampleDataAssetEditorViewModel()
    {
        CanClose = true;
        EditableTarget.PropertyChanged += (_, _) =>
        {
            _editDebouncer.Debounce(() => Target = EditableTarget.ToAsset(), TimeSpan.FromMilliseconds(100));
        };
    }

    partial void OnTargetChanged(SampleDataAsset value)
    {
        value.UpdateViewModel(EditableTarget);
    }
}
