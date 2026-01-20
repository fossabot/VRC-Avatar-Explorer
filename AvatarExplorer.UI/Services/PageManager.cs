using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Services;

internal class PageManager
{
    private readonly Dictionary<ItemTagState, int> _currentPageStates = new()
    {
        { ItemTagState.SearchItem, 0 },
        { ItemTagState.RootAvatar, 0 },
        { ItemTagState.RootAuthor, 0 },
        { ItemTagState.RootCategory, 0 },
        { ItemTagState.RootSelectedCategory, 0 },
        { ItemTagState.RootSelectedItem, 0 },
        { ItemTagState.ItemFileCategoryOpen, 0 }
    };
    
    internal bool IsPageSupported(ItemTagState itemTagState) => _currentPageStates.ContainsKey(itemTagState);

    internal int GetPage(ItemTagState itemTagState) => IsPageSupported(itemTagState) ? _currentPageStates[itemTagState] : -1;
    internal void SetPage(ItemTagState itemTagState, int value)
    {
        if (!IsPageSupported(itemTagState)) return;
        _currentPageStates[itemTagState] = value;
    }

    internal void ResetPageValue(ItemTagState itemTagState)
    {
        if (!IsPageSupported(itemTagState)) return;
        SetPage(itemTagState, 0);
    }
    internal void ResetAllPageValues()
    {
        foreach (ItemTagState key in GetKeys())
            ResetPageValue(key);
    }

    internal ItemTagState[] GetKeys() => _currentPageStates.Keys.ToArray();
}
