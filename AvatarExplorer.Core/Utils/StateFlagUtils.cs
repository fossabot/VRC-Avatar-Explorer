using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class StateFlagUtils
{
    public static readonly ItemTagState ItemsFlag = ItemTagState.SearchItem | ItemTagState.RootAvatar | ItemTagState.RootSelectedItem;
    public static readonly ItemTagState CategoriesFlag = ItemTagState.RootCategory | ItemTagState.RootSelectedCategory | ItemTagState.ItemFileCategory;
}
