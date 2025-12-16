using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class ItemAuthorAggregator
{
    internal static IReadOnlyList<ItemCountInfo> Aggregate(IReadOnlyList<Item> items)
    {
        return items
            .GroupBy(item => new { item.Author, item.AuthorThumbnmailFileName })
            .Select(g => new ItemCountInfo(
                new Author
                {
                    Name = g.Key.Author,
                    AuthorThumbnailFileName = g.Key.AuthorThumbnmailFileName
                },
                g.Count()
            ))
            .ToList();
    }
}
