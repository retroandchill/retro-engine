// // @file ViewModelProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core;

[RegisterSingleton]
public sealed class ViewModelProvider(IEnumerable<IViewModelFactory> factories)
{
    private readonly ImmutableDictionary<Type, IViewModelFactory> _factories = factories.ToImmutableDictionary(x =>
        x.ViewModelType
    );

    public IViewModel Create(Type type)
    {
        return _factories.TryGetValue(type, out var factory)
            ? factory.Create()
            : throw new TypeLoadException($"No factory found for {type.FullName}");
    }

    public TViewModel Create<TViewModel>()
        where TViewModel : IViewModel
    {
        return (TViewModel)Create(typeof(TViewModel));
    }
}
