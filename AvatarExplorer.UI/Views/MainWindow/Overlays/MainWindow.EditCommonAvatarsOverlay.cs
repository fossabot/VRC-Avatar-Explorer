using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private CommonAvatar? _editCommonAvatarsOverlay_SelectedGroup = null;

    private void EditCommonAvatarsOverlay_Show()
    {
        EditCommonAvatarsOverlay.IsVisible = true;

        EditCommonAvatarsOverlay_RefleshGroupList();
        EditCommonAvatarsOverlay_RefleshAvatarList();

        if (EditCommonAvatarsOverlay_GroupComboBox.Items.Count > 0) EditCommonAvatarsOverlay_GroupComboBox.SelectedIndex = 0;
    }
    private void EditCommonAvatarsOverlay_Hide() => EditCommonAvatarsOverlay.IsVisible = false;

    private void EditCommonAvatarsOverlay_RefleshGroupList()
    {
        EditCommonAvatarsOverlay_GroupComboBox.Items.Clear();
        foreach (CommonAvatar commonAvatar in _avatarExplorerApp.GetCommonAvatars())
        {
            EditCommonAvatarsOverlay_GroupComboBox.Items.Add(commonAvatar.GroupName);
        }
    }

    private void EditCommonAvatarsOverlay_RefleshAvatarList()
    {
        if (EditCommonAvatarsOverlay_AvatarsList == null) return;
        EditCommonAvatarsOverlay_AvatarsList.Children.Clear();
        IEnumerable<ItemCountInfo> avatars = _avatarExplorerApp.GetAvatars().Where(i => string.IsNullOrEmpty(EditCommonAvatarsOverlay_SearchTextBox.Text) || ((Item)i.Item).Title.Contains(EditCommonAvatarsOverlay_SearchTextBox.Text));

        foreach (ItemCountInfo itemCountInfo in avatars)
        {
            Button button = ItemButtonFactory.AddItemButton(EditCommonAvatarsOverlay_AvatarsList, new UISelectableItem(itemCountInfo), RuntimeSettings, _userPreferences, onClick: EditCommonAvatarsOverlay_ItemButton_Click);
            button.Margin = new Thickness(0, 0, 10, 0); // 通常のリスト用のMarginなのでそれを直す。AddItemButtonの中身でMarginが指定されてるのもどうかと思うけどね。

            if (_editCommonAvatarsOverlay_SelectedGroup?.AvatarsView.Contains(((Item)itemCountInfo.Item).ItemPath) ?? false) button.Classes.Add("selected");
        }
    }

    #region Event Handler
    private void EditCommonAvatarsOverlay_Close_Click(object? sender, RoutedEventArgs e) => EditCommonAvatarsOverlay_Hide();
    private void EditCommonAvatarsOverlay_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_editCommonAvatarsOverlay_SelectedGroup == null) return;
        if (sender is not Button button || button.Tag is not ItemTagInfo itemTagInfo) return;
        
        if (_editCommonAvatarsOverlay_SelectedGroup.AvatarsView.Contains(itemTagInfo.Value)) _editCommonAvatarsOverlay_SelectedGroup.UpdateAvatars(_editCommonAvatarsOverlay_SelectedGroup.AvatarsView.Where(i => i != itemTagInfo.Value));
        else _editCommonAvatarsOverlay_SelectedGroup.UpdateAvatars(_editCommonAvatarsOverlay_SelectedGroup.AvatarsView.Append(itemTagInfo.Value));
        
        EditCommonAvatarsOverlay_RefleshAvatarList();
    }
    private async void EditCommonAvatarsOverlay_AddGroup_Click(object? sender, RoutedEventArgs e)
    {
        string? commonAvatarGroupName = await Main_ShowTextDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Title.AddCommonAvatarGroup]);
        if (string.IsNullOrEmpty(commonAvatarGroupName)) return;

        CommonAvatar? result = _avatarExplorerApp.AddCommonAvatar(commonAvatarGroupName, []);
        if (result == null) return;
        
        EditCommonAvatarsOverlay_RefleshGroupList();

        // 追加された共通素体グループを選択してあげる
        int index = EditCommonAvatarsOverlay_GroupComboBox.Items.IndexOf(result.GroupName);
        if (index != -1) EditCommonAvatarsOverlay_GroupComboBox.SelectedIndex = index;

        EditCommonAvatarsOverlay_RefleshAvatarList();
    }
    private async void EditCommonAvatarsOverlay_RenameGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (_editCommonAvatarsOverlay_SelectedGroup == null)
        {
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.GetCommonAvatarGroup]);
            return;
        }

        string previousInternalGroupPath = _editCommonAvatarsOverlay_SelectedGroup.GetInternalPath();

        string? commonAvatarGroupName = await Main_ShowTextDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Title.NewCommonAvatarGroupName], _editCommonAvatarsOverlay_SelectedGroup.GroupName);
        if (string.IsNullOrEmpty(commonAvatarGroupName)) return;

        _editCommonAvatarsOverlay_SelectedGroup.GroupName = commonAvatarGroupName;
        string newInternalGroupPath = _editCommonAvatarsOverlay_SelectedGroup.GetInternalPath();

        _avatarExplorerApp.RenameCommonAvatarGroupName(previousInternalGroupPath, newInternalGroupPath); // TODO: CommonAvatarのクラスはRecordでやってしまってもいいかも
        
        int previousIndex = EditCommonAvatarsOverlay_GroupComboBox.SelectedIndex;
        EditCommonAvatarsOverlay_RefleshGroupList();
        if (previousIndex != -1) EditCommonAvatarsOverlay_GroupComboBox.SelectedIndex = previousIndex;
        EditCommonAvatarsOverlay_RefleshAvatarList();
    }
    private async void EditCommonAvatarsOverlay_RemoveGroup_Click(object? sender, RoutedEventArgs e)
    {
        if (_editCommonAvatarsOverlay_SelectedGroup == null)
        {
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.GetCommonAvatarGroup]);
            return;
        }

        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance.GetDisplayName(LocalizationKey.UI.Dialog.Confirmation.RemoveCommonAvatarGroup, _editCommonAvatarsOverlay_SelectedGroup.GroupName));
        if (result != YesNoResult.Yes) return;

        _avatarExplorerApp.RemoveCommonAvatar(_editCommonAvatarsOverlay_SelectedGroup.GroupName);
        
        EditCommonAvatarsOverlay_RefleshGroupList();
        if (EditCommonAvatarsOverlay_GroupComboBox.Items.Count > 0) EditCommonAvatarsOverlay_GroupComboBox.SelectedIndex = 0;
        EditCommonAvatarsOverlay_RefleshAvatarList();
    }
    private void EditCommonAvatarsOverlay_GroupComboBox_Changed(object? sender, RoutedEventArgs e)
    {
        _editCommonAvatarsOverlay_SelectedGroup = _avatarExplorerApp.GetCommonAvatars().FirstOrDefault(i => i.GroupName == EditCommonAvatarsOverlay_GroupComboBox.SelectedItem?.ToString());
        EditCommonAvatarsOverlay_RefleshAvatarList();
    }
    private void EditCommonAvatarsOverlay_SearchTextBox_Changed(object? sender, RoutedEventArgs e)
        => EditCommonAvatarsOverlay_RefleshAvatarList();
    #endregion
}
