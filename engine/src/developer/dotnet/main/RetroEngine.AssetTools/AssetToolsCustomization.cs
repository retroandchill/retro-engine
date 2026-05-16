// @file AssetToolsCustomization.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace RetroEngine.AssetTools;

internal sealed class AssetToolCustomizer(Action<IAssetTools> configure)
{
    public void Configure(IAssetTools tools)
    {
        configure(tools);
    }
}

public static class AssetToolsCustomization
{
    public static IServiceCollection ConfigureAssetTools(
        this IServiceCollection services,
        Action<IAssetTools> configure
    )
    {
        return services.AddSingleton(new AssetToolCustomizer(configure));
    }
}
