// // @file DefaultAssetAssociations.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace RetroEngine.AssetTools;

internal static class DefaultAssetAssociations
{
    [RegisterServices]
    public static void RegisterDefaultAssociations(IServiceCollection services) { }
}
