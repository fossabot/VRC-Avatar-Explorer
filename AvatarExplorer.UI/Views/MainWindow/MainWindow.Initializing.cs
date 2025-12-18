using Avalonia.Controls;
using Avalonia.Layout;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void InitializeAvatarExplorer()
    {
        try
        {
            _avatarExplorerApp.LoadItemDatabase(true); // TODO: V1のデータを読み込む設定になっているのでリリース時に直す
            _avatarExplorerApp.LoadCommonAvatarDatabase(true);
            _avatarExplorerApp.LoadRuntimeSettings();
            ApplyRuntimeSettingsToUi(); // 並び替え順をセットするため
        }
        catch
        {
            // Ignored
        }
    }
    private void InitializeUserPreferences()
    {
        var userPreferences = UserPreferencesService.LoadUserPreferences(SystemPath.UserPreferencesFilePath);
        _userPreferences.FromOther(userPreferences);

        ApplyPreferenceSettingsToUi();

        UserPreferencesService.SaveUserPreferences(_userPreferences);
    }
    private void InitializeNoItemsLabel()
    {
        if (Main_RightPanelParent == null) return;

        Main_RightPanelParent.Children.Clear();

        Main_RightPanelParent.Children.Add(new Image
        {
            Source = ImageService.GetImage(SystemIconKey.EmptyIcon),
            Width = 150,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        });

        Main_RightPanelParent.Children.Add(new TextBlock
        {
            Text = Localizer.Instance[LocalizationKey.Error.Nothing],
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 25
        });
    }
    private void InitializeContextMenuHandlers()
    {
        _contextMenuHandlers = new()
        {
            { ActionKey.OpenItemFolder, ItemButton_ContextMenu_OpenItemFolder },
            { ActionKey.CopyBoothLink, ItemButton_ContextMenu_CopyBoothLink },
            { ActionKey.OpenBoothLink, ItemButton_ContextMenu_OpenBoothLink },
            { ActionKey.ShowOtherItemsByAuthor, ItemButton_ContextMenu_ShowOtherItemsByAuthor },
            { ActionKey.ChangeThumbnail, ItemButton_ContextMenu_ChangeThumbnail },
            { ActionKey.EditItem, ItemButton_ContextMenu_EditItem },
            { ActionKey.AddItemMemo, ItemButton_ContextMenu_AddMemo},
            { ActionKey.AddItemFolder, ItemButton_ContextMenu_AddItemFolder },
            { ActionKey.EditImplementedAvatar, ItemButton_ContextMenu_EditImplementedAvatar },
            { ActionKey.EditItemTag, ItemButton_ContextMenu_EditItemTag },
            { ActionKey.RemoveItem, ItemButton_ContextMenu_RemoveItem }
        };
    }
    private void InitializeLanguageBox()
    {
        Localizer.Instance.LoadFromFolder("locales");

        Main_LanguageComboBox.Items.Clear();
        SettingsOverlay_DefaultLanguageComboBox.Items.Clear();

        foreach (string language in Localizer.Instance.GetLanguageList())
        {
            Main_LanguageComboBox.Items.Add(language);
            SettingsOverlay_DefaultLanguageComboBox.Items.Add(language);
        }
    }
}
