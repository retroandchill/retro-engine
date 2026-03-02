// // @file TextHistoryBase.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.History;

internal class TextHistoryBase : TextHistory
{
    public sealed override TextId TextId { get; }
    private readonly string _source = "";
    private string? _localized;

    public TextHistoryBase() { }

    public TextHistoryBase(TextId id, string source, string? localized = null)
    {
        TextId = id;
        _source = source;
        _localized = localized;
    }

    public override string SourceString => _source;
    public override string DisplayString => _localized ?? _source;
    public override string? LocalizedString => _localized;

    public override string BuildInvariantDisplayString()
    {
        return _source;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return false;
    }

    protected override bool CanUpdateDisplayString => !TextId.IsEmpty;

    internal override void UpdateDisplayString()
    {
        _localized = LocalizationManager.Instance.GetDisplayString(TextId.Namespace, TextId.Key, _source);
    }
}
