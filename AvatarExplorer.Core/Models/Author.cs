using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class Author : ISelectableItem
{
    /// <summary>
    /// アイテムの作者の名前を取得または設定します。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者の画像パスを取得または設定します。
    /// </summary>
    public string AuthorThumbnailFileName { get; set; } = string.Empty;

    public int AuthorItemCount { get; set; } = 0;

    #region ISelectableItem
    public string GetTitle() => Name;
    public (string internalId, string[] args) GetDescription() => ("Button.Description.Item.Count", [ AuthorItemCount.ToString() ]);
    public string GetImageFileName() => AuthorThumbnailFileName;
    public string CustomTagType { get; set; } = string.Empty;
    public ItemTagInfo GetTag() => new ItemTagInfo(string.IsNullOrEmpty(CustomTagType) ? "Author" : CustomTagType, Name);
    public IconType IconType { get; set; } = IconType.Author;
    public string InternalId { get; set; } = "";
    #endregion
}
