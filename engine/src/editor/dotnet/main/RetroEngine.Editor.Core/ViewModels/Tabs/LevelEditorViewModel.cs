// // @file LevelEditorTabViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.Mvvm.Core;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<LevelEditorView>]
public partial class LevelEditorViewModel : DockableTabBase;
