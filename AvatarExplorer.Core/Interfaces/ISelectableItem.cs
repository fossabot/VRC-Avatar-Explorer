using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Interfaces;

public interface ISelectableItem
{
    public string GetTitle();
    public (string internalId, string[] args) GetDescription();
    public string GetImageFileName();

    public string CustomTagType { get; set; }
    public ItemTagInfo GetTag();
    public IconType IconType { get; }
    public string InternalId { get; set; }
}
