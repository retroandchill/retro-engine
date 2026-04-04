// // @file IStringTableLoader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization.StringTables;

public interface IStringTableLoader
{
    bool CanFindOrLoadStringTableAsset { get; }

    Name LoadStringTableAsset(Name tableId);

    ValueTask<Name> LoadStringTableAssetAsync(Name tableId, CancellationToken cancellationToken = default);

    Name RedirectStringTableAsset(Name tableId);

    bool IsStringTableFromAsset(Name tableId);
}

public static partial class StringTableLoader
{
    public static IStringTableLoader? Instance
    {
        get;
        set => Interlocked.Exchange(ref field, value);
    }

    public static bool CanFindOrLoadStringTableAsset => Instance?.CanFindOrLoadStringTableAsset ?? true;

    private static void EnsureCanFindOrLoadStringTableAsset()
    {
        if (!CanFindOrLoadStringTableAsset)
            throw new InvalidOperationException("Cannot find or load string table asset");
    }

    [CreateSyncVersion]
    public static async ValueTask<Name> LoadStringTableAssetAsync(
        Name tableId,
        CancellationToken cancellationToken = default
    )
    {
        EnsureCanFindOrLoadStringTableAsset();

        if (Instance is not null)
            return await Instance.LoadStringTableAssetAsync(tableId, cancellationToken);

        return tableId;
    }

    public static Name RedirectStringTableAsset(Name tableId) => Instance?.RedirectStringTableAsset(tableId) ?? tableId;

    public static bool IsStringTableFromAsset(Name tableId) => Instance?.IsStringTableFromAsset(tableId) ?? false;
}
