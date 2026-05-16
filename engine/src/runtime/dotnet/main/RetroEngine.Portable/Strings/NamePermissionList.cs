// @file NamePermissionList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using RetroEngine.Utilities;
using RetroEngine.Utilities.Collections;

namespace RetroEngine.Portable.Strings;

public sealed class NamePermissionList
{
    private readonly Dictionary<Name, List<Name>> _denyList = [];
    public IReadOnlyDictionary<Name, IReadOnlyList<Name>> DenyList { get; }
    private readonly Dictionary<Name, List<Name>> _allowList = [];
    public IReadOnlyDictionary<Name, IReadOnlyList<Name>> AllowList { get; }
    private readonly List<Name> _denyAllList = [];
    public event Action? OnFilterChanged;
    private bool _suppressOnFilterChanged;

    public bool HasFiltering => _denyList.Count > 0 || _allowList.Count > 0 || _denyAllList.Count > 0;

    public IEnumerable<Name> OwnerNames =>
        _denyList
            .Values.SelectMany(x => x)
            .Concat(_allowList.Values.SelectMany(x => x))
            .Concat(_denyAllList)
            .Distinct();

    public bool IsDenyAllList => _denyAllList.Count > 0;

    public NamePermissionList()
    {
        DenyList = new NameListView(_denyList);
        AllowList = new NameListView(_allowList);
    }

    [Pure]
    public bool PassesFilter(Name item)
    {
        if (_denyAllList.Count > 0)
            return false;

        if (_allowList.Count > 0 && !_allowList.ContainsKey(item))
            return false;

        return !_denyList.ContainsKey(item);
    }

    public bool AddDenyListItem(Name ownerName, Name item)
    {
        var oldCount = _denyList.Count;
        _denyList.GetOrAdd(item, _ => []).AddUnique(ownerName);

        var filterChanged = oldCount != _denyList.Count;
        if (filterChanged && !_suppressOnFilterChanged)
            OnFilterChanged?.Invoke();

        return filterChanged;
    }

    public bool AddAllowListItem(Name ownerName, Name item)
    {
        var oldCount = _allowList.Count;
        _allowList.GetOrAdd(item, _ => []).AddUnique(ownerName);

        var filterChanged = oldCount != _allowList.Count;
        if (filterChanged && !_suppressOnFilterChanged)
            OnFilterChanged?.Invoke();

        return filterChanged;
    }

    public bool RemoveDenyListItem(Name ownerName, Name itemName)
    {
        if (!_denyList.TryGetValue(itemName, out var owners))
            return false;

        if (!owners.Remove(ownerName))
            return false;

        if (owners.Count == 0)
        {
            _denyList.Remove(itemName);
        }
        if (!_suppressOnFilterChanged)
            OnFilterChanged?.Invoke();
        return true;
    }

    public bool RemoveAllowListItem(Name ownerName, Name itemName)
    {
        if (!_allowList.TryGetValue(itemName, out var owners))
            return false;

        if (!owners.Remove(ownerName))
            return false;

        if (owners.Count == 0)
        {
            _allowList.Remove(itemName);
        }
        if (!_suppressOnFilterChanged)
            OnFilterChanged?.Invoke();
        return true;
    }

    public bool AddDenyListAll(Name ownerName)
    {
        var filterChanged = _denyAllList.AddUnique(ownerName);
        if (filterChanged && !_suppressOnFilterChanged)
            OnFilterChanged?.Invoke();
        return filterChanged;
    }

    public bool UnregisterOwner(Name ownerName)
    {
        var filterChanged = false;

        var keysToRemove = new List<Name>();
        foreach (var (key, owners) in _denyList)
        {
            owners.Remove(ownerName);
            if (owners.Count != 0)
                continue;
            keysToRemove.Add(key);
            filterChanged = true;
        }

        foreach (var key in keysToRemove)
            _denyList.Remove(key);

        keysToRemove.Clear();

        foreach (var (key, owners) in _allowList)
        {
            owners.Remove(ownerName);
            if (owners.Count != 0)
                continue;
            keysToRemove.Add(key);
            filterChanged = true;
        }

        foreach (var key in keysToRemove)
            _allowList.Remove(key);

        filterChanged |= _denyAllList.Remove(ownerName);

        if (filterChanged && !_suppressOnFilterChanged)
        {
            OnFilterChanged?.Invoke();
        }

        return filterChanged;
    }

    public bool UnregisterOwners(ReadOnlySpan<Name> ownerNames)
    {
        var filterChanged = true;
        using (GuardValue.Create(ref _suppressOnFilterChanged, true))
        {
            foreach (var ownerName in ownerNames)
            {
                filterChanged |= UnregisterOwner(ownerName);
            }
        }

        if (filterChanged && !_suppressOnFilterChanged)
        {
            OnFilterChanged?.Invoke();
        }

        return filterChanged;
    }

    public bool Append(NamePermissionList other)
    {
        var filterChanged = true;
        using (GuardValue.Create(ref _suppressOnFilterChanged, true))
        {
            foreach (var (key, value) in other._denyList)
            {
                filterChanged = value.Aggregate(
                    filterChanged,
                    (current, ownerName) => current | AddDenyListItem(ownerName, key)
                );
            }

            foreach (var (key, value) in other._allowList)
            {
                filterChanged = value.Aggregate(
                    filterChanged,
                    (current, ownerName) => current | AddAllowListItem(ownerName, key)
                );
            }

            filterChanged = other._denyAllList.Aggregate(
                filterChanged,
                (current, ownerName) => current | AddDenyListAll(ownerName)
            );
        }

        if (filterChanged && !_suppressOnFilterChanged)
        {
            OnFilterChanged?.Invoke();
        }

        return filterChanged;
    }

    public bool UnregisterOwnersAndAppend(ReadOnlySpan<Name> ownerNames, NamePermissionList other)
    {
        var filterChanged = false;
        using (GuardValue.Create(ref _suppressOnFilterChanged, true))
        {
            filterChanged |= UnregisterOwners(ownerNames);
            filterChanged |= Append(other);
        }

        if (filterChanged && !_suppressOnFilterChanged)
        {
            OnFilterChanged?.Invoke();
        }

        return filterChanged;
    }
}
