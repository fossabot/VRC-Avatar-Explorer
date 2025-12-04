using System;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class Category : ISelectableItem
{
    public ItemType Type { get; private set; } = ItemType.None;
    public string CustomCategory { get; private set; } = string.Empty;
    public int CategoryItemCount { get; set; } = 0;

    #region Constructor
    public Category()
    {
    }

    public Category(Category category)
    {
        Type = category.Type;
        CustomCategory = category.CustomCategory;
    }

    public Category(ItemType itemType)
    {
        Type = itemType;
        CustomCategory = string.Empty;
    }

    public Category(string customCategory)
    {
        Type = ItemType.Custom;
        CustomCategory = customCategory;
    }
    #endregion

    public void SetCategory(Category category)
    {
        Type = category.Type;
        CustomCategory = category.CustomCategory;
    }

    public void SetCategory(ItemType type, string customCategory = "")
    {
        Type = string.IsNullOrEmpty(customCategory) ? type : ItemType.Custom;
        CustomCategory = customCategory;
    }
    
    public void SetCategory(string customCategory)
    {
        Type = ItemType.Custom;
        CustomCategory = customCategory;
    }

    public string GetCategoryName()
    {
        if (Type == ItemType.Custom) return CustomCategory;
        return Type.ToString();
    }

    public bool IsEmpty() => Type == ItemType.None && CustomCategory == string.Empty;

    public void SetEmpty()
    {
        Type = ItemType.None;
        CustomCategory = string.Empty;
    }

    public string GetTitle()
    {
        if (Type == ItemType.Custom)
        {
            return CustomCategory;
        }
        else
        {
            return Type.ToString();
        }
    }

    public string GetDescription()
        => string.Format("{0}個の項目", CategoryItemCount);

    public string GetImagePath()
    {
        return "System.Icon.Folder";
    }

    public string CustomTagType { get; set; } = string.Empty;
    public ItemTagInfo GetTag()
    {
        return new ItemTagInfo(string.IsNullOrEmpty(CustomTagType) ? "Item.Category" : CustomTagType, Type.GetInternalId() ?? CustomCategory);
    }
    
    public IconType IconType { get; set; } = IconType.Item;
}
