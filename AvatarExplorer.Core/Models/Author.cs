using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class Author : ISelectableItem
{
    public string Name { get; set; } = string.Empty;
    public string AuthorThumbnailFileName { get; set; } = string.Empty;
}
