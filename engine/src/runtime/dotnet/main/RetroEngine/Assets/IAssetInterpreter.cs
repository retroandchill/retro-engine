// // @file IAssetInterpreter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetInterpreter
{
    Name AssetType { get; }

    ImmutableArray<string> Extensions { get; }

    string DefaultExtension => Extensions.First();
}
