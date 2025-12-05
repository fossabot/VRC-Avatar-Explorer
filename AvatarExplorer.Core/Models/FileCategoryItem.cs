using System;
using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class FileCategoryItem : ISelectableItem
{
    public FileCategory FileCategory { get; set; } = FileCategory.None;
    public List<string> FilePaths { get; set; } = new();
}
