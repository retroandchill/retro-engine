// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal enum TextHistoryType : sbyte
{
    None = -1,
    Base = 0,
    NamedFormat,
    OrderedFormat,
    ArgumentFormat,
    AsNumber,
    AsPercent,
    AsCurrency,
    AsDate,
    AsTime,
    AsDateTime,
    Transform,
    StringTableEntry,
    TextGenerator,
}

internal abstract class TextHistory : ITextData
{
    public virtual string SourceString => DisplayString;
    public abstract string DisplayString { get; }
    public virtual string? LocalizedString => null;
    private ushort _globalHistoryRevision;
    public ushort GlobalHistoryRevision
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _globalHistoryRevision;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    private ushort _localHistoryRevision;
    public ushort LocalHistoryRevision
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _localHistoryRevision;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    public TextHistory History => this;
    private readonly ReaderWriterLockSlim _lock = new();

    public abstract TextHistoryType HistoryType { get; }

    public virtual TextId TextId => new();

    public abstract string BuildInvariantDisplayString(TextHistory other, TextIdenticalModeFlags compareModeFlags);

    public virtual IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text) => [];

    public virtual HistoricTextNumericData? GetHistoryNumericData(Text text) => null;

    public void UpdateDisplayStringIfOutOfDate()
    {
        if (!CanUpdateDisplayString)
            return;

        var (globalRevision, localRevision) = LocalizationManager.Instance.GetTextRevisions(TextId);

        _lock.EnterWriteLock();
        try
        {
            if (_globalHistoryRevision == globalRevision && _localHistoryRevision == localRevision)
                return;

            _globalHistoryRevision = globalRevision;
            _localHistoryRevision = localRevision;
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
            _globalHistoryRevision = 0;
            _localHistoryRevision = 0;
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
            if (canUpdate)
            {
                (_globalHistoryRevision, _localHistoryRevision) = LocalizationManager.Instance.GetTextRevisions(TextId);
            }
            else
            {
                _globalHistoryRevision = 0;
                _localHistoryRevision = 0;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
