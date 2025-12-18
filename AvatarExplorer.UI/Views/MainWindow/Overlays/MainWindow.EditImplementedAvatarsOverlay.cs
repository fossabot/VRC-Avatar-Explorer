using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal readonly List<string> _editImplementedAvatarsOverlay_selectedAvatars = new();
    
    internal void EditImplementedAvatarsOverlay_Show(List<string>? avatars = null)
    {
        EditImplementedAvatarsOverlay.IsVisible = true;
        EditImplementedAvatarsOverlay_InitializeList(avatars);
    }
    internal void EditImplementedAvatarsOverlay_InitializeList(List<string>? avatars = null)
    {
        _editImplementedAvatarsOverlay_selectedAvatars.Clear();
        if (avatars != null) _editImplementedAvatarsOverlay_selectedAvatars.AddRange(avatars);
        EditImplementedAvatarsOverlay_RefleshList();
    }
    internal void EditImplementedAvatarsOverlay_RefleshList()
    {
        EditImplementedAvatarsOverlay_AvatarsList.Children.Clear();
        IEnumerable<ItemCountInfo> avatars = _avatarExplorerApp.GetAvatars().Where(i => string.IsNullOrEmpty(EditImplementedAvatarsOverlay_SearchTextBox.Text) || ((Item)i.Item).Title.Contains(EditImplementedAvatarsOverlay_SearchTextBox.Text));

        foreach (ItemCountInfo itemCountInfo in avatars)
        {
            Button button = ItemButtonFactory.AddItemButton(EditImplementedAvatarsOverlay_AvatarsList, new UISelectableItem(itemCountInfo), RuntimeSettings.RemoveBrackets, onClick: EditImplementedAvatarsOverlay_ItemButton_Click);
            button.Margin = new Thickness(0, 0, 10, 0); // 通常のリスト用のMarginなのでそれを直す。AddItemButtonの中身でMarginが指定されてるのもどうかと思うけどね。

            if (_editImplementedAvatarsOverlay_selectedAvatars.Contains(((Item)itemCountInfo.Item).ItemPath)) button.Background = new SolidColorBrush(Colors.Green);
        }
    }
    internal void EditImplementedAvatarsOverlay_CloseInternal()
        => EditImplementedAvatarsOverlay.IsVisible = false;

    private void EditImplementedAvatarsOverlay_Cancel_Click(object? sender, RoutedEventArgs e)
        => EditImplementedAvatarsOverlay_CloseInternal();
    private void EditImplementedAvatarsOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => EditImplementedAvatarsOverlay_CloseInternal();
    private void EditImplementedAvatarsOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_contextMenu_selectedItem != null)
        {
            _contextMenu_selectedItem.ImplementedAvatars.Clear();
            _contextMenu_selectedItem.ImplementedAvatars.AddRange(_editImplementedAvatarsOverlay_selectedAvatars);
        }

        EditImplementedAvatarsOverlay_CloseInternal();
    }

    private void EditImplementedAvatarsOverlay_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ItemTagInfo itemTagInfo) return;
        
        if (_editImplementedAvatarsOverlay_selectedAvatars.Contains(itemTagInfo.Value)) _editImplementedAvatarsOverlay_selectedAvatars.RemoveAll(i => i == itemTagInfo.Value);
        else _editImplementedAvatarsOverlay_selectedAvatars.Add(itemTagInfo.Value);
        
        EditImplementedAvatarsOverlay_RefleshList();
    }

    private void EditImplementedAvatarsOverlay_SearchTextBox_Changed(object? sender, RoutedEventArgs e)
        => EditImplementedAvatarsOverlay_RefleshList();
}
