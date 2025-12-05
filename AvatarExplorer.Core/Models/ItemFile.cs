using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

public class ItemFile(string filePath) : ISelectableItem
{
    public string FullPath { get; } = filePath;
    public string FileName { get; } = Path.GetFileName(filePath);
    public string Extension { get; } = Path.GetExtension(filePath)[1.. ].ToUpper();
}
