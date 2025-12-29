using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class StateFlagUtils
{
    public static readonly ItemTagState ItemFlags = ItemTagState.SearchItem | ItemTagState.RootAvatar | ItemTagState.RootSelectedItem;
    public static readonly ItemTagState CategoryFlags = ItemTagState.RootCategory | ItemTagState.RootSelectedCategory | ItemTagState.ItemFileCategory;

    public static bool IsItemState(ItemTagState itemTagState) => ItemFlags.HasFlag(itemTagState);
    public static bool IsCategoryState(ItemTagState itemTagState) => CategoryFlags.HasFlag(itemTagState);
}
