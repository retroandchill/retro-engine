// // @file LabeledSeparator.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls.Primitives;
using PropertyGenerator.Avalonia;

namespace RetroEngine.ToolMenu.Controls;

public sealed partial class LabeledSeparator : TemplatedControl
{
    [GeneratedStyledProperty]
    public partial string? Header { get; set; }

    [GeneratedStyledProperty]
    public partial double HeaderSpacing { get; set; }

    [GeneratedStyledProperty]
    public partial double SeparatorHeight { get; set; }

    [GeneratedDirectProperty]
    internal partial double ActualHeaderSpacing { get; set; }

    partial void OnHeaderPropertyChanged(string? newValue)
    {
        ActualHeaderSpacing = !string.IsNullOrEmpty(newValue) ? HeaderSpacing : 0;
    }

    partial void OnHeaderSpacingPropertyChanged(double newValue)
    {
        ActualHeaderSpacing = !string.IsNullOrEmpty(Header) ? newValue : 0;
    }
}
