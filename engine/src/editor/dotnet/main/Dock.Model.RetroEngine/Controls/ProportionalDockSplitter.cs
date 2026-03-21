// // @file ProportionalDockSplitter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public class ProportionalDockSplitter : DockableBase, IProportionalDockSplitter
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanResize
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ResizePreview
    {
        get;
        set => SetProperty(ref field, value);
    }
}
