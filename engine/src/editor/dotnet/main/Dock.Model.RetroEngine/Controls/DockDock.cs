// // @file DockDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public partial class DockDock : DockBase, IDockDock
{
    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool LastChildFill { get; set; } = true;
}
