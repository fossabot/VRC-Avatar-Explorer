using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class CategoryUtils
{
    public static readonly ItemType[] InvalidItemTypes = [ItemType.None];

    public static bool IsCategoryMatch(Item item, string category)
    {
        return (item.Type == ItemType.Custom && item.CustomCategory == category) || (item.Type.GetLocalizationKey() == category);
    }
}
