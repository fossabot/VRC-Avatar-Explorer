using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class ItemAuthorAggregator
{
    internal static IReadOnlyList<ItemCountInfo> Aggregate(IReadOnlyList<Item> items)
    {
        return items
            .GroupBy(i => new { i.Author, i.AuthorThumbnmailFileName })
            .Select(i => new ItemCountInfo(new Author { Name = i.Key.Author, AuthorThumbnailFileName = i.Key.AuthorThumbnmailFileName }, i.Count()))
            .ToList();
    }
}
