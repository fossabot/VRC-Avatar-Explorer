using System.Collections.Generic;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Models;

internal class UISelectableItem
{
    internal string Title { get; private set; } = string.Empty;
    internal (string LocalizationKey, string[] Args) Description { get; set; } = new();
    internal string ImageFileName { get; private set; } = string.Empty;
    internal ItemTagInfo Tag { get; private set; } = new(); // ボタンが選択されたときに使用されるタグ
    internal IconType IconType { get; private set; } = IconType.None;

    internal List<string> ItemTags { get; private set; } = new(); // アイテムのタグ
    internal string CommonAvatarName { get; set; } // アイテム表記用
    internal int ItemCount { get; set; } = 0; // カテゴリなどの数表記用

    internal UISelectableItem(ISelectableItem source, int itemCount, string commonAvatarName = "")
    {
        ItemCount = itemCount;
        CommonAvatarName = commonAvatarName;

        if (source is Item item) FromItem(item);
        else if (source is Author author) FromAuthor(author);
        else if (source is Category category) FromCategory(category);
        else if (source is FileCategoryItem fileCategoryItem) FromFileCategoryItem(fileCategoryItem);
        else if (source is ItemFile itemFile) FromFileItemFile(itemFile);
    }

    internal UISelectableItem(ItemCountInfo itemCountInfo)
        : this(itemCountInfo.Item, itemCountInfo.Count, itemCountInfo.CommonAvatarName)
    {
    }

    internal UISelectableItem SetState(ItemTagState state)
    {
        Tag = new ItemTagInfo(state, Tag.Value);
        return this;
    }

    private void FromItem(Item item)
    {
        Title = item.Title;

        if (!string.IsNullOrEmpty(CommonAvatarName))
        {
            Description = (LocalizationKey.UI.Button.Description.Item.Author.WithAvatar, [item.Author, CommonAvatarName]);
        }
        else
        {
            Description = (LocalizationKey.UI.Button.Description.Item.Author.Default, [item.Author]);
        }

        ImageFileName = item.ThumbnmailFileName;
        Tag = new(ItemTagState.RootSelectedItem, item.ItemPath);
        IconType = IconType.Item;

        ItemTags.Clear();
        ItemTags.AddRange(item.Tags);
    }

    private void FromAuthor(Author author)
    {
        Title = author.Name;
        Description = (LocalizationKey.UI.Button.Description.Item.Count, [ItemCount.ToString()]);
        ImageFileName = author.AuthorThumbnailFileName;

        Tag = new(ItemTagState.RootAuthor, author.Name);
        IconType = IconType.Author;
    }

    private void FromCategory(Category category)
    {
        Title = category.ToString();
        Description = (LocalizationKey.UI.Button.Description.Item.Count, [ItemCount.ToString()]);
        ImageFileName = SystemIcon.FolderIcon;
        
        Tag = new(ItemTagState.RootSelectedCategory, category.Type.GetLocalizationKey() ?? category.CustomCategory);
        IconType = IconType.Author;
    }

    private void FromFileCategoryItem(FileCategoryItem fileCategoryItem)
    {
        Title = fileCategoryItem.FileCategory.GetLocalizationKey() ?? "";
        Description = (LocalizationKey.UI.Button.Description.Item.Count, [ItemCount.ToString()]);
        ImageFileName = SystemIcon.FolderIcon;
        Tag = new(ItemTagState.ItemFileCategory, fileCategoryItem.FileCategory.GetLocalizationKey() ?? "");
        IconType = IconType.Author;
    }

    private void FromFileItemFile(ItemFile itemFile)
    {
        Title = itemFile.FileName;
        Description = (LocalizationKey.UI.Button.Description.File.Extension, [itemFile.Extension]);
        ImageFileName = SystemIcon.FileIcon;
        Tag = new(ItemTagState.ItemFileCategoryOpen, itemFile.FullPath);
        IconType = IconType.Author;
    }
}
