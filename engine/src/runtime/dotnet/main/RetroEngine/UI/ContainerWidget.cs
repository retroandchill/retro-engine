// @file ContainerWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZLinq;

namespace RetroEngine.UI;

public abstract class ContainerWidget : Widget
{
    public abstract IReadOnlyList<LayoutSlot> Slots { get; }

    public bool CanHaveMultipleChildren { get; protected init; }

    public bool CanAddMoreChildren => CanHaveMultipleChildren || ChildrenCount == 0;

    public abstract int ChildrenCount { get; }

    public abstract IEnumerable<Widget> Children { get; }

    public abstract Widget this[int index] { get; set; }

    public abstract int ChildIndex(Widget child);

    public abstract bool HasChild(Widget child);

    public abstract LayoutSlot AddChild(Widget content);

    public abstract LayoutSlot InsertChildAt(int index, Widget content);

    public abstract void RemoveChildAt(int index);

    public abstract void RemoveChild(Widget child);
}

public abstract class ContainerWidget<TSlot> : ContainerWidget
    where TSlot : LayoutSlot
{
    private readonly List<TSlot> _slots = [];

    public sealed override IReadOnlyList<TSlot> Slots => _slots;

    public sealed override int ChildrenCount => _slots.Count;

    public sealed override Widget this[int index]
    {
        get
        {
            ThrowIfDisposed();
            return _slots[index].Content;
        }
        set
        {
            ThrowIfDisposed();
            var slot = _slots[index];
            if (ReferenceEquals(slot.Content, value))
                return;

            slot.Content.LayoutSlot = null;
            slot.Content.Dispose();
            slot.Content = value;
            value.LayoutSlot = slot;
        }
    }

    public sealed override IEnumerable<Widget> Children
    {
        get
        {
            ThrowIfDisposed();
            return _slots.Select(slot => slot.Content);
        }
    }

    public sealed override int ChildIndex(Widget child)
    {
        ThrowIfDisposed();
        foreach (var (i, slot) in _slots.AsValueEnumerable().Index())
        {
            if (ReferenceEquals(slot.Content, child))
                return i;
        }

        return -1;
    }

    public sealed override bool HasChild(Widget child)
    {
        ThrowIfDisposed();
        return ReferenceEquals(child.Parent, this);
    }

    public sealed override TSlot AddChild(Widget content)
    {
        ThrowIfDisposed();
        if (!CanHaveMultipleChildren && ChildrenCount > 0)
            throw new InvalidOperationException("Cannot add more children to a container that already has a child.");

        var slot = CreateLayoutSlot(content);
        _slots.Add(slot);
        content.LayoutSlot = slot;
        content.Root = Root;
        OnSlotAdded(slot);
        InvalidateMeasure();
        return slot;
    }

    protected abstract TSlot CreateLayoutSlot(Widget content);

    public sealed override TSlot InsertChildAt(int index, Widget content)
    {
        ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, ChildrenCount);

        if (!CanHaveMultipleChildren && ChildrenCount > 0)
            throw new InvalidOperationException("Cannot add more children to a container that already has a child.");

        var slot = CreateLayoutSlot(content);
        _slots.Insert(index, slot);
        content.LayoutSlot = slot;
        content.Root = Root;
        OnSlotAdded(slot);
        InvalidateMeasure();
        return slot;
    }

    public sealed override void RemoveChildAt(int index)
    {
        ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, ChildrenCount);

        var slot = _slots[index];
        _slots.RemoveAt(index);

        slot.Content.LayoutSlot = null;
        slot.Content.Root = null;
        OnSlotRemoved(slot);
        slot.Content.Dispose();

        InvalidateMeasure();
    }

    public sealed override void RemoveChild(Widget child)
    {
        ThrowIfDisposed();
        var index = ChildIndex(child);
        if (index == -1)
            return;

        RemoveChildAt(index);
    }

    protected override void OnAttached(UiRoot root)
    {
        base.OnAttached(root);
        foreach (var slot in _slots)
            slot.Content.Root = root;
    }

    protected override void OnDetached()
    {
        base.OnDetached();
        foreach (var slot in _slots)
            slot.Content.Root = null;
    }

    protected virtual void OnSlotAdded(TSlot slot) { }

    protected virtual void OnSlotRemoved(TSlot slot) { }
}
