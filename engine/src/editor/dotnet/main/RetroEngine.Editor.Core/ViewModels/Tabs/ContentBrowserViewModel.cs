// // @file ContentBrowserViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.RetroEngine.Controls;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<ContentBrowserView>]
public partial class ContentBrowserViewModel : Tool
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserViewModel";

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
    }
}
