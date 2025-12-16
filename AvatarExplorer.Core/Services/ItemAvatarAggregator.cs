using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class ItemAvatarAggregator
{
    internal static IReadOnlyList<ItemCountInfo> Aggregate(IReadOnlyList<Item> items, RuntimeSettings runtimeSettings)
    {
        return items
            .Where(i => i.Type == ItemType.Avatar)
            .GetSortedItems(runtimeSettings)
            .Select(i => new ItemCountInfo(i, 0))
            .ToList();
    }
}
