// // @file DesignResolve.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace RetroEngine.Editor.Core.Extensions;

public sealed class DesignResolve(Type type) : MarkupExtension
{
    private Type Type { get; init; } = type;

    internal static IServiceProvider ServiceProvider { get; set; } = null!;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return ServiceProvider.GetRequiredService(Type);
    }
}
