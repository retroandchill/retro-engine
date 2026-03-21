// // @file ViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Editor.Core.Services;

public interface IViewModelFactory
{
    object CreateViewModel();
}

public interface IViewModelFactory<out TViewModel> : IViewModelFactory
    where TViewModel : class
{
    new TViewModel CreateViewModel();
}

public abstract class ViewModelFactory<TViewModel> : IViewModelFactory<TViewModel>
    where TViewModel : class
{
    public Type ViewModelType => typeof(TViewModel);

    public abstract TViewModel CreateViewModel();

    object IViewModelFactory.CreateViewModel() => CreateViewModel();
}
