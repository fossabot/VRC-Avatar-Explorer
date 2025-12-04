using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models.V1;

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
    public string AuthorId { get; set; } = string.Empty;

    public int AuthorItemCount { get; set; } = 0;

    public string GetTitle()
    {
        return Name;
    }
    public string GetDescription()
    {
        return string.Format("{0}個の項目", AuthorItemCount);
    }
        
    public string GetImagePath()
    {
        return AuthorId;
    }

    public string CustomTagType { get; set; } = string.Empty;
    public ItemTagInfo GetTag()
    {
        return new ItemTagInfo(string.IsNullOrEmpty(CustomTagType) ? "Author" : CustomTagType, Name);
    }
}
