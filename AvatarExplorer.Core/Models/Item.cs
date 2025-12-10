using System.Text.Json.Serialization;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

/// <summary>
/// アイテム情報を表します。
/// </summary>
public class Item : ISelectableItem
{
    /// <summary>
    /// アイテムのタイトルを取得または設定します。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者の名前を取得また設定します。
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者のIDを取得または設定します。
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのBooth IDを取得または設定します。
    /// </summary>
    public int BoothId { get; set; } = -1;

    /// <summary>
    /// アイテムフォルダがあるパスを取得または設定します。
    /// `ItemUtils.GetItemPath()`でフルパスを取得できます。
    /// </summary>
    public string ItemPath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのマテリアル用のフォルダのパスを取得または設定します。
    /// </summary>
    public string MaterialPath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのサムネイルのファイルパスを取得または設定します。
    /// </summary>
    public string ThumbnmailFileName { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのサムネイルのファイルパスを取得または設定します。
    /// </summary>
    public string AuthorThumbnmailFileName { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのタイプを取得または設定します。
    /// </summary>
    public ItemType Type { get; set; }

    /// <summary>
    /// もしタイプがカスタムカテゴリだった場合の、そのカスタムカテゴリ名を取得または設定します。
    /// </summary>
    public string CustomCategory { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの対応アバターを取得また設定します。
    /// </summary>
    public List<string> SupportedAvatars { get; set; } = new List<string>();
    
    /// <summary>
    /// アイテムが実装済みかどうかを管理する配列を取得または設定します。
    /// </summary>
    public List<string> ImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// アイテムのタグを取得または設定します。
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// アイテムのメモを取得または設定します。
    /// </summary>
    public string ItemMemo { get; set; } = string.Empty;
    
    /// <summary>
    /// アイテムの作成日時を取得または設定します。
    /// </summary>
    public string CreatedDate { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの更新日時を取得または設定します。
    /// </summary>
    public string UpdatedDate { get; set; } = string.Empty;

    public string GetBoothLink()
    {
        return string.Format(BoothLink.ItemURLFormat, AuthorId, BoothId);
    }

    public string GetBoothJsonLink()
    {
        // 強制で日本のBoothにアクセスするJsonのURLです。
        // これは、カテゴリ名などを使って内部でアイテムカテゴリを判別するためです。
        return string.Format(BoothLink.ItemJsonURLFormat, BoothId);
    }

    [JsonIgnore]
    public string SearchIndex { get; private set; } = string.Empty;

    public void BuildSearchIndex(Dictionary<string, string> avatarMap)
    {
        var avatars = SupportedAvatars
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

    public static Item FromV1(ItemV1 item)
    {
        var migratedItem = new Item()
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

        migratedItem.SupportedAvatars.Clear();
        migratedItem.SupportedAvatars.AddRange(item.SupportedAvatar);

        migratedItem.ImplementedAvatars.Clear();
        migratedItem.ImplementedAvatars.AddRange(item.ImplementedAvatars);

        migratedItem.Tags.Clear();
        migratedItem.Tags.AddRange(item.Tags);

        MigrateUtils.MigrateItemPaths(migratedItem.SupportedAvatars);
        MigrateUtils.MigrateItemPaths(migratedItem.ImplementedAvatars);

        return migratedItem;
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

    internal async static Task<(Item? newItem, List<string> processingFailedPaths)> FromItemCreationContext(ItemCreationContext itemCreationContext, RuntimeSettings runtimeSettings)
    {
        string extractDestinationFolderPath = Path.Combine(runtimeSettings.DataRootDirectory, itemCreationContext.LocalizedCategoryName);
        var (itemPath, materialPath, processingFailedPaths) = await FileSystemUtils.ExtractItemFolders(itemCreationContext, runtimeSettings.DataRootDirectory, extractDestinationFolderPath, runtimeSettings.RemoveOriginal);
        
        if (string.IsNullOrEmpty(itemPath))
        {
            return (null, processingFailedPaths);
        }
        
        var newItem = new Item()
        {
            Title = itemCreationContext.Title,
            Author = itemCreationContext.Author,
            AuthorId = itemCreationContext.AuthorId,
            BoothId = itemCreationContext.BoothId,
            ItemPath = itemPath,
            MaterialPath = materialPath,
            Type = itemCreationContext.ItemType,
            CustomCategory = itemCreationContext.CustomCategory
        };

        newItem.SupportedAvatars.Clear();
        newItem.SupportedAvatars.AddRange(itemCreationContext.SupportedAvatars);

        return (newItem, processingFailedPaths);
    }
}
