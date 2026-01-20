using System.Collections.Generic;
using System.Linq;
using Avalonia;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Services;

internal class ScrollManager
{
    private static readonly Vector Empty = new();
    private readonly Dictionary<ItemTagState, Vector> _currentScrollValues = new()
    {
        { ItemTagState.SearchItem, Empty },
        { ItemTagState.RootAvatar, Empty },
        { ItemTagState.RootAuthor, Empty },
        { ItemTagState.RootCategory, Empty },
        { ItemTagState.RootSelectedCategory, Empty },
        { ItemTagState.RootSelectedItem, Empty },
        { ItemTagState.ItemFileCategory, Empty },
        { ItemTagState.ItemFileCategoryOpen, Empty }
    };

    internal bool IsScrollSupported(ItemTagState itemTagState) => _currentScrollValues.ContainsKey(itemTagState);

    internal Vector GetScrollValue(ItemTagState itemTagState) => IsScrollSupported(itemTagState) ? _currentScrollValues[itemTagState] : new();
    internal void SetScroll(ItemTagState itemTagState, Vector value)
    {
        if (!IsScrollSupported(itemTagState)) return;
        _currentScrollValues[itemTagState] = value;
    }

    internal void ResetScrollValue(ItemTagState itemTagState)
    {
        if (!IsScrollSupported(itemTagState)) return;
        SetScroll(itemTagState, Empty);
    }
    internal void ResetAllScrollValues()
    {
        foreach (ItemTagState key in GetKeys())
            ResetScrollValue(key);
    }

    internal ItemTagState[] GetKeys() => _currentScrollValues.Keys.ToArray();
}
