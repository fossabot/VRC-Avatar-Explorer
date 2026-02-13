using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models.Items;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.Items;

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

            string avatarId = string.Empty;

            if (itemCountInfo.Item is Item item) avatarId = item.Id;
            else if (itemCountInfo.Item is CommonAvatar commonAvatar) avatarId = commonAvatar.GetInternalId();

            if (!string.IsNullOrEmpty(avatarId) && _editSupportedAvatarsOverlay_selectedAvatars.Contains(avatarId)) button.Classes.Add("accent");
        }
    }

    #region Event Handler
    private void EditSupportedAvatarsOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => EditSupportedAvatarsOverlay_Hide();
    private void EditSupportedAvatarsOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Item? item = _avatarExplorerApp.GetItemById(_contextMenu_selectedItemId);
        if (item == null)
        {
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.ItemNotFound]);
            return;
        }
    
        item.UpdateSupportedAvatars(_editSupportedAvatarsOverlay_selectedAvatars);
        _avatarExplorerApp.UpdateSearchIndex(item.Id);
        _avatarExplorerApp.SaveItemDatabase();

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
