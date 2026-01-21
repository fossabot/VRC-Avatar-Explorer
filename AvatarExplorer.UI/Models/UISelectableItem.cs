using System.Collections.Generic;
using System.Linq;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Models;

internal class UISelectableItem
{
    internal string Title { get; private set; } = string.Empty;
    internal (string LocalizationKey, string[] Args) Description { get; set; } = new();
    internal string ImageFileName { get; private set; } = string.Empty;
    internal ItemTagInfo Tag { get; private set; } = new(); // ボタンが選択されたときに使用されるタグ
    internal IconType IconType { get; private set; } = IconType.None;

    internal int ItemCount { get; set; } = 0; // カテゴリなどの数表記用

    internal string CommonAvatarName { get; set; } // アイテム表記用
    internal string CreatedDate { get; set; } = string.Empty; // アイテムTooltip表記用
    internal string UpdatedDate { get; set; } = string.Empty; // アイテムTooltip表記用
    private List<string> ItemTags { get; set; } = new(); // アイテムのタグ
    internal IReadOnlyList<string> ItemTagsView => ItemTags;
    internal string ItemMemo { get; set; } = string.Empty; // アイテムTooltip表記用

    internal UISelectableItem(ISelectableItem source, int itemCount, string commonAvatarName = "")
    {
        ItemCount = itemCount;
        CommonAvatarName = commonAvatarName;

        if (source is Item item) FromItem(item);
        else if (source is Author author) FromAuthor(author);
        else if (source is Category category) FromCategory(category);
        else if (source is FileCategoryItem fileCategoryItem) FromFileCategoryItem(fileCategoryItem);
        else if (source is ItemFile itemFile) FromFileItemFile(itemFile);
        else if (source is CommonAvatar commonAvatar) FromCommonAvatar(commonAvatar);
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
        Description = (LocalizationKey.UI.Button.Description.Item.Author, [item.Author]);
        ImageFileName = item.ThumbnmailFileName;
        Tag = new(ItemTagState.RootSelectedItem, item.ItemPath);
        IconType = IconType.Item;

        CreatedDate = DatetimeUtils.GetDateStringFromUnixTime(item.CreatedDate);
        UpdatedDate = DatetimeUtils.GetDateStringFromUnixTime(item.UpdatedDate);
        
        ItemTags = item.TagsView.ToList();
        
        ItemMemo = item.ItemMemo;
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
        ImageFileName = SystemIconKey.FolderIcon;
        
        Tag = new(ItemTagState.RootSelectedCategory, category.Type.GetLocalizationKey() ?? category.CustomCategory);
        IconType = IconType.None;
    }

    private void FromFileCategoryItem(FileCategoryItem fileCategoryItem)
    {
        Title = fileCategoryItem.FileCategory.GetLocalizationKey() ?? string.Empty;
        Description = (LocalizationKey.UI.Button.Description.Item.Count, [ItemCount.ToString()]);
        ImageFileName = SystemIconKey.FolderIcon;
        Tag = new(ItemTagState.ItemFileCategory, fileCategoryItem.FileCategory.GetLocalizationKey() ?? string.Empty);
        IconType = IconType.None;
    }

    private void FromFileItemFile(ItemFile itemFile)
    {
        Title = itemFile.FileName;
        Description = (LocalizationKey.UI.Button.Description.File.Extension, [itemFile.Extension]);
        ImageFileName = SystemIconKey.FileIcon;
        Tag = new(ItemTagState.ItemFileCategoryOpen, itemFile.FullPath);
        IconType = IconType.None;
    }

    private void FromCommonAvatar(CommonAvatar commonAvatar)
    {
        Title = Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.Tag.CommonAvatar, commonAvatar.GroupName);
        Description = (LocalizationKey.UI.Button.Description.CommonAvatar.Count, [commonAvatar.AvatarsView.Count.ToString()]);
        ImageFileName = SystemIconKey.GroupIcon;
        Tag = new(ItemTagState.None, commonAvatar.GetInternalPath());
        IconType = IconType.None;
    }
}
