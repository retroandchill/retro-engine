// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Concurrency;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

internal interface ITextHistory : ITextData
{
    static virtual ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return ParseResult.Empty<ITextData>(input);
    }
}

internal abstract class TextHistory : ITextData
{
    private readonly ReaderWriterLockSlim _lock = new();

    public virtual string SourceString => DisplayString;
    public abstract string DisplayString { get; }
    public virtual string? LocalizedString => null;

    private TextRevisions _revisions;
    public TextRevisions Revisions
    {
        get
        {
            using var scope = _lock.EnterReadScope();
            return _revisions;
        }
    }

    public TextHistory History => this;

    public virtual TextId TextId => TextId.Empty;

    public abstract string BuildInvariantDisplayString();

    public abstract bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags);

    public virtual IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text) => [];

    public virtual HistoricTextNumericData? GetHistoricNumericData(Text text) => null;

    public virtual bool ExportToString(StringBuilder buffer) => false;

    public void UpdateDisplayStringIfOutOfDate()
    {
        if (!CanUpdateDisplayString)
            return;

        var currentRevision = LocalizationManager.Instance.GetTextRevisions(TextId);

        using var scope = _lock.EnterWriteScope();
        if (_revisions == currentRevision)
            return;
        _revisions = currentRevision;
        UpdateDisplayString();
    }

    protected virtual bool CanUpdateDisplayString => true;

    internal abstract void UpdateDisplayString();

    protected void MarkDisplayStringOutOfDate()
    {
        using var scope = _lock.EnterWriteScope();
        _revisions = new TextRevisions(0, 0);
    }

    protected void MarkDisplayStringUpToDate()
    {
        var canUpdate = CanUpdateDisplayString;
        using var scope = _lock.EnterWriteScope();
        _revisions = canUpdate ? LocalizationManager.Instance.GetTextRevisions(TextId) : new TextRevisions(0, 0);
    }
}
