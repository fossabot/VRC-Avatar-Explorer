using Avalonia.Interactivity;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void AddCustomCategory_Show()
    {
        AddCustomCategory_CustomCategoryTextBox.Text = string.Empty;
        AddCustomCategoryOverlay.IsVisible = true;
    }
    private void AddCustomCategory_Hide()
        => AddCustomCategoryOverlay.IsVisible = false;

    #region Event Handler
    private void AddCustomCategory_Add_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategoryOverlay.IsVisible = false;

        if (!string.IsNullOrEmpty(AddCustomCategory_CustomCategoryTextBox.Text))
        {
            int index = AddItemOverlay_ItemTypeComboBox.Items.Add(AddCustomCategory_CustomCategoryTextBox.Text);
            AddItemOverlay_ItemTypeComboBox.SelectedIndex = index;
        }
    }
    private void AddCustomCategory_Border_Click(object? sender, RoutedEventArgs e)
        => AddCustomCategory_Hide();
    private void AddCustomCategory_Cancel_Click(object? sender, RoutedEventArgs e)
        => AddCustomCategory_Hide();
    #endregion
}
