namespace AvatarExplorer.UI.Models.Items;

internal class BulkImportItem(string itemId)
{
    internal string ItemId { get; init; } = itemId;
    internal int SelectedIndex { get; set; } = 0;
}
