using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Models.OverlayValues;

internal class AddItemOverlayWindowValues
{
    internal List<string> Folders { get; set; } = new(); // 新規作成のときにしか使わない
    internal string Title { get; set; } = string.Empty;
    internal string Author { get; set; } = string.Empty;
    internal string BoothAuthorId { get; set; } = string.Empty;
    internal string BoothThumbnailUrl { get; set; } = string.Empty;
    internal string BoothAuthorThumbnailUrl { get; set; } = string.Empty;
    internal int BoothId { get; set; } = -1;
    internal ItemType ItemType { get; set; } = ItemType.Avatar;
    private List<string> SupportedAvatars { get; set; } = new();
    internal IReadOnlyList<string> SupportedAvatarsView => SupportedAvatars;

    internal void Reset()
    {
        Folders.Clear();
        Title = string.Empty;
        Author = string.Empty;
        BoothAuthorId = string.Empty;
        BoothThumbnailUrl = string.Empty;
        BoothAuthorThumbnailUrl = string.Empty;
        BoothId = -1;
        ItemType = ItemType.Avatar;
        SupportedAvatars.Clear();
    }

    private void UpdateSupportedAvatars(IEnumerable<string> newList) => SupportedAvatars = newList.ToList();

    internal void FromItem(Item item)
    {
        Title = item.Title;
        Author = item.Author;
        BoothAuthorId = item.AuthorId;
        BoothThumbnailUrl = string.Empty;
        BoothAuthorThumbnailUrl = string.Empty;
        BoothId = item.BoothId;
        ItemType = item.Type;

        UpdateSupportedAvatars(item.SupportedAvatarsView);
    }

    internal (bool, string) Validate()
    {
        if (Folders.Count == 0) return (false, LocalizationKey.Error.Validation.NoFolders);
        if (string.IsNullOrEmpty(Title)) return (false, LocalizationKey.Error.Validation.EmptyTitle);
        if (string.IsNullOrEmpty(Author)) return (false, LocalizationKey.Error.Validation.EmptyAuthor);

        return (true, string.Empty);
    }
}
