using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
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
            SettingsOverlay_ApplyRuntimeSettingsToUi(); // 並び替え順をセットするため
        }
        catch
        {
            // Ignored
        }
    }
    private void InitializeUserPreferences()
    {
        var userPreferences = UserPreferencesService.Load(SystemPath.UserPreferencesFilePath);
        _userPreferences.FromOther(userPreferences);

        SettingsOverlay_ApplyPreferenceSettingsToUi();

        UserPreferencesService.Save(_userPreferences);
    }
    private void InitializeNoItemsLabel()
    {
        if (Main_RightPanelParent == null) return;

        Main_RightPanelParent.Children.Clear();

        Main_RightPanelParent.Children.Add(new Image
        {
            Source = ImageService.Get(SystemIconKey.EmptyIcon),
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
    private void InitializeCurrentPath()
    {
        string? currentDirectory = Path.GetDirectoryName(ProcessUtils.GetCurrentProcessPath());
        if (currentDirectory != null) Directory.SetCurrentDirectory(currentDirectory);
    }
    private void InitializePipeServer()
    {
        SingleInstanceService.OnPipeMessageReceived += (_, args) => OnPipeMessageReceived(args);
        SingleInstanceService.StartServer();
    }
    private void OnPipeMessageReceived(string[] args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Activate();
            SetArgs(args);
        });
    }

    private void CheckScheme()
    {
        if (SchemeService.IsSchemeRegistered())
        {
            YesNoDialog_onYesClick += Main_RegisterScheme_Click;
            YesNoDialog_onNoClick += Main_SkipScheme_Click;

            string? currentInternalSchemePath = SchemeService.GetInternalSchemePath();
            
            if (!string.IsNullOrEmpty(currentInternalSchemePath) && !SchemeService.IsSkipped(currentInternalSchemePath) && currentInternalSchemePath != ProcessUtils.GetCurrentProcessPath())
            {
                YesNoDialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.PathChanged]);
            }
            else if (string.IsNullOrEmpty(currentInternalSchemePath))
            {
                YesNoDialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterAgain]);
            }
        }
        else
        {
            YesNoDialog_onYesClick += Main_RegisterScheme_Click;
            YesNoDialog_onNoClick += Main_SkipScheme_Click;

            YesNoDialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.Register]);
        }
    }

    private void Main_RegisterScheme_Click(object? s, RoutedEventArgs e)
    {
        Main_ResetSchemeDialogEvents();

        if (!SchemeService.IsRunAsAdmin())
        {
            YesNoDialog_onYesClick += Main_RestartAsAdmin_Click;
            YesNoDialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RestartAsAdmin]);
        }
        else
        {
            SchemeService.RegisterScheme();
            Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterSuccess]);
        }
    }
    private void Main_SkipScheme_Click(object? s, RoutedEventArgs e)
    {
        Main_ResetSchemeDialogEvents();

        SchemeService.MarkSchemeSkipped();
        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterSkipped]);
    }
    private void Main_ResetSchemeDialogEvents()
    {
        YesNoDialog_onYesClick -= Main_RegisterScheme_Click;
        YesNoDialog_onNoClick -= Main_SkipScheme_Click;
    }
    private void Main_RestartAsAdmin_Click(object? s, RoutedEventArgs e)
    {
        YesNoDialog_onYesClick -= Main_RestartAsAdmin_Click;
        SchemeService.RestartAsAdmin();
    }
}
