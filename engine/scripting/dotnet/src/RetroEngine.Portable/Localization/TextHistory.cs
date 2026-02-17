// // @file TextHistory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
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

    protected NumberFormattingOptions? FormatOptions { get; }

    protected CultureHandle? TargetCulture { get; }

    protected TextHistoryFormatNumber() { }

    protected TextHistoryFormatNumber(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formatOptions = null,
        CultureHandle? targetCulture = null
    )
        : base(displayString)
    {
        SourceValue = sourceValue;
        FormatOptions = formatOptions;
        TargetCulture = targetCulture;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryFormatNumber<T> castOther
            && SourceValue == castOther.SourceValue
            && (FormatOptions ?? NumberFormattingOptions.DefaultWithGrouping)
                == (castOther.FormatOptions ?? NumberFormattingOptions.DefaultWithGrouping)
            && Equals(TargetCulture, castOther.TargetCulture);
    }

    protected string BuildNumericDisplayString(DecimalNumberFormattingRules formattingRules, int valueMultiplier = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valueMultiplier);
        var formattingOptions = FormatOptions ?? formattingRules.DefaultFormattingOptions;
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
        NumberFormattingOptions? formatOptions = null,
        CultureHandle? targetCulture = null
    )
        : base(displayString, sourceValue, formatOptions, targetCulture) { }

    public override string BuildInvariantDisplayString()
    {
        var culture = LocalizationManager.Instance.InvariantCulture;
        var numberFormatInfo = culture.NumberFormattingRules;
        return BuildNumericDisplayString(numberFormatInfo);
    }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? LocalizationManager.Instance.CurrentCulture;
        var numberFormatInfo = culture.NumberFormattingRules;
        return BuildNumericDisplayString(numberFormatInfo);
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
