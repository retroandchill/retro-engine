// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Concurrency;
using RetroEngine.Portable.Localization.Formatting;

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

    public void UpdateDisplayStringIfOutOfDate()
    {
        if (!CanUpdateDisplayString)
            return;

        var currentRevision = LocalizationManager.Instance.GetTextRevision(TextId);

        using var scope = _lock.EnterWriteScope();
        if (Revision == currentRevision)
            return;
        Revision = currentRevision;
        UpdateDisplayString();
    }

    protected virtual bool CanUpdateDisplayString => true;

    internal abstract void UpdateDisplayString();

    protected void MarkDisplayStringOutOfDate()
    {
        using var scope = _lock.EnterWriteScope();
        Revision = new TextRevision(0, 0);
    }

    protected void MarkDisplayStringUpToDate()
    {
        var canUpdate = CanUpdateDisplayString;
        using var scope = _lock.EnterWriteScope();
        Revision = canUpdate ? LocalizationManager.Instance.GetTextRevision(TextId) : new TextRevision(0, 0);
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

internal abstract class TextHistoryFormatNumber<T> : TextHistoryGenerated
    where T : unmanaged, INumber<T>
{
    protected T SourceValue { get; }

    private readonly string? _formatPattern;

    private readonly CultureHandle? _targetCulture;

    public TextHistoryFormatNumber() { }

    public TextHistoryFormatNumber(
        string displayString,
        T sourceValue,
        string formatString,
        CultureHandle? targetCulture
    )
        : base(displayString)
    {
        SourceValue = sourceValue;
        _formatPattern = formatString;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildNumericDisplayString(LocalizationManager.Instance.InvariantCulture.Culture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryFormatNumber<T> otherNumber
            && SourceValue == otherNumber.SourceValue
            && _formatPattern == otherNumber._formatPattern
            && Equals(_targetCulture, otherNumber._targetCulture);
    }

    protected override string BuildLocalizedDisplayString()
    {
        var targetCulture = _targetCulture ?? LocalizationManager.Instance.CurrentCulture;
        return BuildNumericDisplayString(targetCulture.Culture);
    }

    private string BuildNumericDisplayString(IFormatProvider formatProvider)
    {
        return SourceValue.ToString(_formatPattern, formatProvider);
    }
}

internal sealed class TextHistoryAsNumber<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    private readonly NumberFormatType _formatType;

    public TextHistoryAsNumber() { }

    public TextHistoryAsNumber(
        string displayString,
        T sourceValue,
        NumberFormatType formatType,
        string formatString,
        CultureHandle? targetCulture
    )
        : base(displayString, sourceValue, formatString, targetCulture)
    {
        _formatType = formatType;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(_formatType, FormatNumericArg.FromNumber(SourceValue));
    }
}

internal sealed class TextHistoryAsCurrency<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    public TextHistoryAsCurrency() { }

    public TextHistoryAsCurrency(string displayString, T sourceValue, string formatString, CultureHandle? targetCulture)
        : base(displayString, sourceValue, formatString, targetCulture) { }
}

internal sealed class TextHistoryAsDate : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly DateTimeFormatStyle _formatStyle;
    private readonly string? _timeZoneId;
    private readonly CultureHandle? _targetCulture;

    public TextHistoryAsDate() { }

    public TextHistoryAsDate(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        CultureHandle? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _formatStyle = formatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(LocalizationManager.Instance.InvariantCulture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsDate otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _formatStyle == otherDateHistory._formatStyle
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? LocalizationManager.Instance.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(CultureHandle culture)
    {
        return _sourceDateTime.ToDateString(_formatStyle, _timeZoneId, culture);
    }
}

internal sealed class TextHistoryAsTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly DateTimeFormatStyle _formatStyle;
    private readonly string? _timeZoneId;
    private readonly CultureHandle? _targetCulture;

    public TextHistoryAsTime() { }

    public TextHistoryAsTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        CultureHandle? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _formatStyle = formatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(LocalizationManager.Instance.InvariantCulture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsTime otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _formatStyle == otherDateHistory._formatStyle
            && _timeZoneId == otherDateHistory._timeZoneId
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? LocalizationManager.Instance.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(CultureHandle culture)
    {
        return _sourceDateTime.ToTimeString(_formatStyle, _timeZoneId, culture);
    }
}

internal sealed class TextHistoryAsDateTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private string? _customPattern;
    private readonly DateTimeFormatStyle _dateFormatStyle;
    private readonly DateTimeFormatStyle _timeFormatStyle;
    private readonly string? _timeZoneId;
    private readonly CultureHandle? _targetCulture;

    public TextHistoryAsDateTime() { }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormatStyle,
        DateTimeFormatStyle timeFormatStyle,
        string? timeZoneId,
        CultureHandle? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _dateFormatStyle = dateFormatStyle;
        _timeFormatStyle = timeFormatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        string pattern,
        string? timeZoneId,
        CultureHandle? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _customPattern = pattern;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(LocalizationManager.Instance.InvariantCulture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsDateTime otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _dateFormatStyle == otherDateHistory._dateFormatStyle
            && _timeFormatStyle == otherDateHistory._timeFormatStyle
            && _timeZoneId == otherDateHistory._timeZoneId
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? LocalizationManager.Instance.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(CultureHandle culture)
    {
        if (_customPattern is null)
            return _sourceDateTime.ToDateTimeString(_dateFormatStyle, _timeFormatStyle, _timeZoneId, culture);

        var timeZone = _timeZoneId is not null ? TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId) : TimeZoneInfo.Local;
        var localTime = _sourceDateTime.ToOffset(timeZone.GetUtcOffset(_sourceDateTime));
        return localTime.ToString(_customPattern, culture.Culture);
    }
}

internal sealed class TextHistoryAsTimespan : TextHistoryGenerated
{
    private readonly TimeSpan _sourceTimeSpan;
    private readonly CultureHandle? _targetCulture;

    public TextHistoryAsTimespan() { }

    public TextHistoryAsTimespan(string displayString, TimeSpan timeSpan, CultureHandle? targetCulture)
        : base(displayString)
    {
        _sourceTimeSpan = timeSpan;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildTimespanDisplayString(LocalizationManager.Instance.InvariantCulture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsTimespan otherAsTimespan
            && otherAsTimespan._sourceTimeSpan == _sourceTimeSpan
            && otherAsTimespan._targetCulture == _targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildTimespanDisplayString(_targetCulture ?? LocalizationManager.Instance.CurrentCulture);
    }

    private string BuildTimespanDisplayString(CultureHandle culture)
    {
        return _sourceTimeSpan.ToString("g", culture.Culture);
    }
}

internal sealed class TextHistoryTransformed : TextHistoryGenerated
{
    public enum TransformType
    {
        ToUpper,
        ToLower,
    }

    private readonly Text _sourceText;
    private readonly TransformType _transformType;

    public TextHistoryTransformed()
    {
        _sourceText = Text.Empty;
        _transformType = TransformType.ToUpper;
    }

    public TextHistoryTransformed(string displayString, Text sourceText, TransformType transformType)
        : base(displayString)
    {
        _sourceText = sourceText;
        _transformType = transformType;
    }

    public override string BuildInvariantDisplayString()
    {
        _sourceText.Rebuild();

        return _transformType switch
        {
            TransformType.ToUpper => _sourceText.BuildSourceString().ToUpperInvariant(),
            TransformType.ToLower => _sourceText.BuildSourceString().ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(_transformType), _transformType, null),
        };
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryTransformed otherTransformed
            && _sourceText.IdenticalTo(otherTransformed._sourceText, flags)
            && _transformType == otherTransformed._transformType;
    }

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        return _sourceText.HistoricFormatData;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return _sourceText.HistoricNumericData;
    }

    protected override string BuildLocalizedDisplayString()
    {
        _sourceText.Rebuild();

        return _transformType switch
        {
            TransformType.ToUpper => TextTransformer.ToUpper(_sourceText.ToString()),
            TransformType.ToLower => TextTransformer.ToLower(_sourceText.ToString()),
            _ => throw new ArgumentOutOfRangeException(nameof(_transformType), _transformType, null),
        };
    }
}
