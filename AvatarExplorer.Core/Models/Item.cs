using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Models;

/// <summary>
/// アイテム情報を表します。
/// </summary>
public class Item : ISelectableItem
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public int BoothId { get; set; } = -1;
    public string ItemPath { get; set; } = string.Empty;
    public string MaterialPath { get; set; } = string.Empty;
    public string ThumbnmailFileName { get; set; } = string.Empty;
    public string AuthorThumbnmailFileName { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public string CustomCategory { get; set; } = string.Empty;
    public List<string> SupportedAvatars { get; set; } = new List<string>();
    public List<string> ImplementedAvatars { get; set; } = new List<string>();
    public List<string> Tags { get; set; } = new List<string>();
    public string ItemMemo { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string UpdatedDate { get; set; } = string.Empty;

    public string GetBoothLink()
        => string.Format(BoothLink.ItemURLFormat, AuthorId, BoothId);

    public string GetBoothJsonLink()
        => string.Format(BoothLink.ItemJsonURLFormat, BoothId);

    [JsonIgnore]
    internal string SearchIndex { get; private set; } = string.Empty;

    internal void BuildSearchIndex(Dictionary<string, string> avatarMap)
    {
        IEnumerable<string> avatars = SupportedAvatars
            .Concat(ImplementedAvatars)
            .Select(a => ItemUtils.GetAvatarNameFromDictionary(avatarMap, a))
            .Where(name => !string.IsNullOrEmpty(name));

        SearchIndex = string.Join("\n",
            Title,
            Author,
            ItemMemo,
            BoothId.ToString(),
            string.Join(" ", Tags),
            string.Join(" ", avatars)
        ).ToLowerInvariant();
    }
    
    internal Item SetValuesFromCreationContext(ItemCreationContext itemCreationContext)
    {
        Title = itemCreationContext.Title;
        Author = itemCreationContext.Author;
        AuthorId = itemCreationContext.AuthorId;
        BoothId = itemCreationContext.BoothId;
        Type = itemCreationContext.ItemType;
        CustomCategory = itemCreationContext.CustomCategory;

        SupportedAvatars.Clear();
        SupportedAvatars.AddRange(itemCreationContext.SupportedAvatars);

        return this;
    }

    internal static Item FromV1(ItemV1 item)
    {
        Item migratedItem = new()
        {
            Title = item.Title,
            Author = item.AuthorName,
            AuthorId = item.AuthorId,
            BoothId = item.BoothId,
            ItemPath = MigrateUtils.MigrateItemPath(item.ItemPath),
            MaterialPath = MigrateUtils.MigrateItemPath(item.MaterialPath),
            ThumbnmailFileName = MigrateUtils.MigrateItemPath(item.ImagePath),
            AuthorThumbnmailFileName = MigrateUtils.MigrateItemPath(item.AuthorImageFilePath),
            Type = item.Type,
            CustomCategory = item.CustomCategory,
            ItemMemo = item.ItemMemo,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate,
        };

        migratedItem.SupportedAvatars.AddRange(item.SupportedAvatar);
        migratedItem.ImplementedAvatars.AddRange(item.ImplementedAvatars);
        migratedItem.Tags.AddRange(item.Tags);

        MigrateUtils.MigrateItemPaths(migratedItem.SupportedAvatars);
        MigrateUtils.MigrateItemPaths(migratedItem.ImplementedAvatars);

        return migratedItem;
    }
}
