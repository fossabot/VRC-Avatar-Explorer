namespace AvatarExplorer.Core.Models;

internal class ItemFileCategoryDefinition
{
    internal FileCategory FileCategory { get; set; } = FileCategory.None;
    internal string[]? ExtensionFilters { get; set; } = null;
    internal string[]? FilenameFilters { get; set; } = null;
    internal FileCategoryItem Item { get; set; } = new();
}
