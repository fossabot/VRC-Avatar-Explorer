using System;
using System.Collections.Generic;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Models.OverlayValues;

internal class AddItemOverlayWindowValues
{
    internal List<string> Folders { get; set; } = new(); // 新規作成のときにしか使わない
    internal string MaterialFolder { get; set; } = string.Empty; // 新規作成のときにしか使わない
    internal string Title { get; set; } = string.Empty;
    internal string Author { get; set; } = string.Empty;
    internal string BoothAuthorId { get; set; } = string.Empty; // 内部の値
    internal string BoothThumbnailUrl { get; set; } = string.Empty; // 内部の値
    internal int BoothId { get; set; } = -1; // 内部の値
    internal ItemType ItemType { get; set; } = ItemType.Avatar;
    internal List<string> SupportedAvatars { get; set; } = new();

    internal void Reset()
    {
        Folders.Clear();
        MaterialFolder = string.Empty;
        Title = string.Empty;
        Author = string.Empty;
        BoothAuthorId = string.Empty;
        BoothThumbnailUrl = string.Empty;
        BoothId = -1;
        ItemType = ItemType.Avatar;
        SupportedAvatars.Clear();
    }

    internal void FromItem(Item item)
    {
        Title = item.Title;
        Author = item.Author;
        BoothAuthorId = item.AuthorId;
        BoothThumbnailUrl = string.Empty;
        BoothId = item.BoothId;
        ItemType = item.Type;

        SupportedAvatars.Clear();
        SupportedAvatars.AddRange(item.SupportedAvatars);
    }

    internal (bool, string) Validate()
    {
        if (Folders.Count == 0) return (false, LocalizationKey.Error.Validation.NoFolders);
        if (string.IsNullOrEmpty(Title)) return (false, LocalizationKey.Error.Validation.EmptyTitle);
        if (string.IsNullOrEmpty(Author)) return (false, LocalizationKey.Error.Validation.EmptyAuthor);

        return (true, string.Empty);
    }
}
