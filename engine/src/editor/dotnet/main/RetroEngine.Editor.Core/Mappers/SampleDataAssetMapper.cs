// @file SampleDataAssetMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets.Decoders;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using Riok.Mapperly.Abstractions;

namespace RetroEngine.Editor.Core.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class SampleDataAssetMapper
{
    public static partial SampleDataAsset ToAsset(this SampleDataAssetViewModel viewModel);

    public static partial void UpdateViewModel(this SampleDataAsset asset, SampleDataAssetViewModel viewModel);
}
