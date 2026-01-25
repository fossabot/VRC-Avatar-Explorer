using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

internal static class ItemCategoryAggregator
{
    internal static IReadOnlyList<ItemCountInfo> Aggregate(IEnumerable<Item> items)
    {
        List<ItemCountInfo> categories = new();

        Dictionary<ItemType, int> itemsByType = items
            .GroupBy(i => i.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        Dictionary<string, int> itemsByCustomCategory = items
            .Where(i => !string.IsNullOrEmpty(i.CustomCategory))
            .GroupBy(i => i.CustomCategory)
            .ToDictionary(g => g.Key, g => g.Count());

        categories.AddRange(
            Enum.GetValues<ItemType>()
                .Where(i => !CategoryUtils.InvalidItemTypes.Contains(i) && i != ItemType.Custom)
                .Where(itemsByType.ContainsKey)
                .Select(i => new ItemCountInfo(new Category(i), itemsByType[i]))
        );

        categories.AddRange(
            items
                .Select(i => i.CustomCategory)
                .Where(i => !string.IsNullOrEmpty(i))
                .Distinct()
                .Where(itemsByCustomCategory.ContainsKey)
                .Select(i => new ItemCountInfo(new Category(i), itemsByCustomCategory[i]))
        );

        return categories;
    }
}
