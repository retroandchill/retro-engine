// @file ContainerWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.UI;

public abstract class ContainerWidget() : Widget(null)
{
    public abstract void Remove(Widget child);
}
