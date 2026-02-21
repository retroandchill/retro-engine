// // @file TextHistoryGenerated.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.History;

internal abstract class TextHistoryGenerated(string displayString) : TextHistory
{
    private string _displayString = displayString;

    public TextHistoryGenerated()
        : this("") { }

    public sealed override TextId TextId => TextId.Empty;
    public override string DisplayString => _displayString;

    internal override void UpdateDisplayString()
    {
        _displayString = BuildLocalizedDisplayString();
    }

    protected abstract string BuildLocalizedDisplayString();
}
