// // @file IMenuItemSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemSource : INotifyPropertyChanged
{
    IEnumerable<IMenuItemEntry> Build(object? context);
}
