using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private readonly List<string> _editSupportedAvatarsOverlay_selectedAvatars = new();
    
    private void EditSupportedAvatarsOverlay_Show(IReadOnlyList<string>? avatars = null)
    {
        EditSupportedAvatarsOverlay.IsVisible = true;
        EditSupportedAvatarsOverlay_InitializeList(avatars);
    }
    private void EditSupportedAvatarsOverlay_Hide() => EditSupportedAvatarsOverlay.IsVisible = false;
    
    private void EditSupportedAvatarsOverlay_InitializeList(IReadOnlyList<string>? avatars = null)
    {
        _editSupportedAvatarsOverlay_selectedAvatars.Clear();
        if (avatars != null) _editSupportedAvatarsOverlay_selectedAvatars.AddRange(avatars);
        EditSupportedAvatarsOverlay_RefleshList();
    }
    private void EditSupportedAvatarsOverlay_RefleshList()
    {
        EditSupportedAvatarsOverlay_AvatarsList.Children.Clear();
        IEnumerable<ItemCountInfo> avatars = _avatarExplorerApp.GetAvatars(includeCommonAvatar: true).Where(i => string.IsNullOrEmpty(EditSupportedAvatarsOverlay_SearchTextBox.Text) || (i.Item is Item item && item.Title.Contains(EditSupportedAvatarsOverlay_SearchTextBox.Text)) || (i.Item is CommonAvatar commonAvatar && commonAvatar.GroupName.Contains(EditSupportedAvatarsOverlay_SearchTextBox.Text)));

        foreach (ItemCountInfo itemCountInfo in avatars)
        {
            Button button = ItemButtonFactory.AddItemButton(EditSupportedAvatarsOverlay_AvatarsList, new UISelectableItem(itemCountInfo), RuntimeSettings, _userPreferences, onClick: EditSupportedAvatarsOverlay_ItemButton_Click);
            button.Margin = new Thickness(0, 0, 10, 0); // 通常のリスト用のMarginなのでそれを直す。AddItemButtonの中身でMarginが指定されてるのもどうかと思うけどね。

            string avatarPath = string.Empty;
            
            if (itemCountInfo.Item is Item item) avatarPath = item.ItemPath;
            else if (itemCountInfo.Item is CommonAvatar commonAvatar) avatarPath = commonAvatar.GetInternalPath();

            if (!string.IsNullOrEmpty(avatarPath) && _editSupportedAvatarsOverlay_selectedAvatars.Contains(avatarPath)) button.Classes.Add("selected");
        }
    }

    #region Event Handler
    private void EditSupportedAvatarsOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => EditSupportedAvatarsOverlay_Hide();
    private void EditSupportedAvatarsOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        _addItemOverlay_selectedItem?.UpdateSupportedAvatars(_editSupportedAvatarsOverlay_selectedAvatars);

        AddItemOverlay_UpdateSupportedAvatarsLabel();

        EditSupportedAvatarsOverlay_Hide();
    }
    private void EditSupportedAvatarsOverlay_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ItemTagInfo itemTagInfo) return;
        
        if (_editSupportedAvatarsOverlay_selectedAvatars.Contains(itemTagInfo.Value)) _editSupportedAvatarsOverlay_selectedAvatars.RemoveAll(i => i == itemTagInfo.Value);
        else _editSupportedAvatarsOverlay_selectedAvatars.Add(itemTagInfo.Value);
        
        EditSupportedAvatarsOverlay_RefleshList();
    }
    private void EditSupportedAvatarsOverlay_SearchTextBox_Changed(object? sender, RoutedEventArgs e) => EditSupportedAvatarsOverlay_RefleshList();
    #endregion
}
