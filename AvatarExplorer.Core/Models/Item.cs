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
    public string ThumbnmailFileName { get; set; } = string.Empty;
    public string AuthorThumbnmailFileName { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public string CustomCategory { get; set; } = string.Empty;
    [JsonInclude] public List<string> SupportedAvatars { get; private set; } = new List<string>();
    [JsonInclude] public List<string> ImplementedAvatars { get; private set; } = new List<string>();
    [JsonInclude] public List<string> Tags { get; private set; } = new List<string>();
    public string ItemMemo { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string UpdatedDate { get; set; } = string.Empty;

    public string GetBoothLink()
        => string.Format(BoothLink.ItemURLFormat, AuthorId, BoothId);

    public string GetBoothJsonLink()
        => string.Format(BoothLink.ItemJsonURLFormat, BoothId);

    public void SetSupportedAvatars(IEnumerable<string> avatars, bool clear)
        => ListUtils.Add(SupportedAvatars, avatars, clear);

    public void SetImplementedAvatars(IEnumerable<string> avatars, bool clear)
        => ListUtils.Add(ImplementedAvatars, avatars, clear);

    public void SetTags(IEnumerable<string> tags, bool clear)
        => ListUtils.Add(Tags, tags, clear);

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

        SetSupportedAvatars(itemCreationContext.SupportedAvatars, true);

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
            ItemPath = item.ItemPath,
            ThumbnmailFileName = MigrateUtils.MigrateItemPath(item.ImagePath),
            AuthorThumbnmailFileName = MigrateUtils.MigrateItemPath(item.AuthorImageFilePath),
            Type = item.Type,
            CustomCategory = item.CustomCategory,
            ItemMemo = item.ItemMemo,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate,
        };

        migratedItem.SetSupportedAvatars(item.SupportedAvatar, true);
        migratedItem.SetImplementedAvatars(item.ImplementedAvatars, true);
        migratedItem.SetTags(item.Tags, true);

        return migratedItem;
    }
}
