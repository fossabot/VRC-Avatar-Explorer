using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models.Items;
using AvatarExplorer.Core.Models.System;
using AvatarExplorer.Core.Services.IO;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.Common;
using AvatarExplorer.UI.Models.Settings;
using AvatarExplorer.UI.Services.System;
using AvatarExplorer.UI.Services.Utilities;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void SettingsOverlay_Show()
    {
        SettingsOverlay_SetUiValueFromCurrentSettings();
        SettingsOverlay.IsVisible = true;
    }
    private void SettingsOverlay_Hide() => SettingsOverlay.IsVisible = false;

    private void SettingsOverlay_SetUiValueFromCurrentSettings()
    {
        RuntimeSettings runtimeSettings = _avatarExplorerApp.GetRuntimeSettings();
        UserPreferences userPreferences = _userPreferences;

        // 基本
        if (SettingsOverlay_ItemsFolderPathTextBox != null) SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory ?? string.Empty;
        
        if (SettingsOverlay_LanguageComboBox != null)
        {
            SettingsOverlay_LanguageComboBox.SelectedIndex = -1;
            SettingsOverlay_LanguageComboBox.SelectedIndex = userPreferences.Language;
        }

        if (SettingsOverlay_SortOrderComboBox != null)
        {
            SettingsOverlay_SortOrderComboBox.SelectedIndex = -1;
            SettingsOverlay_SortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
        }

        if (SettingsOverlay_ThemeComboBox != null)
        {
            SettingsOverlay_ThemeComboBox.SelectedIndex = -1;
            SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userPreferences.Theme;
        }

        // 表示
        if (SettingsOverlay_RemoveBracketsCheckBox != null) SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        if (SettingsOverlay_NormalIconSizeSlider != null) SettingsOverlay_NormalIconSizeSlider.Value = userPreferences.NormalIconSize;
        if (SettingsOverlay_EnableHoverIconSizeCheckBox != null) SettingsOverlay_EnableHoverIconSizeCheckBox.IsChecked = userPreferences.EnableHoverIconSize;
        if (SettingsOverlay_HoverIconSizeSlider != null) SettingsOverlay_HoverIconSizeSlider.Value = userPreferences.HoverIconSize;
        if (SettingsOverlay_AntiAliasingModeComboBox != null) SettingsOverlay_AntiAliasingModeComboBox.SelectedIndex = (int)userPreferences.AntiAliasingMode;
        if (SettingsOverlay_ItemsPerPageTextBox != null) SettingsOverlay_ItemsPerPageTextBox.Text = userPreferences.ItemsPerPage.ToString();

        // アイテム
        if (SettingsOverlay_RemoveOriginalCheckBox != null) SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;

        // 背景
        if (SettingsOverlay_UseBackgroundImageCheckBox != null) SettingsOverlay_UseBackgroundImageCheckBox.IsChecked = userPreferences.UseBackgroundImage;
        if (SettingsOverlay_BackgroundImagePathTextBox != null) SettingsOverlay_BackgroundImagePathTextBox.Text = userPreferences.BackgroundImage ?? string.Empty;
        if (SettingsOverlay_BackgroundImageOpacitySlider != null) SettingsOverlay_BackgroundImageOpacitySlider.Value = userPreferences.BackgroundOpacity;

        // データ
        if (SettingsOverlay_AutoBackupPathTextBox != null) SettingsOverlay_AutoBackupPathTextBox.Text = runtimeSettings.AutoBackupRootDirectory ?? string.Empty;
        if (SettingsOverlay_AutoBackupIntervalTextBox != null) SettingsOverlay_AutoBackupIntervalTextBox.Text = runtimeSettings.AutoBackupInterval.ToString();
    }
    private void SettingsOverlay_ApplySettingsValues()
    {
        string previousDataRootDirectoryPath = RuntimeSettings.DataRootDirectory;

        // 基本
        if (SettingsOverlay_ItemsFolderPathTextBox != null) _avatarExplorerApp.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? string.Empty);
        if (SettingsOverlay_LanguageComboBox != null) _userPreferences.SetLanguage(SettingsOverlay_LanguageComboBox.SelectedIndex);
        if (SettingsOverlay_SortOrderComboBox != null) _avatarExplorerApp.SetItemsSortOrder((ItemSortOrder)SettingsOverlay_SortOrderComboBox.SelectedIndex);
        if (SettingsOverlay_ThemeComboBox != null) _userPreferences.SetTheme((Theme)SettingsOverlay_ThemeComboBox.SelectedIndex);

        // 表示
        if (SettingsOverlay_RemoveBracketsCheckBox != null) _avatarExplorerApp.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        if (SettingsOverlay_NormalIconSizeSlider != null && SettingsOverlay_HoverIconSizeSlider != null) _userPreferences.SetIconSize((int)SettingsOverlay_NormalIconSizeSlider.Value, (int)SettingsOverlay_HoverIconSizeSlider.Value);
        if (SettingsOverlay_EnableHoverIconSizeCheckBox != null) _userPreferences.UseHoverIconSize(SettingsOverlay_EnableHoverIconSizeCheckBox.IsChecked ?? false);
        if (SettingsOverlay_AntiAliasingModeComboBox != null) _userPreferences.SetAntialiasing((BitmapAntiAliasingMode)SettingsOverlay_AntiAliasingModeComboBox.SelectedIndex);
        if (SettingsOverlay_ItemsPerPageTextBox != null) _userPreferences.SetItemsPerPage(ValueParser.Int(SettingsOverlay_ItemsPerPageTextBox.Text, 30));
        if (SettingsOverlay_RemoveOriginalCheckBox != null) _avatarExplorerApp.SetRemoveOriginal(SettingsOverlay_RemoveOriginalCheckBox.IsChecked ?? false);

        // 背景
        if (SettingsOverlay_UseBackgroundImageCheckBox != null) _userPreferences.UseBackground(SettingsOverlay_UseBackgroundImageCheckBox.IsChecked ?? false);
        if (SettingsOverlay_BackgroundImagePathTextBox != null) _userPreferences.SetBackground(SettingsOverlay_BackgroundImagePathTextBox.Text ?? string.Empty);
        if (SettingsOverlay_BackgroundImageOpacitySlider != null) _userPreferences.SetBackgroundOpacity(Math.Clamp((int)SettingsOverlay_BackgroundImageOpacitySlider.Value, 0, 100));

        // データ
        if (SettingsOverlay_AutoBackupPathTextBox != null) _avatarExplorerApp.SetAutoBackupRootDirectory(SettingsOverlay_AutoBackupPathTextBox.Text ?? string.Empty);
        if (SettingsOverlay_AutoBackupIntervalTextBox != null) _avatarExplorerApp.SetAutoBackupInterval(ValueParser.Int(SettingsOverlay_AutoBackupIntervalTextBox.Text, 5));

        SettingsOverlay_ApplyPreferenceSettingsToUi();
        SettingsOverlay_SetUiValueFromCurrentSettings();

        if (RuntimeSettings.DataRootDirectory != previousDataRootDirectoryPath) _ = SettingsOverlay_CheckDataCopy(previousDataRootDirectoryPath, RuntimeSettings.DataRootDirectory);
    }

    private async Task SettingsOverlay_CheckDataCopy(string previousPath, string currentPath)
    {
        //データをコピーするか
        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.StoragePathChange.CopyData]);
        if (result != YesNoResult.Yes) return;

        // Item1: LocalizationKey, Item2: ProgressValue
        async Task progressAction((string, int) tuple)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressOverlay_Show(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                ProgressOverlay_Update(tuple.Item2);
            });
        }

        await FileSystemService.CopyDirectory(previousPath, currentPath, progressAction);
        ProgressOverlay_Hide();
    }

    private void SettingsOverlay_ApplyPreferenceSettingsToUi()
    {
        SettingsOverlay_SetApplicationTheme(Application.Current, _userPreferences.Theme);
        SettingsOverlay_SetBackground(_userPreferences.Theme);
        SettingsOverlay_ApplyBackgroundImage(_userPreferences);
        SettingsOverlay_ApplyLanguage(_userPreferences.Language);
    }

    private void SettingsOverlay_SetApplicationTheme(Application? application, Theme theme)
    {
        if (application == null) return;

        if (theme == Models.Common.Theme.Dark) application.RequestedThemeVariant = ThemeVariant.Dark;
        else if (theme == Models.Common.Theme.Light) application.RequestedThemeVariant = ThemeVariant.Light;
    }
    private void SettingsOverlay_SetBackground(Theme theme)
    {
        if (theme == Models.Common.Theme.Dark) Background = new SolidColorBrush(new Color(255, 32, 32, 32));
        else if (theme == Models.Common.Theme.Light) Background = new SolidColorBrush(new Color(255, 249, 249, 249));
    }
    private void SettingsOverlay_ApplyBackgroundImage(UserPreferences userPreferences)
    {
        if (userPreferences.UseBackgroundImage && !string.IsNullOrEmpty(userPreferences.BackgroundImage) && File.Exists(userPreferences.BackgroundImage))
        {
            WindowPanel.Background = new ImageBrush()
            {
                Source = new Bitmap(userPreferences.BackgroundImage),
                Opacity = Math.Clamp(userPreferences.BackgroundOpacity / 100.0, 0, 1),
                Stretch = Stretch.UniformToFill
            };
        }
        else
        {
            WindowPanel.Background = null;
        }
    }
    private void SettingsOverlay_ApplyLanguage(int language)
    {
        Localizer.Instance.SetLanguage(language);
        Main_ReloadCurrentWindow();
    }

    #region Event Handler
    private async void SettingsOverlay_OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false);
        if (folders == null || folders.Length == 0) return;

        if (SettingsOverlay_ItemsFolderPathTextBox != null) SettingsOverlay_ItemsFolderPathTextBox.Text = folders[0];
    }
    private async void SettingsOverlay_OpenBackgroundFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFilePath], false);
        if (files == null || files.Length == 0) return;

        if (SettingsOverlay_BackgroundImagePathTextBox != null) SettingsOverlay_BackgroundImagePathTextBox.Text = files[0];
    }
    private async void SettingsOverlay_OpenAutoBackupRootFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false, RuntimeSettings.AutoBackupRootDirectory);
        if (folders == null || folders.Length == 0) return;

        if (SettingsOverlay_AutoBackupPathTextBox != null) SettingsOverlay_AutoBackupPathTextBox.Text = folders[0];
    }
    private async void SettingsOverlay_RegisterScheme_Click(object? sender, RoutedEventArgs e) => await Main_RegisterSchemeAsync();

    private void SettingsOverlay_Close_Click(object? sender, RoutedEventArgs e) => SettingsOverlay_Hide();
    private void SettingsOverlay_Apply_Click(object? sender, RoutedEventArgs e)
    {
        SettingsOverlay_ApplySettingsValues();

        // 適用時は自動で保存する
        _avatarExplorerApp.SaveRuntimeSettings();
        UserPreferencesService.Save(_userPreferences);

        Main_ReloadCurrentWindow();
    }

    private void SettingsOverlay_ImportData_Click(object? sender, RoutedEventArgs e) => SelectImportTypeOverlay_Show();

    private async void SettingsOverlay_ExportDataToCsv_Click(object? sender, RoutedEventArgs e)
    {
        string? filePath = await StorageService.SaveFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectSaveFilePath], "csv");
        if (filePath == null) return;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);

        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.ExportToCsv.IncludeImplementedToSupported]);
        if (result == YesNoResult.Yes) await _avatarExplorerApp.ExportToCsv(filePath, localizedItemTypesMapping, true);
        else await _avatarExplorerApp.ExportToCsv(filePath, localizedItemTypesMapping, false);

        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Export]);
    }
    private async void SettingsOverlay_EditCommonAvatars_Click(object? sender, RoutedEventArgs e) => EditCommonAvatarsOverlay_Show();
    private async void SettingsOverlay_ResetItemDatabase_Click(object? sender, RoutedEventArgs e)
    {
        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.ResetItemDatabase]);
        if (result != YesNoResult.Yes) return;

        _avatarExplorerApp.ResetItemDatabase();
        Main_ReloadCurrentWindow();
    }

    private async void SettingsOverlay_ResetCommonAvatarDatabase_Click(object? sender, RoutedEventArgs e)
    {
        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.ResetCommonAvatarDatabase]);
        if (result != YesNoResult.Yes) return;

        _avatarExplorerApp.ResetCommonAvatarDatabase();
        Main_ReloadCurrentWindow();
    }

    private async void SettingsOverlay_RestoreDataFromBackup_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folderPaths = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false, RuntimeSettings.AutoBackupRootDirectory);
        if (folderPaths == null || folderPaths.Length == 0) return;

        // バックアップを復元する前に、今の状態をバックアップしておく
        await _avatarExplorerApp.ExecuteBackup(RuntimeSettings.AutoBackupRootDirectory);

        string backupRootPath = folderPaths[0];

        string itemDatabasePath = Path.Join(backupRootPath, SystemFileName.Database.Items);
        string commonAvatarDatabasePath = Path.Join(backupRootPath, SystemFileName.Database.CommonAvatars);
        string runtimeSettingsFilePath = Path.Join(backupRootPath, SystemFileName.Settings.Runtime);
        string userPreferencesFilePath = Path.Join(backupRootPath, SystemFileName.Settings.Preferences);

        if (File.Exists(itemDatabasePath))
        {
            _avatarExplorerApp.LoadItemDatabase(itemDatabasePath);
            _avatarExplorerApp.SaveItemDatabase();
        }

        if (File.Exists(commonAvatarDatabasePath))
        {
            _avatarExplorerApp.LoadCommonAvatarDatabase(commonAvatarDatabasePath);
            _avatarExplorerApp.SaveCommonAvatarDatabase();
        }

        if (File.Exists(runtimeSettingsFilePath))
        {
            _avatarExplorerApp.LoadRuntimeSettings(runtimeSettingsFilePath);
            _avatarExplorerApp.SaveRuntimeSettings();
        }

        if (File.Exists(userPreferencesFilePath))
        {
            _userPreferences.FromOther(UserPreferencesService.Load(userPreferencesFilePath));
            UserPreferencesService.Save(_userPreferences);
        }

        SettingsOverlay_SetUiValueFromCurrentSettings();
        SettingsOverlay_ApplySettingsValues();

        Main_ReloadCurrentWindow();
    }
    private async void SettingsOverlay_ShowErrorLog_Click(object? sender, RoutedEventArgs e) => ErrorLogOverlay_Show();
    #endregion
}
