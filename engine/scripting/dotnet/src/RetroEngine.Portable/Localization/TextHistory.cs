// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal abstract class TextHistory : ITextData
{
    private readonly ReaderWriterLockSlim _lock = new();

    public virtual string SourceString => DisplayString;
    public abstract string DisplayString { get; }
    public virtual string? LocalizedString => null;

    public TextRevision Revision
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return field;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        private set;
    }

    public TextHistory History => this;

    public virtual TextId TextId => TextId.Empty;

    public abstract string BuildInvariantDisplayString();

    void UpdateDisplayStringIfOutOfDate()
    {
        if (!CanUpdateDisplayString)
            return;

        var currentRevision = LocalizationManager.Instance.GetTextRevision(TextId);

        _lock.EnterWriteLock();
        try
        {
            if (Revision == currentRevision)
                return;
            Revision = currentRevision;
            UpdateDisplayString();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected virtual bool CanUpdateDisplayString => true;

    protected abstract void UpdateDisplayString();

    protected void MarkDisplayStringOutOfDate()
    {
        _lock.EnterWriteLock();
        try
        {
            Revision = new TextRevision(0, 0);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected void MarkDisplayStringUpToDate()
    {
        var canUpdate = CanUpdateDisplayString;

        _lock.EnterWriteLock();
        try
        {
            Revision = canUpdate ? LocalizationManager.Instance.GetTextRevision(TextId) : new TextRevision(0, 0);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}

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

    protected override bool CanUpdateDisplayString => !TextId.IsEmpty;

    protected override void UpdateDisplayString()
    {
        _localized = LocalizationManager.Instance.GetDisplayString(TextId.Namespace, TextId.Key, _source);
    }
}
