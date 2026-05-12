// // @file IAssetTypeActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.AssetTools;

public interface IAssetTypeActions
{
    Type SupportedType { get; }

    AssetTypeCategories Categories { get; }
}
