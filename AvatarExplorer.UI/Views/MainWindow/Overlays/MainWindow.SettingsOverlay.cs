using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

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
        SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory;
        SettingsOverlay_LanguageComboBox.SelectedIndex = userPreferences.Language;
        SettingsOverlay_SortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
        SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userPreferences.Theme;

        // 表示
        SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        SettingsOverlay_NormalIconSizeSlider.Value = userPreferences.NormalIconSize;
        SettingsOverlay_EnableHoverIconSizeCheckBox.IsChecked = userPreferences.EnableHoverIconSize;
        SettingsOverlay_HoverIconSizeSlider.Value = userPreferences.HoverIconSize;
        SettingsOverlay_AntiAliasingModeComboBox.SelectedIndex = (int)userPreferences.AntiAliasingMode;
        SettingsOverlay_ItemsPerPageTextBox.Text = userPreferences.ItemsPerPage.ToString();

        // アイテム
        SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;

        // 背景
        SettingsOverlay_UseBackgroundImageCheckBox.IsChecked = userPreferences.UseBackgroundImage;
        SettingsOverlay_BackgroundImagePathTextBox.Text = userPreferences.BackgroundImage;
        SettingsOverlay_BackgroundImageOpacitySlider.Value = userPreferences.BackgroundOpacity;
        
        // データ
        SettingsOverlay_AutoBackupPathTextBox.Text = runtimeSettings.AutoBackupRootDirectory;
        SettingsOverlay_AutoBackupIntervalTextBox.Text = runtimeSettings.AutoBackupInterval.ToString();
    }
    private void SettingsOverlay_ApplySettingsValues()
    {
        // 基本
        _avatarExplorerApp.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? string.Empty);
        _userPreferences.SetLanguage(SettingsOverlay_LanguageComboBox.SelectedIndex);
        _avatarExplorerApp.SetItemsSortOrder((SortOrder)SettingsOverlay_SortOrderComboBox.SelectedIndex);
        _userPreferences.SetTheme((Theme)SettingsOverlay_ThemeComboBox.SelectedIndex);

        // 表示
        _avatarExplorerApp.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _userPreferences.SetIconSize((int)SettingsOverlay_NormalIconSizeSlider.Value, (int)SettingsOverlay_HoverIconSizeSlider.Value);
        _userPreferences.UseHoverIconSize(SettingsOverlay_EnableHoverIconSizeCheckBox.IsChecked ?? false);
        _userPreferences.SetAntialiasing((BitmapAntiAliasingMode)SettingsOverlay_AntiAliasingModeComboBox.SelectedIndex);
        _userPreferences.SetItemsPerPage(int.TryParse(SettingsOverlay_ItemsPerPageTextBox.Text, out var count) ? count : 30);
        _avatarExplorerApp.SetRemoveOriginal(SettingsOverlay_RemoveOriginalCheckBox.IsChecked ?? false);

        // 背景
        _userPreferences.UseBackground(SettingsOverlay_UseBackgroundImageCheckBox.IsChecked ?? false);
        _userPreferences.SetBackground(SettingsOverlay_BackgroundImagePathTextBox.Text ?? string.Empty);
        _userPreferences.SetBackgroundOpacity(Math.Clamp((int)SettingsOverlay_BackgroundImageOpacitySlider.Value, 0, 100));

        // データ
        _avatarExplorerApp.SetAutoBackupRootDirectory(SettingsOverlay_AutoBackupPathTextBox.Text ?? string.Empty);
        _avatarExplorerApp.SetAutoBackupInterval(int.TryParse(SettingsOverlay_AutoBackupIntervalTextBox.Text, out var interval) ? interval : 5);

        SettingsOverlay_ApplyPreferenceSettingsToUi();
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

        if (theme == Models.Theme.Dark) application.RequestedThemeVariant = ThemeVariant.Dark;
        else if (theme == Models.Theme.Light) application.RequestedThemeVariant = ThemeVariant.Light;
    }
    private void SettingsOverlay_SetBackground(Theme theme)
    {
        if (theme == Models.Theme.Dark) Background = new SolidColorBrush(new Color(255, 32, 32, 32));
        else if (theme == Models.Theme.Light) Background = new SolidColorBrush(new Color(255, 249, 249, 249));
    }
    private void SettingsOverlay_ApplyBackgroundImage(UserPreferences userPreferences)
    {
        if (userPreferences.UseBackgroundImage && File.Exists(userPreferences.BackgroundImage))
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

        InitializeNoItemsLabel();
        Main_ReloadCurrentWindow();
    }

    #region Event Handler
    private async void SettingsOverlay_OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false);
        if (folders == null || folders.Length == 0) return;

        SettingsOverlay_ItemsFolderPathTextBox.Text = folders[0];
    }
    private async void SettingsOverlay_OpenBackgroundFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFilePath], false);
        if (files == null || files.Length == 0) return;

        SettingsOverlay_BackgroundImagePathTextBox.Text = files[0];
    }
    private async void SettingsOverlay_OpenAutoBackupRootFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false, RuntimeSettings.AutoBackupRootDirectory);
        if (folders == null || folders.Length == 0) return;

        SettingsOverlay_AutoBackupPathTextBox.Text = folders[0];
    }
    
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
        _avatarExplorerApp.SaveItemDatabase();
        Main_ReloadCurrentWindow();
    }
    
    private async void SettingsOverlay_ResetCommonAvatarDatabase_Click(object? sender, RoutedEventArgs e)
    {
        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.ResetCommonAvatarDatabase]);
        if (result != YesNoResult.Yes) return;

        _avatarExplorerApp.ResetCommonAvatarDatabase();
        _avatarExplorerApp.SaveCommonAvatarDatabase();
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
    #endregion
}
