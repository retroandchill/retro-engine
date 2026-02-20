// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Concurrency;
using RetroEngine.Portable.Localization.Cultures;
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

    protected NumberFormattingOptions? FormattingOptions { get; }

    protected Culture? TargetCulture { get; }

    public TextHistoryFormatNumber() { }

    public TextHistoryFormatNumber(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString)
    {
        SourceValue = sourceValue;
        FormattingOptions = formattingOptions;
        TargetCulture = targetCulture;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryFormatNumber<T> otherNumber
            && GetType() == other.GetType()
            && SourceValue == otherNumber.SourceValue
            && FormattingOptions == otherNumber.FormattingOptions
            && Equals(TargetCulture, otherNumber.TargetCulture);
    }

    protected string BuildNumericDisplayString(DecimalNumberFormattingRules formattingRules, int valueMultiplier = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valueMultiplier);
        var formattingOptions = FormattingOptions ?? formattingRules.DefaultFormattingOptions;
        return FastDecimalFormat.NumberToString(SourceValue, formattingRules, formattingOptions);
    }
}

internal sealed class TextHistoryAsNumber<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    public TextHistoryAsNumber() { }

    public TextHistoryAsNumber(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture) { }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? Culture.CurrentCulture;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = Culture.InvariantCulture;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Number, FormatNumericArg.FromNumber(SourceValue));
    }
}

internal sealed class TextHistoryAsPercent<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    public TextHistoryAsPercent() { }

    public TextHistoryAsPercent(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture) { }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? Culture.CurrentCulture;
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = Culture.InvariantCulture;
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Percent, FormatNumericArg.FromNumber(SourceValue));
    }
}

internal sealed class TextHistoryAsCurrency<T> : TextHistoryFormatNumber<T>
    where T : unmanaged, INumber<T>
{
    private readonly string? _currencyCode;

    public TextHistoryAsCurrency() { }

    public TextHistoryAsCurrency(
        string displayString,
        T sourceValue,
        string? currencyCode,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture)
    {
        _currencyCode = currencyCode;
    }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? Culture.CurrentCulture;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = Culture.InvariantCulture;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }
}

internal sealed class TextHistoryAsDate : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly DateTimeFormatStyle _formatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsDate() { }

    public TextHistoryAsDate(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        Culture? targetCulture
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
        return BuildDateTimeDisplayString(Culture.InvariantCulture);
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
        return BuildDateTimeDisplayString(_targetCulture ?? Culture.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return TextChronoFormatter.AsDate(_sourceDateTime, _formatStyle, _timeZoneId, culture);
    }
}

internal sealed class TextHistoryAsTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly DateTimeFormatStyle _formatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsTime() { }

    public TextHistoryAsTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        Culture? targetCulture
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
        return BuildDateTimeDisplayString(Culture.InvariantCulture);
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
        return BuildDateTimeDisplayString(_targetCulture ?? Culture.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return TextChronoFormatter.AsTime(_sourceDateTime, _formatStyle, _timeZoneId, culture);
    }
}

internal sealed class TextHistoryAsDateTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly string? _customPattern;
    private readonly DateTimeFormatStyle _dateFormatStyle;
    private readonly DateTimeFormatStyle _timeFormatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsDateTime() { }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormatStyle,
        DateTimeFormatStyle timeFormatStyle,
        string? timeZoneId,
        Culture? targetCulture
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
        Culture? targetCulture
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
        return BuildDateTimeDisplayString(Culture.InvariantCulture);
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
        return BuildDateTimeDisplayString(_targetCulture ?? Culture.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return _customPattern is not null
            ? TextChronoFormatter.AsDateTime(_sourceDateTime, _customPattern, _timeZoneId, culture)
            : TextChronoFormatter.AsDateTime(_sourceDateTime, _dateFormatStyle, _timeFormatStyle, _timeZoneId, culture);
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
