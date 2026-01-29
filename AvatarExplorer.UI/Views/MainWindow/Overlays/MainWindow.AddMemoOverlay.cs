using Avalonia.Interactivity;
using AvatarExplorer.Core.Models.Items;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void AddMemoOverlay_Show(string initialMemo = "")
    {
        AddMemoOverlay.IsVisible = true;
        if (!string.IsNullOrEmpty(initialMemo)) AddMemoOverlay_MemoTextBox.Text = initialMemo;
    }
    private void AddMemoOverlay_Hide() => AddMemoOverlay.IsVisible = false;

    #region Event Handler
    private void AddMemoOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => AddMemoOverlay_Hide();
    private void AddMemoOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Item? item = _avatarExplorerApp.GetItemById(_addItemOverlay_selectedItemId);
        if (item != null)
        {
            item.ItemMemo = AddMemoOverlay_MemoTextBox.Text ?? string.Empty;
            _avatarExplorerApp.UpdateSearchIndex(item.Id);
            _avatarExplorerApp.SaveItemDatabase();
        }

        AddMemoOverlay_Hide();
        Main_ReloadCurrentWindow();
    }
    #endregion
}
