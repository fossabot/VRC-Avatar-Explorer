using Avalonia.Interactivity;

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
        if (_contextMenu_selectedItem != null)
        {
            _contextMenu_selectedItem.ItemMemo = AddMemoOverlay_MemoTextBox.Text ?? string.Empty;
            _avatarExplorerApp.SaveItemDatabase();
        }

        AddMemoOverlay_Hide();
        Main_ReloadCurrentWindow();
    }
    #endregion
}
