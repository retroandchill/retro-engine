// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Concurrency;
using Superpower.Model;

namespace RetroEngine.Portable.Localization.History;

internal interface ITextHistory : ITextData
{
    static virtual Result<ITextData> ReadFromBuffer(TextSpan input, string? textNamespace)
    {
        return Result.Empty<ITextData>(input);
    }
}

internal abstract class TextHistory : ITextData
{
    private readonly ReaderWriterLockSlim _lock = new();

    public virtual string SourceString => DisplayString;
    public abstract string DisplayString { get; }
    public virtual string? LocalizedString => null;

    public TextRevisions Revisions
    {
        get
        {
            using var scope = _lock.EnterReadScope();
            return field;
        }
        private set;
    }

    public TextHistory History => this;

    public virtual TextId TextId => TextId.Empty;

    public abstract string BuildInvariantDisplayString();

    public abstract bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags);

    public virtual IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text) => [];

    public virtual HistoricTextNumericData? GetHistoricNumericData(Text text) => null;

    public virtual bool WriteToBuffer(StringBuilder buffer) => false;

    public void UpdateDisplayStringIfOutOfDate()
    {
        if (!CanUpdateDisplayString)
            return;

        var currentRevision = LocalizationManager.Instance.GetTextRevisions(TextId);

        using var scope = _lock.EnterWriteScope();
        if (Revisions == currentRevision)
            return;
        Revisions = currentRevision;
        UpdateDisplayString();
    }

    protected virtual bool CanUpdateDisplayString => true;

    internal abstract void UpdateDisplayString();

    protected void MarkDisplayStringOutOfDate()
    {
        using var scope = _lock.EnterWriteScope();
        Revisions = new TextRevisions(0, 0);
    }

    protected void MarkDisplayStringUpToDate()
    {
        var canUpdate = CanUpdateDisplayString;
        using var scope = _lock.EnterWriteScope();
        Revisions = canUpdate ? LocalizationManager.Instance.GetTextRevisions(TextId) : new TextRevisions(0, 0);
    }
}
