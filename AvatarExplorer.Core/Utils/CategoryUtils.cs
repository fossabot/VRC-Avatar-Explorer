using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class CategoryUtils
{
    public static readonly ItemType[] InvalidItemTypes = [ItemType.None, ItemType.Unknown];

    public static IReadOnlyList<Category> GetCategories(IReadOnlyList<Item> items)
    {
        var categories = new List<Category>();

        categories.AddRange(
            Enum.GetValues<ItemType>()
                .Where(i => !InvalidItemTypes.Contains(i) && i != ItemType.Custom)
                .Select(i => new Category(i)
                {
                    CategoryItemCount = items.Count(item => item.Type == i)
                })
        );

        categories.AddRange(
            items
                .Select(i => i.CustomCategory)
                .Where(i => i != string.Empty)
                .Distinct()
                .Select(i => new Category(i)
                {
                    CategoryItemCount = items.Count(item => item.CustomCategory == i)
                })
        );

        return categories;
    }
}
