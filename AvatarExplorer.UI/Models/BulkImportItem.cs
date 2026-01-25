namespace AvatarExplorer.UI.Models;

internal class BulkImportItem(string itemId)
{
    internal string ItemId { get; init; } = itemId;
    internal int SelectedIndex { get; set; } = 0;
}
