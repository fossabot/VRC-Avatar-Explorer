using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Data;
using AvatarExplorer.UI.Extensions;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.Common;
using AvatarExplorer.UI.Models.ContextMenu;
using AvatarExplorer.UI.Models.Settings;
using AvatarExplorer.UI.Services.System;
using AvatarExplorer.UI.Services.Utilities;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void InitializeTitle()
    {
        Title = string.Format("VRC Avatar Explorer {0}", AvatarExplorerApp.CurrentVersion);
    }
    private void InitializeAvatarExplorer()
    {
        try
        {
            _avatarExplorerApp.LoadItemDatabase();
            _avatarExplorerApp.LoadCommonAvatarDatabase();
            _avatarExplorerApp.LoadRuntimeSettings();
            _avatarExplorerApp.StartAutoBackup();
        }
        catch
        {
            // Ignored
        }
    }
    private void InitializeUserPreferences()
    {
        UserPreferences userPreferences = UserPreferencesService.Load(SystemPath.UserPreferencesFilePath);
        _userPreferences.FromOther(userPreferences);
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
            { ActionKey.FetchThumbnail, ItemButton_ContextMenu_FetchThumbnail },
            { ActionKey.EditItem, ItemButton_ContextMenu_EditItem },
            { ActionKey.EditItemTitle, ItemButton_ContextMenu_EditItemTitle },
            { ActionKey.AddItemMemo, ItemButton_ContextMenu_AddMemo },
            { ActionKey.AddToBulkImportList, ItemButton_ContextMenu_AddToBulkImportList },
            { ActionKey.AddItemFile, ItemButton_ContextMenu_AddItemFile },
            { ActionKey.AddItemFolder, ItemButton_ContextMenu_AddItemFolder },
            { ActionKey.EditImplementedAvatar, ItemButton_ContextMenu_EditImplementedAvatar },
            { ActionKey.EditItemTag, ItemButton_ContextMenu_EditItemTag },
            { ActionKey.RemoveItem, ItemButton_ContextMenu_RemoveItem },
            { ActionKey.OpenFile, ItemButton_ContextMenu_OpenFile },
            { ActionKey.AddFileToBulkImportList, ItemButton_ContextMenu_AddFileToBulkImportList },
            { ActionKey.OpenFileInExplorer, ItemButton_ContextMenu_OpenFileInExplorer }
        };
    }
    private void InitializeLanguageBox()
    {
        Localizer.Instance.LoadFromFolder("locales");

        SettingsOverlay_LanguageComboBox.Items.Clear();
        SettingsOverlay_LanguageComboBox.Items.AddRange(Localizer.Instance.GetLanguageList());
    }
    private void CheckFirstLaunching()
    {
        if (_avatarExplorerApp.GetAllItems().Count != 0) return;
        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.FirstLaunch.DialogTitle], Localizer.Instance[LocalizationKey.UI.Dialog.FirstLaunch.DialogMessage]);
    }
    private void InitializePipeServer()
    {
        SingleInstanceService.OnPipeMessageReceived += OnPipeMessageReceived;
        SingleInstanceService.StartServer();
    }
    private void OnPipeMessageReceived(string[] args)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            Activate();
            await SetApplicationArgs(args);
        });
    }

    private async Task CheckScheme()
    {
        if (SchemeService.IsSchemeRegistered())
        {
            string? currentInternalSchemePath = SchemeService.GetInternalSchemePath();

            if (!string.IsNullOrEmpty(currentInternalSchemePath) && !SchemeService.IsSkipped(currentInternalSchemePath) && currentInternalSchemePath != ProcessUtils.GetCurrentProcessPath())
            {
                YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.PathChanged]);
                if (result == YesNoResult.Yes) await Main_RegisterScheme();
                else Main_SkipScheme();
            }
            else if (string.IsNullOrEmpty(currentInternalSchemePath))
            {
                YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterAgain]);
                if (result == YesNoResult.Yes) await Main_RegisterScheme();
                else Main_SkipScheme();
            }
        }
        else
        {
            YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.Register]);
            if (result == YesNoResult.Yes) await Main_RegisterScheme();
            else Main_SkipScheme();
        }
    }

    private async Task Main_RegisterScheme()
    {
        if (!SchemeService.IsRunAsAdmin())
        {
            YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RestartAsAdmin]);
            if (result == YesNoResult.Yes) SchemeService.RestartAsAdmin();
        }
        else
        {
            SchemeService.RegisterScheme();
            Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterSuccess]);
        }
    }
    private void Main_SkipScheme()
    {
        SchemeService.MarkSchemeSkipped();
        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Scheme.RegisterSkipped]);
    }
}
