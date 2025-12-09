using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public class ItemCreationContext
{
    public List<string> Folders { get; set; } = new();
    public string MaterialFolder { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int BoothId { get; set; } = -1;
    public ItemType ItemType { get; set; } = ItemType.Avatar;
    public string CustomCategory { get; set; } = string.Empty;
    public string LocalizedCategoryName { get; set; } = string.Empty;
    public List<string> SupportedAvatars { get; set; } = new();

    public string? GetSafeTitle()
    {

        var safeTitle = Title;
        foreach (var invalidChar in FileSystemUtils.InvalidChars)
        {
            safeTitle = safeTitle.Replace(invalidChar, '_');
        }

        return string.IsNullOrEmpty(safeTitle) ? null : safeTitle;
    }
}
