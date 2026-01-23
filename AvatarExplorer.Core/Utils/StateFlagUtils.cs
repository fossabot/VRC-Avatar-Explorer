using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class StateFlagUtils
{
    public static readonly ItemTagState ItemFlags = ItemTagState.SearchItem | ItemTagState.RootAvatar | ItemTagState.RootSelectedItem;
    public static readonly ItemTagState DraggableFlags = ItemTagState.SearchItem | ItemTagState.RootAvatar | ItemTagState.RootSelectedItem | ItemTagState.ItemFileCategoryOpen;
    public static readonly ItemTagState CategoryFlags = ItemTagState.RootCategory | ItemTagState.RootSelectedCategory | ItemTagState.ItemFileCategory;

    public static bool IsItemState(ItemTagState itemTagState) => itemTagState != ItemTagState.None && ItemFlags.HasFlag(itemTagState);
    public static bool IsCategoryState(ItemTagState itemTagState) => itemTagState != ItemTagState.None && CategoryFlags.HasFlag(itemTagState);
    public static bool IsDraggableState(ItemTagState itemTagState) => itemTagState != ItemTagState.None && DraggableFlags.HasFlag(itemTagState);
}
