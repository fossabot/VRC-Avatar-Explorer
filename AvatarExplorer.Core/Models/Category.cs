using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class Category : ISelectableItem
{
    public ItemType Type { get; private set; } = ItemType.None;
    public string CustomCategory { get; private set; } = string.Empty;

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

    #region Set API
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
    #endregion

    public string GetCategoryName()
    {
        if (Type == ItemType.Custom) return CustomCategory;
        return Type.ToString();
    }

    public bool IsEmpty() => Type == ItemType.None && CustomCategory == string.Empty;
    public override string ToString() => Type == ItemType.Custom ? CustomCategory : (Type.GetLocalizationKey() ?? string.Empty);
}
