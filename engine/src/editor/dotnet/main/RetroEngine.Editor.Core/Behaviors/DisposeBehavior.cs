// // @file DisposeBehavior.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Xaml.Interactivity;

namespace RetroEngine.Editor.Core.Behaviors;

public class DisposeBehavior : Behavior<Visual>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.DetachedFromVisualTree -= AssociatedObjectOnDetachedFromVisualTree;
        }

        if (AssociatedObject?.DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnDetaching();
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject?.DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
