using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Models;

public class UISelectableItem
{
    public string Title { get; private set; } = string.Empty;
    public (string internalId, string[] args) Description { get; set; } = new();
    public string ImageFileName { get; private set; } = string.Empty;
    public ItemTagInfo Tag { get; private set; } = new(); // 選択されたときに使用されるタグ
    public IconType IconType { get; private set; } = IconType.None;

    public string CommonAvatarName { get; set; } // アイテム表記用
    public int ItemCount { get; set; } = 0; // カテゴリなどの数表記用

    public UISelectableItem(ISelectableItem source, int itemCount, string commonAvatarName = "")
    {
        ItemCount = itemCount;
        CommonAvatarName = commonAvatarName;

        if (source is Item item) FromItem(item);
        else if (source is Author author) FromAuthor(author);
        else if (source is Category category) FromCategory(category);
        else if (source is FileCategoryItem fileCategoryItem) FromFileCategoryItem(fileCategoryItem);
        else if (source is ItemFile itemFile) FromFileItemFile(itemFile);
    }

    public UISelectableItem(ItemCountInfo itemCountInfo)
        : this(itemCountInfo.Item, itemCountInfo.Count)
    {
    }

    public UISelectableItem SetType(string type)
    {
        Tag = new ItemTagInfo(type, Tag.Value);
        return this;
    }

    private void FromItem(Item item)
    {
        Title = item.Title;

        if (!string.IsNullOrEmpty(CommonAvatarName) && item.Tags.Count > 0)
        {
            Description = ("Button.Description.Item.AuthorName.CommonAvatar.Tags", [item.Author, item.CustomCategory, string.Join(", ", item.Tags)]);
        }
        else if (!string.IsNullOrEmpty(CommonAvatarName))
        {
            Description = ("Button.Description.Item.AuthorName.CommonAvatar", [item.Author, item.CustomCategory]);
        } else
        {
            Description = ("Button.Description.Item.AuthorName", [item.Author]);
        }

        ImageFileName = item.ThumbnmailFileName;
        Tag = new(ItemTagState.Item, item.ItemPath);
        IconType = IconType.Item;
    }

    private void FromAuthor(Author author)
    {
        Title = author.Name;
        Description = ("Button.Description.Item.Count", [ ItemCount.ToString() ]);
        ImageFileName = author.AuthorThumbnailFileName;
        Tag = new(ItemTagState.Author, author.Name);
        IconType = IconType.Author;
    }

    private void FromCategory(Category category)
    {
        Title = category.ToString();
        Description = ("Button.Description.Item.Count", [ ItemCount.ToString() ]);
        ImageFileName = SystemIcon.FolderIcon;
        Tag = new(ItemTagState.ItemCategory, category.Type.GetInternalId() ?? category.CustomCategory);
        IconType = IconType.Author;
    }

    private void FromFileCategoryItem(FileCategoryItem fileCategoryItem)
    {
        Title = fileCategoryItem.FileCategory.GetInternalId() ?? "";
        Description = ("Button.Description.Item.Count", [ ItemCount.ToString() ]);
        ImageFileName = SystemIcon.FolderIcon;
        Tag = new(ItemTagState.ItemFileCategory, fileCategoryItem.FileCategory.GetInternalId() ?? "");
        IconType = IconType.Author;
    }

    private void FromFileItemFile(ItemFile itemFile)
    {
        Title = itemFile.FileName;
        Description = ("Button.Description.File.Extension", [ itemFile.Extension ]);
        ImageFileName = SystemIcon.FileIcon;
        Tag = new(ItemTagState.ItemFileCategoryOpen, itemFile.FullPath);
        IconType = IconType.Author;
    }
}
