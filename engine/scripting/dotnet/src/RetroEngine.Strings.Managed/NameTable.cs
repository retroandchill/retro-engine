using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RetroEngine.Strings;

internal readonly record struct NameHashEntry(uint Id, int Hash);

internal readonly record struct NameIndices(uint ComparisonIndex, uint DisplayStringIndex)
{
    public bool IsNone => ComparisonIndex == 0;

    public static readonly NameIndices None = new(0, 0);
}

internal class NameTable
{
    private const int BucketCount = 1024;
    private const int BucketMask = BucketCount - 1;

    private readonly ConcurrentBag<NameHashEntry>[] _comparisonBuckets =
        new ConcurrentBag<NameHashEntry>[BucketCount];
    private readonly ConcurrentDictionary<uint, string> _comparisonStrings = new();

    private readonly ConcurrentBag<NameHashEntry>[] _displayBuckets =
        new ConcurrentBag<NameHashEntry>[BucketCount];
    private readonly ConcurrentDictionary<uint, string> _displayStrings = new();

    private uint _nextComparisonId = 1;
    private uint _nextDisplayId = 1;

    public static readonly NameTable Instance = new();

    private NameTable()
    {
        for (var i = 0; i < BucketCount; i++)
        {
            _comparisonBuckets[i] = [];
            _displayBuckets[i] = [];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeHashIgnoreCase(ReadOnlySpan<char> span)
    {
        var hash = 0;

        foreach (var t in span)
        {
            var c = char.ToLowerInvariant(t);
            hash = ((hash << 5) + hash) ^ c;
        }
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeHashCaseSensitive(ReadOnlySpan<char> span)
    {
        var hash = 0;

        foreach (var c in span)
        {
            hash = ((hash << 5) + hash) ^ c;
        }
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SpanEqualsString(
        ReadOnlySpan<char> span,
        string str,
        bool caseSensitive = false
    )
    {
        if (span.Length != str.Length)
            return false;

        for (var i = 0; i < span.Length; i++)
        {
            if (caseSensitive)
            {
                if (span[i] != str[i])
                    return false;

                continue;
            }

            if (char.ToLowerInvariant(span[i]) != char.ToLowerInvariant(str[i]))
                return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NameIndices GetOrAddEntry(ReadOnlySpan<char> value, FindName findType)
    {
        if (IsNoneSpan(value))
            return NameIndices.None;

        var hashIgnore = ComputeHashIgnoreCase(value);
        var hashCase = ComputeHashCaseSensitive(value);

        var cmpBucketIdx = hashIgnore & BucketMask;
        var dispBucketIdx = hashCase & BucketMask;

        var cmpBucket = _comparisonBuckets[cmpBucketIdx];
        var dispBucket = _displayBuckets[dispBucketIdx];

        uint comparisonId = 0;
        uint displayId = 0;

        // ---- Comparison lookup (case-insensitive) ----
        foreach (var entry in cmpBucket)
        {
            if (entry.Hash != hashIgnore)
                continue;

            if (!SpanEqualsString(value, _comparisonStrings[entry.Id], caseSensitive: false))
                continue;

            comparisonId = entry.Id;
            break;
        }

        // ---- Display lookup (case-sensitive) ----
        foreach (var entry in dispBucket)
        {
            if (entry.Hash != hashCase)
                continue;

            if (!SpanEqualsString(value, _displayStrings[entry.Id], caseSensitive: true))
                continue;
            displayId = entry.Id;
            break;
        }

        // If both found, we're done.
        if (comparisonId != 0 && displayId != 0)
            return new NameIndices(comparisonId, displayId);

        if (findType == FindName.Find)
        {
            // For strict "find only", require that both indices exist.
            // If you want looser semantics (e.g., comparison exists but
            // display doesn't), this is the place to tweak.
            return new NameIndices(0, 0);
        }

        // ---- Add path ----
        // We only allocate the string once here.
        var str = value.ToString();

        // (1) Ensure comparison entry exists
        if (comparisonId == 0)
        {
            comparisonId = _nextComparisonId;
            Interlocked.Increment(ref _nextComparisonId);
            var cmpEntry = new NameHashEntry(comparisonId, hashIgnore);
            cmpBucket.Add(cmpEntry);
            _comparisonStrings.TryAdd(comparisonId, str);
        }

        // (2) Ensure display entry exists
        if (displayId != 0)
            return new NameIndices(comparisonId, displayId);

        displayId = _nextDisplayId;
        Interlocked.Increment(ref _nextDisplayId);
        var dispEntry = new NameHashEntry(displayId, hashCase);
        dispBucket.Add(dispEntry);
        _displayStrings.TryAdd(displayId, str);

        return new NameIndices(comparisonId, displayId);
    }

    public bool IsValid(uint comparisonId, uint displayId)
    {
        if (comparisonId == 0 || displayId == 0)
        {
            return false;
        }

        return _comparisonStrings.ContainsKey(comparisonId)
            && _displayStrings.ContainsKey(displayId);
    }

    /// <summary>
    /// Get display string for a display id (case-preserving).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDisplayString(uint displayId) =>
        _displayStrings.GetValueOrDefault(displayId, "None");

    /// <summary>
    /// Compare comparison id against a span, case-insensitive.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool EqualsComparison(uint comparisonId, ReadOnlySpan<char> span)
    {
        if (comparisonId == 0)
            return IsNoneSpan(span);

        return _comparisonStrings.TryGetValue(comparisonId, out var value)
            && SpanEqualsString(span, value, caseSensitive: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNoneSpan(ReadOnlySpan<char> span)
    {
        return span.IsEmpty
            || (
                span.Length == 4
                && (span[0] == 'N' || span[0] == 'n')
                && (span[1] == 'o' || span[1] == 'O')
                && (span[2] == 'n' || span[2] == 'N')
                && (span[3] == 'e' || span[3] == 'E')
            );
    }
}
