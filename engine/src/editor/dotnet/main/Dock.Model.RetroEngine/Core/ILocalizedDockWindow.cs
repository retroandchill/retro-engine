// // @file ILocalizedDockWindow.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.Core;
using RetroEngine.Portable.Localization;

namespace Dock.Model.RetroEngine.Core;

public interface ILocalizedDockWindow : IDockWindow
{
    new Text Title { get; set; }
}
