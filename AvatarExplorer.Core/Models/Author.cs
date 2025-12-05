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
}
