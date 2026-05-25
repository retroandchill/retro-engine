// @file LayoutSlot.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.UI;

public abstract class LayoutSlot(ContainerWidget parent, Widget child)
{
    public ContainerWidget Parent { get; private set; } = parent;
    public Widget Child { get; private set; } = child;
}
