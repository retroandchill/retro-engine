// @file CanvasPanel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Utilities;

namespace RetroEngine.UI.Containers;

public sealed class CanvasSlot(CanvasWidget parent, Widget content) : LayoutSlot(parent, content)
{
    public AnchorData LayoutData { get; set; }

    public int ZOrder { get; set; }

    public bool AutoSize { get; set; }

    public RectF GetContentRect(RectF availableRect)
    {
        var size = new Vector2F(availableRect.Width, availableRect.Height);
        var subRect = LayoutData.GetBounds(size);
        subRect = subRect with { X = subRect.X + availableRect.X, Y = subRect.Y + availableRect.Y };

        if (!AutoSize)
            return subRect;
        if (Math.IsNearlyEqual(LayoutData.Anchors.Minimum.X, LayoutData.Anchors.Maximum.X))
        {
            subRect = subRect with { Width = Content.DesiredSize.X };
        }

        if (Math.IsNearlyEqual(LayoutData.Anchors.Minimum.Y, LayoutData.Anchors.Maximum.Y))
        {
            subRect = subRect with { Height = Content.DesiredSize.Y };
        }

        return subRect;
    }
}

public sealed class CanvasWidget : ContainerWidget<CanvasSlot>
{
    public CanvasWidget(IUiRoot root)
        : base(root)
    {
        CanHaveMultipleChildren = true;
    }

    protected override CanvasSlot CreateLayoutSlot(Widget content)
    {
        return new CanvasSlot(this, content);
    }

    protected override Vector2F ComputeDesiredSize(Vector2F availableSize)
    {
        foreach (var slot in Slots)
        {
            slot.Content.Measure(availableSize);
        }

        // Canvas panels don't have any preferred size, but instead they have a desired size of 0.
        return Vector2F.Zero;
    }

    protected override void OnArrange(RectF finalRect)
    {
        foreach (var slot in Slots)
        {
            var contentRect = slot.GetContentRect(finalRect);
            slot.Content.Arrange(contentRect);
            slot.Content.SceneObject?.ZOrder = slot.ZOrder;
        }
    }
}
