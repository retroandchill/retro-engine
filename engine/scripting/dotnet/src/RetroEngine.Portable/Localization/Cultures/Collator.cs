// // @file Collator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum CollationResult
{
    Equal = 0,
    Greater = 1,
    Less = -1,
}

internal enum CollationStrength
{
    Default = -1,
    Primary = 0,
    Secondary = 1,
    Tertiary = 2,
    DefaultStrength = Tertiary,
    Quaternary = 3,
    Identical = 15,
    Off = 16,
    On = 17,
    Shifted = 20,
    NonIgnorable = 21,
    LowerFirst = 24,
    UpperFirst = 25,
}

internal sealed partial class Collator : IDisposable, ICloneable
{
    private readonly IntPtr _collator;
    private bool _disposed;

    public CollationStrength Strength
    {
        set => NativeSetStrength(_collator, value);
    }

    private Collator(IntPtr collator) => _collator = collator;

    ~Collator()
    {
        ReleaseUnmanagedResources();
    }

    public static Collator? Create(CultureId id)
    {
        var collator = NativeOpen(id, out _);
        return collator != IntPtr.Zero ? new Collator(collator) : null;
    }

    object ICloneable.Clone() => Clone();

    public Collator Clone() => new(NativeClone(_collator, out _));

    public CollationResult Compare(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    {
        return NativeStrColl(_collator, s1, s1.Length, s2, s2.Length);
    }

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ucol_open")]
    private static partial IntPtr NativeOpen(CultureId cultureId, out IcuErrorCode errorCode);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ucol_close")]
    private static partial void NativeClose(IntPtr collator);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ucol_clone")]
    private static partial IntPtr NativeClone(IntPtr collator, out IcuErrorCode errorCode);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ucol_strcoll")]
    private static partial CollationResult NativeStrColl(
        IntPtr collator,
        ReadOnlySpan<char> s1,
        int s1Len,
        ReadOnlySpan<char> s2,
        int s2Len
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ucol_setStrength")]
    private static partial void NativeSetStrength(IntPtr collator, CollationStrength strength);

    private void ReleaseUnmanagedResources()
    {
        NativeClose(_collator);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}

internal static class CollationExtensions
{
    public static CollationStrength ToCollationStrength(this TextComparisonLevel level)
    {
        return level switch
        {
            TextComparisonLevel.CultureDefault => CollationStrength.Default,
            TextComparisonLevel.IgnoreCaseAccentWidth => CollationStrength.Primary,
            TextComparisonLevel.IgnoreCase => CollationStrength.Secondary,
            TextComparisonLevel.CultureSensitive => CollationStrength.Tertiary,
            TextComparisonLevel.CultureSensitiveWithPunctuation => CollationStrength.Quaternary,
            TextComparisonLevel.Ordinal => CollationStrength.Identical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }
}
