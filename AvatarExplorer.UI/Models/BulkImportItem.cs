namespace AvatarExplorer.UI.Models;

internal class BulkImportItem(string itemPath)
{
    internal string ItemPath { get; init; } = itemPath;
    internal int SelectedIndex { get; set; } = 0;
}
