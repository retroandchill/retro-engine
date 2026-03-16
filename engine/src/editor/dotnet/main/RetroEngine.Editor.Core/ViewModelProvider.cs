// // @file ViewModelProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core;

[RegisterSingleton]
public sealed class ViewModelProvider(IServiceProvider serviceProvider)
{
    private readonly ConcurrentDictionary<Type, IViewModelFactory> _factoriesCache = new();

    public IViewModel Create(Type type)
    {
        var factory = _factoriesCache.GetOrAdd(
            type,
            static (t, p) =>
            {
                var factoryType = typeof(IViewModelFactory<>).MakeGenericType(t);
                return (IViewModelFactory)p.GetRequiredService(factoryType);
            },
            serviceProvider
        );
        return factory.Create();
    }

    public TViewModel Create<TViewModel>()
        where TViewModel : IViewModel
    {
        var factory = _factoriesCache.GetOrAdd(
            typeof(TViewModel),
            static (_, p) => p.GetRequiredService<IViewModelFactory<TViewModel>>(),
            serviceProvider
        );
        return (TViewModel)factory.Create();
    }
}
