using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class Category : ISelectableItem
{
    public ItemType Type { get; private set; } = ItemType.None;
    public string CustomCategory { get; private set; } = string.Empty;
    
    public bool IsEmpty => Type == ItemType.None && CustomCategory == string.Empty;
    public string CategoryName => Type == ItemType.Custom ? CustomCategory : Type.ToString();
    public string LocalizationKey => Type == ItemType.Custom ? string.Empty : (Type.GetLocalizationKey() ?? Type.ToString());

    #region Constructor
    public Category()
    {
    }
    public Category(Category category)
    {
        Type = category.Type;
        CustomCategory = category.CustomCategory;
    }

    public Category(ItemType type, string customCategory = "")
    {
        Type = string.IsNullOrEmpty(customCategory) ? type : ItemType.Custom;
        CustomCategory = customCategory;
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

    public override string ToString() => Type == ItemType.Custom ? CustomCategory : Type.ToString();
}
