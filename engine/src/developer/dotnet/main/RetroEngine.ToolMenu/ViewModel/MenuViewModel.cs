// // @file MenuViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;

namespace RetroEngine.ToolMenu.ViewModel;

public sealed partial class MenuViewModel : ObservableObject, IDisposable
{
    private readonly SourceList<IMenuItemSource> _itemSources = new();
    private readonly IDisposable _menuItemsSubscription;

    [ObservableProperty]
    public partial object? Context { get; set; }

    private readonly ReadOnlyObservableCollection<IMenuItemEntry> _menuItems;

    public ReadOnlyObservableCollection<IMenuItemEntry> MenuItems => _menuItems;

    public MenuViewModel()
    {
        var contextChanged = Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => PropertyChanged += handler,
                handler => PropertyChanged -= handler
            )
            .Where(args => args.EventArgs.PropertyName == nameof(Context))
            .Select(_ => Context)
            .StartWith(Context);

        _menuItemsSubscription = contextChanged
            .Select(context => _itemSources.Connect().AutoRefresh().TransformMany(source => source.Build(context)))
            .Switch()
            .Bind(out _menuItems)
            .Subscribe();
    }

    public void Dispose()
    {
        _menuItemsSubscription.Dispose();
        _itemSources.Dispose();
    }
}
