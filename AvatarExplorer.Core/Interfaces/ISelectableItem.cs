using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Interfaces;

public interface ISelectableItem
{
    public string GetTitle();
    public string GetDescription();
    public string GetImagePath();

    public string CustomTagType { get; set; }
    public ItemTagInfo GetTag();
    public IconType IconType { get; }
}
