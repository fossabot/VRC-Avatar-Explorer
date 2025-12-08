using System;
using System.Collections.Generic;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Models.OverlayValues;

internal class AddItemOverlayWindowValues
{
    internal List<string> Folders { get; set; } = new(); // 新規作成のときにしか使わない
    internal string MaterialFolder { get; set; } = string.Empty; // 新規作成のときにしか使わない
    internal string BoothTitle { get; set; } = string.Empty;
    internal string BoothAuthor { get; set; } = string.Empty;
    internal string BoothAuthorId { get; set; } = string.Empty; // 内部の値
    internal string BoothThumbnailUrl { get; set; } = string.Empty; // 内部の値
    internal int BoothId { get; set; } = -1; // 内部の値
    internal ItemType ItemType { get; set; } = ItemType.None;
    internal List<string> SupportedAvatars { get; set; } = new();

    internal void Reset()
    {
        Folders.Clear();
        MaterialFolder = string.Empty;
        BoothTitle = string.Empty;
        BoothAuthor = string.Empty;
        BoothAuthorId = string.Empty;
        BoothThumbnailUrl = string.Empty;
        BoothId = -1;
        ItemType = ItemType.None;
        SupportedAvatars.Clear();
    }

    internal void FromItem(Item item)
    {
        BoothTitle = item.Title;
        BoothAuthor = item.Author;
        BoothAuthorId = item.AuthorId;
        BoothThumbnailUrl = string.Empty;
        BoothId = item.BoothId;
        ItemType = item.Type;

        SupportedAvatars.Clear();
        SupportedAvatars.AddRange(item.SupportedAvatars);
    }
}
