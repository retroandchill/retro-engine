// // @file ViewModelService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class ViewModelProvider(IServiceProvider serviceProvider)
{
    private readonly ConcurrentDictionary<Type, IViewModelFactory> _factories = new();

    public object CreateViewModel(Type viewModelType)
    {
        var factory = _factories.GetOrAdd(
            viewModelType,
            t =>
            {
                var factoryType = typeof(IViewModelFactory<>).MakeGenericType(t);
                return (IViewModelFactory)serviceProvider.GetRequiredService(factoryType);
            }
        );
        return factory.CreateViewModel();
    }

    public TViewModel CreateViewModel<TViewModel>()
        where TViewModel : class
    {
        var factory =
            (IViewModelFactory<TViewModel>)
                _factories.GetOrAdd(
                    typeof(TViewModel),
                    _ => serviceProvider.GetRequiredService<IViewModelFactory<TViewModel>>()
                );
        return factory.CreateViewModel();
    }
}
