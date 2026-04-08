// // @file TextHistoryBase.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MessagePack;

namespace RetroEngine.Portable.Localization.History;

internal abstract class TextHistoryBase : TextHistory
{
    [Key(0)]
    public sealed override TextId TextId { get; } = TextId.Empty;

    [Key(1)]
    protected string Source { get; } = "";

    [IgnoreMember]
    private string? _localized;

    protected TextHistoryBase() { }

    protected TextHistoryBase(TextId id, string source, string? localized = null)
    {
        TextId = id;
        Source = source;
        _localized = localized;
    }

    public override string SourceString => Source;
    public override string DisplayString => _localized ?? Source;
    public override string? LocalizedString => _localized;

    public override string BuildInvariantDisplayString()
    {
        return Source;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return false;
    }

    protected override bool CanUpdateDisplayString => !TextId.IsEmpty;

    internal override void UpdateDisplayString()
    {
        _localized = LocalizationManager.Instance.GetDisplayString(TextId.Namespace, TextId.Key, Source);
    }
}
