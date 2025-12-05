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
        return new Item()
        {
            Title = item.Title,
            Author = item.AuthorName,
            AuthorId = item.AuthorId,
            BoothId = item.BoothId,
            ItemPath = item.ItemPath,
            MaterialPath = item.MaterialPath,
            ThumbnmailFileName = item.ImagePath.Replace("Datas\\Thumbnail\\", ""),
            AuthorThumbnmailFileName = item.AuthorImageFilePath.Replace("Datas\\AuthorImage\\", ""),
            Type = item.Type,
            CustomCategory = item.CustomCategory,
            SupportedAvatars = new List<string>(item.SupportedAvatar),
            ImplementedAvatars = new List<string>(item.ImplementedAvatars),
            Tags = new List<string>(item.Tags),
            ItemMemo = item.ItemMemo,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate,
        };
    }
}
