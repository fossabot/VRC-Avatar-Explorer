using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class FileCategoryItem(FileCategory fileCategory = FileCategory.None) : ISelectableItem
{
    public FileCategory FileCategory { get; set; } = fileCategory;
    public List<string> FilePaths { get; } = new List<string>();
}
