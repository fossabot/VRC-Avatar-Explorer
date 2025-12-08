using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Extensions;

internal static class ItemExtensions
{
    internal static IEnumerable<Item> GetSortedItems(this IEnumerable<Item> items, SortOrder sortOrder)
    {
        return sortOrder switch
        {
            SortOrder.Title => items.OrderBy(item => item.Title),
            SortOrder.Author => items.OrderBy(item => item.Author),
            SortOrder.Created => items.OrderByDescending(item => item.CreatedDate),
            SortOrder.Updated => items.OrderByDescending(item => item.UpdatedDate),
            _ => items.OrderBy(item => item.Title)
        };
    }

    internal static IEnumerable<ItemCountInfo> GetSortedItemsFromCountInfo(this IEnumerable<ItemCountInfo> itemCountInfos, SortOrder sortOrder)
    {
        if (itemCountInfos.Any(i => i.Item is not Item)) return itemCountInfos;

        return sortOrder switch
        {
            SortOrder.Title => itemCountInfos.OrderBy(i => ((Item)i.Item).Title),
            SortOrder.Author => itemCountInfos.OrderBy(i => ((Item)i.Item).Author),
            SortOrder.Created => itemCountInfos.OrderByDescending(i => ((Item)i.Item).CreatedDate),
            SortOrder.Updated => itemCountInfos.OrderByDescending(i => ((Item)i.Item).UpdatedDate),
            _ => itemCountInfos.OrderBy(i => ((Item)i.Item).Title)
        };
    }
}
