using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private async void SettingsOverlay_OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, "フォルダを選択してください", false);
        if (folders == null || folders.Length == 0) return;

        SettingsOverlay_ItemsFolderPathTextBox.Text = folders[0];
    }
    
    private void SettingsOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => SettingsOverlay.IsVisible = false;
    private void SettingsOverlay_Close_Click(object? sender, RoutedEventArgs e)
        => SettingsOverlay.IsVisible = false;
    private void SettingsOverlay_Apply_Click(object? sender, RoutedEventArgs e)
    {
        ApplySettingsValues();
        Main_ReloadCurrentWindow();
    }
    
    #region Methods
    private void SetUiValueFromCurrentSettings() // 設定画面を読み込んだ時に値をセットするための関数
    {
        RuntimeSettings runtimeSettings = _avatarExplorer.GetRuntimeSettings();
        UserPreferences userUiPreferences = _userPreferences;

        SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory;
        SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;
        SettingsOverlay_ItemsPerPageTextBox.Text = userUiPreferences.ItemsPerPage.ToString();
        SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userUiPreferences.Theme;
        SettingsOverlay_DefaultLanguageComboBox.SelectedIndex = userUiPreferences.DefaultLanguage;
        SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
    }
    private void ApplySettingsValues() // 設定の適用ボタンが押されたときのみ
    {
        _avatarExplorer.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? "");
        _avatarExplorer.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _avatarExplorer.SetRemoveOriginal(SettingsOverlay_RemoveOriginalCheckBox.IsChecked ?? false);
        _userPreferences.SetItemsPerPage(int.TryParse(SettingsOverlay_ItemsPerPageTextBox.Text, out var result) ? result : 30);
        _userPreferences.SetTheme((Theme)SettingsOverlay_ThemeComboBox.SelectedIndex);
        _userPreferences.SetLanguage(SettingsOverlay_DefaultLanguageComboBox.SelectedIndex);
        _avatarExplorer.SetItemsSortOrder((SortOrder)SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex);

        ApplyPreferenceSettingsToUi();
        ApplyRuntimeSettingsToUi();

        // 適用時は自動で保存する
        _avatarExplorer.SaveRuntimeSettings();
        UserPreferencesService.SaveUserPreferences(_userPreferences);
    }
    
    private void ApplyPreferenceSettingsToUi()
    {
        Application? currentApplication = Application.Current;
        if (currentApplication != null)
        {
            /*
                これも設定する
                TransparencyLevelHint="AcrylicBlur"
                Background="Transparent"
            */

            if (_userPreferences.Theme == Models.Theme.Auto) currentApplication.RequestedThemeVariant = ThemeVariant.Default;
            else if (_userPreferences.Theme == Models.Theme.Dark) currentApplication.RequestedThemeVariant = ThemeVariant.Dark;
            else if (_userPreferences.Theme == Models.Theme.Light) currentApplication.RequestedThemeVariant = ThemeVariant.Light;
        }
        
        Main_LanguageComboBox.SelectedIndex = _userPreferences.DefaultLanguage;
    }
    private void ApplyRuntimeSettingsToUi()
    {
        Main_SortOrderComboBox.SelectedIndex = (int)_avatarExplorer.GetRuntimeSettings().ItemSortOrder;
    }
    #endregion
}
