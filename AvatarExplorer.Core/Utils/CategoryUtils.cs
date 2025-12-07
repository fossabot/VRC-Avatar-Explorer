using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class CategoryUtils
{
    public static readonly ItemType[] InvalidItemTypes = [ItemType.None, ItemType.Unknown];

    public static IReadOnlyList<ItemCountInfo> GetCategories(IReadOnlyList<Item> items)
    {
        var categories = new List<ItemCountInfo>();

        categories.AddRange(
            Enum.GetValues<ItemType>()
                .Where(i => !InvalidItemTypes.Contains(i) && i != ItemType.Custom)
                .Select(i => new ItemCountInfo(new Category(i), items.Count(item => item.Type == i)))
        );

        categories.AddRange(
            items
                .Select(i => i.CustomCategory)
                .Where(i => i != string.Empty)
                .Distinct()
                .Select(i => new ItemCountInfo(new Category(i), items.Count(item => item.CustomCategory == i)))
        );

        return categories;
    }

    public static bool IsCategoryMatch(Item item, string category)
    {
        return (item.Type == ItemType.Custom && item.CustomCategory == category) || (item.Type.GetLocalizationKey() == category);
    }
}
