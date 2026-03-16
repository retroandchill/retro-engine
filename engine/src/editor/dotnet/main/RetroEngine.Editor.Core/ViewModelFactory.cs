// // @file ViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core;

public interface IViewModelFactory
{
    Type ViewModelType { get; }

    IViewModel Create();
}

public interface IViewModelFactory<out TViewModel> : IViewModelFactory
    where TViewModel : IViewModel
{
    TViewModel Create();
}

public abstract class ViewModelFactory<TViewModel> : IViewModelFactory<TViewModel>
    where TViewModel : IViewModel
{
    public Type ViewModelType => typeof(TViewModel);

    IViewModel IViewModelFactory.Create() => Create();

    public abstract TViewModel Create();
}
